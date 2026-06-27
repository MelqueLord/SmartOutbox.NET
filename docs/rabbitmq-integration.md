# RabbitMQ Integration

SmartOutbox.NET publishes outbox events to RabbitMQ through a durable topic exchange.

```mermaid
flowchart LR
    Worker[Outbox Worker] --> Publisher[RabbitMqPublisher]
    Publisher --> Client[DefaultRabbitMqClient]
    Client --> Exchange[outbox.events topic exchange]
    Exchange --> Queue[smartoutbox.events durable queue]
    Queue --> Consumers[Consumers]
    Queue --> Hosted[RabbitMqIntegrationEventConsumer]
    Queue --> DLX[outbox.events.dlx]
    DLX --> DLQ[smartoutbox.events.dlq]
```

## Topology

- Exchange: `outbox.events`
- Exchange type: `topic`
- Queue: `smartoutbox.events`
- Binding key: `#`
- Dead-letter exchange: `outbox.events.dlx`
- Dead-letter queue: `smartoutbox.events.dlq`

All exchanges and queues are durable.

## Reliability Features

- Publisher confirmations are enabled when the channel is created.
- Publisher confirmation tracking is enabled by the RabbitMQ client.
- Messages are published with `mandatory: true`.
- Messages are persistent.
- Message ID is set from the outbox row ID.
- Correlation ID is copied from the originating HTTP request when available.
- Automatic connection and topology recovery are enabled.

## Design Decisions

- The publisher declares topology at startup so local development and demos work without manual broker setup.
- The event type is used as the routing key.
- A catch-all queue binding is used for the sample to keep the demo easy to inspect.
- Dead-letter topology is used by hosted consumers when handler execution fails.

## Tradeoffs

- Application-managed topology is convenient, but larger organizations often prefer broker topology managed by infrastructure automation.
- A catch-all binding is useful for demos; production systems usually bind specific event families.
- Publisher confirms add latency, but they are worth it for reliability-focused event publication.
- The hosted consumer rejects failed handler messages without requeue. Production systems can layer a broker retry policy, delayed exchange, or application-specific recovery flow around the same handler contract.

## Consumer Responsibilities

Consumers can either read RabbitMQ directly or use `AddSmartOutboxRabbitMqConsumer<TEvent, THandler>()` to host a typed handler. The hosted consumer acknowledges a message only after the handler succeeds and the processed-message store has been updated.

Consumers should:

- Read the RabbitMQ `messageId`.
- Treat `messageId` as an idempotency key.
- Persist processed IDs before or within the same transaction as side effects.
- Reject or dead-letter invalid messages intentionally.

The default `InMemoryProcessedMessageStore` is not durable. Replace `IProcessedMessageStore` in production consumers.

## Limitations

- The hosted consumer implements acknowledgements, but no delayed broker retry policy is included.
- The dead-letter queue is broker-level; worker terminal failures are stored in the database.
- No TLS or secret manager integration is configured for local Docker defaults.
