# Developer Contracts

SmartOutbox.NET exposes two developer-facing contracts:

- `IIntegrationEventPublisher` for application code that publishes integration events.
- `IIntegrationEventHandler<TIntegrationEvent>` for consumer code that handles events.

The publisher does not depend on RabbitMQ. The default EF implementation stages an outbox row in the current database transaction and leaves `SaveChanges` with the application service.

```csharp
await publisher.PublishAsync(
    new OrderCreatedEvent(order.Id, order.CustomerName, order.Amount),
    cancellationToken);
```

Consumers implement only the typed handler:

```csharp
public sealed class OrderCreatedHandler : IIntegrationEventHandler<OrderCreatedEvent>
{
    public Task HandleAsync(
        OrderCreatedEvent integrationEvent,
        IntegrationMessageContext context,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
```

RabbitMQ hosting is registered with:

```csharp
builder.Services.AddSmartOutboxRabbitMqConsumer<OrderCreatedEvent, OrderCreatedHandler>(
    builder.Configuration);
```

The RabbitMQ hosted consumer:

- Deserializes the message payload to the target event type.
- Checks `IProcessedMessageStore` before invoking the handler.
- Marks the `messageId` as processed only after the handler succeeds.
- Acknowledges the broker message after successful processing.
- Rejects failed messages without requeue so RabbitMQ can route them to the dead-letter flow.

`InMemoryProcessedMessageStore` is registered by default when no store exists. It is useful for samples and local demos. Production consumers should replace it with a durable store backed by the consumer database.

