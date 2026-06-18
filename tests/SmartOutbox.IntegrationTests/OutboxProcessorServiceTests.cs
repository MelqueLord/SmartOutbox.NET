using System.Reflection;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SmartOutbox.Core.Entities;
using SmartOutbox.EntityFramework;
using SmartOutbox.RabbitMQ;
using SmartOutbox.Worker.Options;
using SmartOutbox.Worker.Services;

namespace SmartOutbox.IntegrationTests;

public sealed class OutboxProcessorServiceTests
{
    [Fact]
    public async Task ProcessBatchAsync_PublishesPendingMessagesAndMarksProcessed()
    {
        var publisher = new CapturingPublisher();
        await using var dbContext = CreateDbContext();
        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = "OrderCreatedEvent",
            Payload = "{}",
            CreatedAt = DateTimeOffset.UtcNow,
            CorrelationId = "request-123"
        };
        dbContext.OutboxMessages.Add(message);
        await dbContext.SaveChangesAsync();

        var services = new ServiceCollection()
            .AddSingleton(_ => dbContext)
            .AddSingleton<IRabbitMqPublisher>(publisher)
            .BuildServiceProvider();

        var options = Options.Create(new OutboxProcessorOptions { BatchSize = 10, MaxRetryCount = 3 });
        var processor = new OutboxProcessorService(
            services,
            options,
            new BackoffCalculator(options),
            NullLogger<OutboxProcessorService>.Instance);

        await InvokeProcessBatchAsync(processor);

        publisher.PublishedMessages.Should().ContainSingle();
        publisher.PublishedMessages[0].MessageId.Should().Be(message.Id.ToString("D"));
        publisher.PublishedMessages[0].CorrelationId.Should().Be("request-123");
        message.ProcessedAt.Should().NotBeNull();
        message.Error.Should().BeNull();
    }

    [Fact]
    public async Task ProcessBatchAsync_WhenPublishFails_SchedulesRetry()
    {
        await using var dbContext = CreateDbContext();
        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = "OrderCreatedEvent",
            Payload = "{}",
            CreatedAt = DateTimeOffset.UtcNow
        };
        dbContext.OutboxMessages.Add(message);
        await dbContext.SaveChangesAsync();

        var services = new ServiceCollection()
            .AddSingleton(_ => dbContext)
            .AddSingleton<IRabbitMqPublisher>(new FailingPublisher())
            .BuildServiceProvider();

        var options = Options.Create(new OutboxProcessorOptions
        {
            BatchSize = 10,
            MaxRetryCount = 3,
            BackoffBaseSeconds = 5,
            MaxBackoffSeconds = 60
        });
        var processor = new OutboxProcessorService(
            services,
            options,
            new BackoffCalculator(options),
            NullLogger<OutboxProcessorService>.Instance);

        await InvokeProcessBatchAsync(processor);

        message.ProcessedAt.Should().BeNull();
        message.RetryCount.Should().Be(1);
        message.NextAttemptAt.Should().NotBeNull();
        message.Error.Should().Be("broker unavailable");
    }

    private static async Task InvokeProcessBatchAsync(OutboxProcessorService processor)
    {
        var method = typeof(OutboxProcessorService).GetMethod("ProcessBatchAsync", BindingFlags.Instance | BindingFlags.NonPublic);
        method.Should().NotBeNull();
        var task = (Task)method!.Invoke(processor, [CancellationToken.None])!;
        await task;
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new ApplicationDbContext(options);
    }

    private sealed class CapturingPublisher : IRabbitMqPublisher
    {
        public List<PublishedMessage> PublishedMessages { get; } = [];

        public Task PublishAsync(string eventType, string payload, string messageId, string? correlationId = null, CancellationToken cancellationToken = default)
        {
            PublishedMessages.Add(new PublishedMessage(eventType, payload, messageId, correlationId));
            return Task.CompletedTask;
        }
    }

    private sealed class FailingPublisher : IRabbitMqPublisher
    {
        public Task PublishAsync(string eventType, string payload, string messageId, string? correlationId = null, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("broker unavailable");
        }
    }

    private sealed record PublishedMessage(string EventType, string Payload, string MessageId, string? CorrelationId);
}
