using System.Threading;
using System.Threading.Tasks;
using SmartOutbox.Core.Events;

namespace SmartOutbox.Core.Interfaces
{
    public interface IEventPublisher
    {
        Task PublishAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
    }
}
