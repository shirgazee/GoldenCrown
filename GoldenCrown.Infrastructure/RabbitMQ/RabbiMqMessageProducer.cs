using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace GoldenCrown.Infrastructure.RabbitMQ;

public class RabbiMqMessageProducer : IMessageProducer
{
    private readonly RabbitMqSettings _settings;

    public RabbiMqMessageProducer(IOptions<RabbitMqSettings> settings)
    {
        _settings = settings.Value;
    }
    
    public async Task SendMessageAsync<T>(T message, CancellationToken token = default)
    {
        var factory = new ConnectionFactory()
        {
            HostName = _settings.Hostname,
            UserName = _settings.Username,
            Password = _settings.Password,
        };

        await using var connection = await factory.CreateConnectionAsync(token);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: token);

        var name = typeof(T).Name;
        await channel.ExchangeDeclareAsync(name, ExchangeType.Direct, cancellationToken: token);
        await channel.QueueDeclareAsync(name, durable: true, exclusive: false, autoDelete: false, cancellationToken: token);
        await channel.QueueBindAsync(name, name, routingKey: "", cancellationToken: token);
        
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);
        
        await channel.BasicPublishAsync(name, "", body: body, cancellationToken: token);
    }
}