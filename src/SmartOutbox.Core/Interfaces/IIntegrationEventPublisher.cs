using SmartOutbox.Core.Events;

namespace SmartOutbox.Core.Interfaces
{
    public interface IIntegrationEventPublisher
    {
        Task PublishAsync<TIntegrationEvent>(TIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
            where TIntegrationEvent : IntegrationEvent;
    }
}
