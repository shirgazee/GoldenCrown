namespace GoldenCrown.Infrastructure.RabbitMQ;

public interface IMessageProducer
{
    Task SendMessageAsync(Guid messageId, string type, string payload, CancellationToken token = default);
}