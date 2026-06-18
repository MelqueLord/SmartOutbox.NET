using System.Threading;
using System.Threading.Tasks;

namespace SmartOutbox.RabbitMQ
{
    public interface IRabbitMqClient : IDisposable
    {
        Task PublishAsync(
            string exchange,
            string routingKey,
            byte[] body,
            string contentType,
            string type,
            string messageId,
            string? correlationId,
            bool persistent,
            long timestamp,
            CancellationToken cancellationToken = default);
    }
}
