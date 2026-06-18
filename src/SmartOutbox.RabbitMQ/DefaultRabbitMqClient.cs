using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using SmartOutbox.RabbitMQ.Options;

namespace SmartOutbox.RabbitMQ
{
    public sealed class DefaultRabbitMqClient : IRabbitMqClient, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;
        private readonly RabbitMqOptions _options;
        private readonly ILogger<DefaultRabbitMqClient> _logger;

        public DefaultRabbitMqClient(IOptions<RabbitMqOptions> options, ILogger<DefaultRabbitMqClient> logger)
        {
            _options = options.Value;
            _logger = logger;
            _connection = CreateConnectionAsync().GetAwaiter().GetResult();
            _channel = _connection.CreateChannelAsync(new CreateChannelOptions(
                publisherConfirmationsEnabled: true,
                publisherConfirmationTrackingEnabled: true,
                outstandingPublisherConfirmationsRateLimiter: null)).GetAwaiter().GetResult();
            DeclareTopologyAsync(CancellationToken.None).GetAwaiter().GetResult();
        }

        public async Task PublishAsync(
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
            var properties = new BasicProperties
            {
                Persistent = persistent,
                ContentType = contentType,
                Type = type,
                MessageId = messageId,
                CorrelationId = correlationId,
                Timestamp = new AmqpTimestamp(timestamp),
                Headers = new Dictionary<string, object?>
                {
                    ["x-message-id"] = Encoding.UTF8.GetBytes(messageId)
                }
            };

            await _channel.BasicPublishAsync(
                exchange: exchange,
                routingKey: routingKey,
                mandatory: true,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            _logger.LogInformation(
                "RabbitMQ accepted message {MessageId} on exchange {Exchange} with routing key {RoutingKey}.",
                messageId,
                exchange,
                routingKey);
        }

        private async Task<IConnection> CreateConnectionAsync()
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
                ClientProvidedName = "smartoutbox-publisher"
            };

            return await factory.CreateConnectionAsync();
        }

        private async Task DeclareTopologyAsync(CancellationToken cancellationToken)
        {
            await _channel.ExchangeDeclareAsync(_options.ExchangeName, _options.ExchangeType, durable: true, autoDelete: false, cancellationToken: cancellationToken);
            await _channel.ExchangeDeclareAsync(_options.DeadLetterExchangeName, ExchangeType.Topic, durable: true, autoDelete: false, cancellationToken: cancellationToken);

            await _channel.QueueDeclareAsync(_options.DeadLetterQueueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);
            await _channel.QueueBindAsync(_options.DeadLetterQueueName, _options.DeadLetterExchangeName, "#", cancellationToken: cancellationToken);

            var queueArguments = new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"] = _options.DeadLetterExchangeName
            };

            await _channel.QueueDeclareAsync(_options.QueueName, durable: true, exclusive: false, autoDelete: false, arguments: queueArguments, cancellationToken: cancellationToken);
            await _channel.QueueBindAsync(_options.QueueName, _options.ExchangeName, _options.RoutingKey, cancellationToken: cancellationToken);
        }

        public void Dispose()
        {
            try
            {
                _channel?.CloseAsync().GetAwaiter().GetResult();
                _channel?.DisposeAsync().AsTask().GetAwaiter().GetResult();
                _connection?.CloseAsync().GetAwaiter().GetResult();
                _connection?.DisposeAsync().AsTask().GetAwaiter().GetResult();
            }
            catch
            {
                // ignored
            }
        }
    }
}
