using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SmartOutbox.Core.Consumption;
using SmartOutbox.Core.Events;
using SmartOutbox.RabbitMQ;

namespace SmartOutbox.UnitTests;

public sealed class RabbitMqConsumerRegistrationTests
{
    [Fact]
    public void AddSmartOutboxRabbitMqConsumer_RegistersHandlerStoreAndHostedConsumer()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["RabbitMq:HostName"] = "localhost",
                ["RabbitMq:Port"] = "5672",
                ["RabbitMq:UserName"] = "guest",
                ["RabbitMq:Password"] = "guest",
                ["RabbitMq:ExchangeName"] = "outbox.events",
                ["RabbitMq:QueueName"] = "smartoutbox.events",
                ["RabbitMq:DeadLetterExchangeName"] = "outbox.events.dlx",
                ["RabbitMq:DeadLetterQueueName"] = "smartoutbox.events.dlq"
            })
            .Build();
        var services = new ServiceCollection();

        services.AddSmartOutboxRabbitMqConsumer<OrderCreatedEvent, CapturingHandler>(configuration);

        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(IIntegrationEventHandler<OrderCreatedEvent>) &&
            descriptor.ImplementationType == typeof(CapturingHandler));
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(IProcessedMessageStore) &&
            descriptor.ImplementationType == typeof(InMemoryProcessedMessageStore));
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(IHostedService) &&
            descriptor.ImplementationType == typeof(RabbitMqIntegrationEventConsumer<OrderCreatedEvent>));
    }

    private sealed class CapturingHandler : IIntegrationEventHandler<OrderCreatedEvent>
    {
        public Task HandleAsync(OrderCreatedEvent integrationEvent, IntegrationMessageContext context, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
