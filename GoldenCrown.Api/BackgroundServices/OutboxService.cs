using GoldenCrown.Database;
using GoldenCrown.Infrastructure.Locking;
using GoldenCrown.Infrastructure.RabbitMQ;
using Microsoft.EntityFrameworkCore;

namespace GoldenCrown.Api.BackgroundServices;

public class OutboxService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan LockTtl = TimeSpan.FromSeconds(30);
    private const string LockKey = "outbox:publisher";
    private const int MaxAttempts = 10;
    private const int MaxBatchSize = 100;
    
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMessageProducer _messageProducer;
    private readonly ILogger<OutboxService> _logger;
    private readonly IDistributedLock _lock;

    public OutboxService(IServiceScopeFactory scopeFactory, IMessageProducer messageProducer,
        ILogger<OutboxService> logger, IDistributedLock @lock)
    {
        _scopeFactory = scopeFactory;
        _messageProducer = messageProducer;
        _logger = logger;
        _lock = @lock;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await PublishMessagesAsync(stoppingToken);
            }
            catch (OperationCanceledException e) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An exception occurred during outbox processing");
            }
        }
    }

    private async Task PublishMessagesAsync(CancellationToken stoppingToken)
    {
        await using var handle = await _lock.TryAcquireLockAsync(LockKey, LockTtl, stoppingToken);
        if (handle == null)
        {
            return;
        }
        
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
        var messages = await db.OutboxMessages
            .Where(m => m.SentAt == null && m.Attempts < MaxAttempts)
            .OrderBy(m => m.CreatedAt)
            .Take(MaxBatchSize)
            .ToListAsync(stoppingToken);
                
        if (messages.Count == 0)
            return;
        
        foreach (var message in messages)
        {
            try
            {
                await _messageProducer.SendMessageAsync(message.Id, message.Type, message.Payload, stoppingToken);
                message.SentAt = DateTime.UtcNow;
                message.Attempts++;
                message.Error = null;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to send outbox message {MessageId}", message.Id);
                message.Attempts++;
                message.Error = e.Message;
            }
        }

        await db.SaveChangesAsync(stoppingToken);
    }
}