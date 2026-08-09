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
    
    public async Task SendMessageAsync(Guid messageId, string type, string payload, CancellationToken token = default)
    {
        var factory = new ConnectionFactory()
        {
            HostName = _settings.Hostname,
            UserName = _settings.Username,
            Password = _settings.Password,
        };

        // todo создавать channel и connection один раз и переиспользовать их, а не создавать каждый раз заново
        await using var connection = await factory.CreateConnectionAsync(token);
        await using var channel = await connection.CreateChannelAsync(
            new CreateChannelOptions(publisherConfirmationsEnabled: true, publisherConfirmationTrackingEnabled: true),
            cancellationToken: token);

        var name = type;
        await channel.ExchangeDeclareAsync(name, ExchangeType.Direct, cancellationToken: token);
        await channel.QueueDeclareAsync(name, durable: true, exclusive: false, autoDelete: false, cancellationToken: token);
        await channel.QueueBindAsync(name, name, routingKey: "", cancellationToken: token);

        var props = new BasicProperties
        {
            MessageId =  messageId.ToString(),
            ContentType = "application/json",
            Type = type,
            DeliveryMode = DeliveryModes.Persistent 
        };
        
        var json = payload;
        var body = Encoding.UTF8.GetBytes(json);
        
        await channel.BasicPublishAsync(type, name, mandatory: false, basicProperties: props, body: body, cancellationToken: token);
    }
}