using FluentAssertions;
using SmartOutbox.Core.Consumption;
using SmartOutbox.Core.Events;
using SmartOutbox.Core.Services;

namespace SmartOutbox.UnitTests;

public sealed class IntegrationEventConsumerTests
{
    [Fact]
    public async Task ConsumeAsync_WhenMessageWasNotProcessed_InvokesHandlerAndMarksProcessed()
    {
        var handler = new CapturingHandler();
        var store = new InMemoryProcessedMessageStore();
        var consumer = new IntegrationEventConsumer<OrderCreatedEvent>(
            new JsonSerializerService(),
            handler,
            store);
        var payload = new JsonSerializerService().Serialize(new OrderCreatedEvent(Guid.NewGuid(), "Acme", 10m));
        var context = new IntegrationMessageContext("message-1", nameof(OrderCreatedEvent), "correlation-1", DateTimeOffset.UtcNow);

        var result = await consumer.ConsumeAsync(payload, context);

        result.Should().Be(IntegrationEventConsumeResult.Processed);
        handler.HandledMessages.Should().ContainSingle();
        (await store.HasProcessedAsync("message-1")).Should().BeTrue();
    }

    [Fact]
    public async Task ConsumeAsync_WhenMessageWasProcessed_SkipsDuplicate()
    {
        var handler = new CapturingHandler();
        var store = new InMemoryProcessedMessageStore();
        await store.MarkProcessedAsync("message-1", DateTimeOffset.UtcNow);
        var consumer = new IntegrationEventConsumer<OrderCreatedEvent>(
            new JsonSerializerService(),
            handler,
            store);
        var payload = new JsonSerializerService().Serialize(new OrderCreatedEvent(Guid.NewGuid(), "Acme", 10m));
        var context = new IntegrationMessageContext("message-1", nameof(OrderCreatedEvent), null, DateTimeOffset.UtcNow);

        var result = await consumer.ConsumeAsync(payload, context);

        result.Should().Be(IntegrationEventConsumeResult.SkippedDuplicate);
        handler.HandledMessages.Should().BeEmpty();
    }

    private sealed class CapturingHandler : IIntegrationEventHandler<OrderCreatedEvent>
    {
        public List<OrderCreatedEvent> HandledMessages { get; } = [];

        public Task HandleAsync(OrderCreatedEvent integrationEvent, IntegrationMessageContext context, CancellationToken cancellationToken = default)
        {
            HandledMessages.Add(integrationEvent);
            return Task.CompletedTask;
        }
    }
}
