using SmartOutbox.Core.Events;

namespace SmartOutbox.Core.Consumption
{
    public interface IIntegrationEventHandler<in TIntegrationEvent>
        where TIntegrationEvent : IntegrationEvent
    {
        Task HandleAsync(TIntegrationEvent integrationEvent, IntegrationMessageContext context, CancellationToken cancellationToken = default);
    }
}
