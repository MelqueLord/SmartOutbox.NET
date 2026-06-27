using SmartOutbox.Core.Events;
using SmartOutbox.Core.Interfaces;

namespace SmartOutbox.Core.Consumption
{
    public sealed class IntegrationEventConsumer<TIntegrationEvent>
        where TIntegrationEvent : IntegrationEvent
    {
        private readonly IJsonSerializerService _serializer;
        private readonly IIntegrationEventHandler<TIntegrationEvent> _handler;
        private readonly IProcessedMessageStore _processedMessageStore;

        public IntegrationEventConsumer(
            IJsonSerializerService serializer,
            IIntegrationEventHandler<TIntegrationEvent> handler,
            IProcessedMessageStore processedMessageStore)
        {
            _serializer = serializer;
            _handler = handler;
            _processedMessageStore = processedMessageStore;
        }

        public async Task<IntegrationEventConsumeResult> ConsumeAsync(
            string payload,
            IntegrationMessageContext context,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(payload))
            {
                throw new ArgumentException("Payload is required.", nameof(payload));
            }

            if (string.IsNullOrWhiteSpace(context.MessageId))
            {
                throw new ArgumentException("Message id is required.", nameof(context));
            }

            if (await _processedMessageStore.HasProcessedAsync(context.MessageId, cancellationToken))
            {
                return IntegrationEventConsumeResult.SkippedDuplicate;
            }

            var integrationEvent = _serializer.Deserialize<TIntegrationEvent>(payload);
            await _handler.HandleAsync(integrationEvent, context, cancellationToken);
            await _processedMessageStore.MarkProcessedAsync(context.MessageId, DateTimeOffset.UtcNow, cancellationToken);

            return IntegrationEventConsumeResult.Processed;
        }
    }
}
