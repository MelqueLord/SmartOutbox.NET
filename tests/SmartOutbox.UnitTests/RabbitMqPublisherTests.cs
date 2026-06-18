using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SmartOutbox.RabbitMQ;
using SmartOutbox.RabbitMQ.Options;

namespace SmartOutbox.UnitTests;

public sealed class RabbitMqPublisherTests
{
    [Fact]
    public async Task PublishAsync_ForwardsMessageMetadata()
    {
        var client = new CapturingRabbitMqClient();
        var publisher = new RabbitMqPublisher(
            client,
            Options.Create(new RabbitMqOptions { ExchangeName = "events" }),
            NullLogger<RabbitMqPublisher>.Instance);

        await publisher.PublishAsync("OrderCreatedEvent", "{}", "message-1", "correlation-1");

        client.Exchange.Should().Be("events");
        client.RoutingKey.Should().Be("OrderCreatedEvent");
        client.MessageId.Should().Be("message-1");
        client.CorrelationId.Should().Be("correlation-1");
    }

    [Fact]
    public async Task PublishAsync_RequiresMessageId()
    {
        var publisher = new RabbitMqPublisher(
            new CapturingRabbitMqClient(),
            Options.Create(new RabbitMqOptions()),
            NullLogger<RabbitMqPublisher>.Instance);

        var act = () => publisher.PublishAsync("OrderCreatedEvent", "{}", "");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    private sealed class CapturingRabbitMqClient : IRabbitMqClient
    {
        public string? Exchange { get; private set; }
        public string? RoutingKey { get; private set; }
        public string? MessageId { get; private set; }
        public string? CorrelationId { get; private set; }

        public Task PublishAsync(
            string exchange,
            string routingKey,
            byte[] body,
            string contentType,
            string type,
            string messageId,
            string? correlationId,
            bool persistent,
            long timestamp,
            CancellationToken cancellationToken = default)
        {
            Exchange = exchange;
            RoutingKey = routingKey;
            MessageId = messageId;
            CorrelationId = correlationId;
            return Task.CompletedTask;
        }

        public void Dispose()
        {
        }
    }
}
