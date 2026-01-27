using GoldenCrown.Database;
using Microsoft.EntityFrameworkCore;

namespace GoldenCrown.BackgroundServices
{
    public class SessionCleanupService : BackgroundService
    {
        private static readonly TimeSpan Delay = TimeSpan.FromMinutes(10);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SessionCleanupService> _logger;

        public SessionCleanupService(IServiceScopeFactory scopeFactory, ILogger<SessionCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var now = DateTime.UtcNow;

                    var deletedCount = await db.Sessions
                        .Where(x => x.ExpiresAt <= now)
                        .ExecuteDeleteAsync(stoppingToken);

                    if (deletedCount > 0)
                    {
                        _logger.LogInformation("Removed sessions: {DeletedCount}", deletedCount);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while removing sessions");
                }

                await Task.Delay(Delay, stoppingToken);
            }
        }
    }
}
