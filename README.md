# SmartOutbox.NET

SmartOutbox.NET is an enterprise-grade .NET 8 solution implementing the Transactional Outbox Pattern for reliable distributed event delivery.

## What problem does it solve?

In distributed systems, the dual write problem occurs when an application persists state to a database and publishes an integration event in separate operations. If the database commit succeeds but the messaging publish fails, or vice versa, the system can become inconsistent.

The transactional outbox pattern solves this by writing events into a durable outbox table inside the same transaction as the business data. A separate background process then reads the outbox, publishes events to RabbitMQ, and marks them as processed.

## Architecture

```
+--------------+    DB transaction   +--------------------+
|  Sample API  | ------------------> |  PostgreSQL        |
|              |                     |  Orders + Outbox   |
+--------------+                     +--------------------+
        |                                     |
        |                                     v
        |                               +-------------+
        |                               | Outbox      |
        |                               | Processor   |
        |                               +-------------+
        |                                     |
        |                                     v
        |                               +----------------+
        |                               | RabbitMQ       |
        |                               +----------------+
```

### Key components

- `SmartOutbox.Core` - Domain entities, integration event contracts, JSON serializer, and shared abstractions.
- `SmartOutbox.EntityFramework` - EF Core persistence, `ApplicationDbContext`, and outbox event publisher.
- `SmartOutbox.RabbitMQ` - RabbitMQ publisher, exchange configuration, and health checks.
- `SmartOutbox.Worker` - Background service that polls `outbox_messages`, publishes events, retries failures, and dead-letters after max retries.
- `SmartOutbox.SampleApi` - ASP.NET Core API that creates orders and publishes `OrderCreatedEvent` using the outbox pattern.

## Solution structure

```
src/
 ├── SmartOutbox.Core
 ├── SmartOutbox.EntityFramework
 ├── SmartOutbox.RabbitMQ
 ├── SmartOutbox.Worker
 ├── SmartOutbox.SampleApi
```

## Setup

### Requirements

- .NET 8 SDK
- Docker
- Docker Compose

### Run locally with Docker

```bash
docker compose up --build
```

The API will be available at `http://localhost:5000`.

### Migrate database manually

If you prefer to run migrations with the SDK:

```bash
dotnet build SmartOutbox.NET.sln
cd src/SmartOutbox.EntityFramework
dotnet ef database update --project SmartOutbox.EntityFramework.csproj --startup-project ../SmartOutbox.SampleApi/SmartOutbox.SampleApi.csproj
```

## Example usage

Create an order:

```bash
curl -X POST http://localhost:5000/orders \
  -H "Content-Type: application/json" \
  -d '{"customerName":"Acme Corp","amount":1499.90}'
```

The request persists the order and writes an outbox event inside the same transaction.

## Health checks

- `GET /healthz`

## Retry strategy

- Messages are retried up to `MaxRetryCount` times.
- After retry exhaustion, the message is dead-lettered by marking it processed and preserving the error text.

## Idempotency

- The outbox processor only publishes messages where `ProcessedAt` is null.
- Duplicate event delivery should be handled by consumers through idempotency keys and event versioning.

## Scalability considerations

- Use batched polling in the worker to process multiple outbox messages.
- Deploy multiple worker instances to scale event publishing.
- Use a durable RabbitMQ exchange with topic routing.
- Keep events immutable and consumers idempotent.

## Docker composition

- `postgres` - PostgreSQL persistence layer
- `rabbitmq` - RabbitMQ broker with management UI
- `sampleapi` - ASP.NET Core API service
- `worker` - Background outbox processor
