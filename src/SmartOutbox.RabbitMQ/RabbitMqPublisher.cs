using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using SmartOutbox.RabbitMQ.Options;

namespace SmartOutbox.RabbitMQ
{
    public sealed class RabbitMqPublisher : IRabbitMqPublisher, IDisposable
    {
        private readonly dynamic _connection;
        private readonly dynamic _channel;
        private readonly RabbitMqOptions _options;
        private readonly ILogger<RabbitMqPublisher> _logger;

        public RabbitMqPublisher(IOptions<RabbitMqOptions> options, ILogger<RabbitMqPublisher> logger)
        {
            _options = options.Value;
            _logger = logger;
            _connection = CreateConnectionAsync().GetAwaiter().GetResult();
            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
            _channel.ExchangeDeclare(_options.ExchangeName, _options.ExchangeType, durable: true, autoDelete: false, arguments: null);
        }

        public Task PublishAsync(string eventType, string payload, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(eventType))
            {
                throw new ArgumentException("Event type is required.", nameof(eventType));
            }

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";
            properties.Type = eventType;
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            var body = Encoding.UTF8.GetBytes(payload);
            _channel.BasicPublish(
                exchange: _options.ExchangeName,
                routingKey: eventType,
                mandatory: false,
                basicProperties: properties,
                body: body);

            _logger.LogInformation("Published event {EventType} to exchange {ExchangeName}", eventType, _options.ExchangeName);
            return Task.CompletedTask;
        }

        private async Task<dynamic> CreateConnectionAsync()
        {
            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost,
            };

            return await factory.CreateConnectionAsync();
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
