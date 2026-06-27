using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SmartOutbox.Core.Consumption;
using SmartOutbox.Core.Events;
using SmartOutbox.RabbitMQ.Options;

namespace SmartOutbox.RabbitMQ
{
    public sealed class RabbitMqIntegrationEventConsumer<TIntegrationEvent> : BackgroundService
        where TIntegrationEvent : IntegrationEvent
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly RabbitMqOptions _options;
        private readonly ILogger<RabbitMqIntegrationEventConsumer<TIntegrationEvent>> _logger;

        public RabbitMqIntegrationEventConsumer(
            IServiceProvider serviceProvider,
            IOptions<RabbitMqOptions> options,
            ILogger<RabbitMqIntegrationEventConsumer<TIntegrationEvent>> logger)
        {
            _serviceProvider = serviceProvider;
            _options = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost,
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(5),
                ClientProvidedName = $"smartoutbox-consumer-{typeof(TIntegrationEvent).Name}"
            };

            await using var connection = await factory.CreateConnectionAsync(stoppingToken);
            await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

            await DeclareTopologyAsync(channel, stoppingToken);
            await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (_, args) => await HandleDeliveryAsync(channel, args, stoppingToken);

            await channel.BasicConsumeAsync(
                queue: _options.QueueName,
                autoAck: false,
                consumer: consumer,
                cancellationToken: stoppingToken);

            _logger.LogInformation(
                "RabbitMQ integration consumer started for event {EventType} on queue {QueueName}.",
                typeof(TIntegrationEvent).Name,
                _options.QueueName);

            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
        }

        private async Task HandleDeliveryAsync(IChannel channel, BasicDeliverEventArgs args, CancellationToken cancellationToken)
        {
            var properties = args.BasicProperties;
            var messageId = string.IsNullOrWhiteSpace(properties.MessageId)
                ? args.DeliveryTag.ToString()
                : properties.MessageId;
            var eventType = string.IsNullOrWhiteSpace(properties.Type)
                ? typeof(TIntegrationEvent).Name
                : properties.Type;
            var payload = Encoding.UTF8.GetString(args.Body.ToArray());

            var context = new IntegrationMessageContext(
                messageId,
                eventType,
                properties.CorrelationId,
                DateTimeOffset.UtcNow);

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var integrationEventConsumer = scope.ServiceProvider.GetRequiredService<IntegrationEventConsumer<TIntegrationEvent>>();
                var result = await integrationEventConsumer.ConsumeAsync(payload, context, cancellationToken);

                await channel.BasicAckAsync(args.DeliveryTag, multiple: false, cancellationToken: CancellationToken.None);
                _logger.LogInformation(
                    "RabbitMQ message {MessageId} for event {EventType} acknowledged with result {Result}.",
                    messageId,
                    eventType,
                    result);
            }
            catch (Exception ex)
            {
                await channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false, cancellationToken: CancellationToken.None);
                _logger.LogError(
                    ex,
                    "RabbitMQ message {MessageId} for event {EventType} was rejected and sent to the dead-letter flow.",
                    messageId,
                    eventType);
            }
        }

        private async Task DeclareTopologyAsync(IChannel channel, CancellationToken cancellationToken)
        {
            await channel.ExchangeDeclareAsync(_options.ExchangeName, _options.ExchangeType, durable: true, autoDelete: false, cancellationToken: cancellationToken);
            await channel.ExchangeDeclareAsync(_options.DeadLetterExchangeName, ExchangeType.Topic, durable: true, autoDelete: false, cancellationToken: cancellationToken);

            await channel.QueueDeclareAsync(_options.DeadLetterQueueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);
            await channel.QueueBindAsync(_options.DeadLetterQueueName, _options.DeadLetterExchangeName, "#", cancellationToken: cancellationToken);

            var queueArguments = new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"] = _options.DeadLetterExchangeName
            };

            await channel.QueueDeclareAsync(_options.QueueName, durable: true, exclusive: false, autoDelete: false, arguments: queueArguments, cancellationToken: cancellationToken);
            await channel.QueueBindAsync(_options.QueueName, _options.ExchangeName, typeof(TIntegrationEvent).Name, cancellationToken: cancellationToken);
        }
    }
}
