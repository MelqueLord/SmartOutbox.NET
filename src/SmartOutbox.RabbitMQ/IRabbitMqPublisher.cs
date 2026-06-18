using System.Threading;
using System.Threading.Tasks;

namespace SmartOutbox.RabbitMQ
{
    public interface IRabbitMqPublisher
    {
        Task PublishAsync(
            string eventType,
            string payload,
            string messageId,
            string? correlationId = null,
            CancellationToken cancellationToken = default);
    }
}
