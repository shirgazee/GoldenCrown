namespace GoldenCrown.Infrastructure.RabbitMQ;

public interface IMessageProducer
{
    Task SendMessageAsync<T> (T message, CancellationToken token = default);
}