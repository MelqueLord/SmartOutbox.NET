using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using SmartOutbox.Core.Events;
using SmartOutbox.Core.Interfaces;
using SmartOutbox.Core.Services;
using SmartOutbox.EntityFramework;

namespace SmartOutbox.IntegrationTests;

public sealed class OutboxPersistenceTests
{
    [Fact]
    public async Task PublishAsync_StoresOutboxMessageWithoutSavingImmediately()
    {
        await using var dbContext = CreateDbContext();
        var correlationContext = new CorrelationContext { CorrelationId = "request-123" };
        var publisher = new EfEventPublisher(
            dbContext,
            new JsonSerializerService(),
            correlationContext,
            NullLogger<EfEventPublisher>.Instance);

        await publisher.PublishAsync(new OrderCreatedEvent(Guid.NewGuid(), "Acme", 42m));

        (await dbContext.OutboxMessages.CountAsync()).Should().Be(0);

        await dbContext.SaveChangesAsync();
        var message = await dbContext.OutboxMessages.SingleAsync();

        message.Type.Should().Be(nameof(OrderCreatedEvent));
        message.Payload.Should().Contain("customerName");
        message.CorrelationId.Should().Be("request-123");
        message.ProcessedAt.Should().BeNull();
    }

    [Fact]
    public async Task IntegrationEventPublisher_StoresOutboxMessageThroughPublicAbstraction()
    {
        await using var dbContext = CreateDbContext();
        IIntegrationEventPublisher publisher = new EfEventPublisher(
            dbContext,
            new JsonSerializerService(),
            new CorrelationContext(),
            NullLogger<EfEventPublisher>.Instance);

        await publisher.PublishAsync(new OrderCreatedEvent(Guid.NewGuid(), "Acme", 42m));
        await dbContext.SaveChangesAsync();

        var message = await dbContext.OutboxMessages.SingleAsync();
        message.Type.Should().Be(nameof(OrderCreatedEvent));
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new ApplicationDbContext(options);
    }
}
