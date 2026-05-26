using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartOutbox.RabbitMQ.Options;

namespace SmartOutbox.RabbitMQ
{
    public sealed class DefaultRabbitMqClient : IRabbitMqClient, IDisposable
    {
        private readonly dynamic _connection;
        private readonly dynamic _channel;
        private readonly RabbitMqOptions _options;
        private readonly ILogger<DefaultRabbitMqClient> _logger;

        public DefaultRabbitMqClient(IOptions<RabbitMqOptions> options, ILogger<DefaultRabbitMqClient> logger)
        {
            _options = options.Value;
            _logger = logger;
            _connection = CreateConnectionAsync().GetAwaiter().GetResult();
            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
            _channel.ExchangeDeclare(_options.ExchangeName, _options.ExchangeType, durable: true, autoDelete: false, arguments: null);
        }

        public Task PublishAsync(string exchange, string routingKey, byte[] body, string contentType, string type, bool persistent, long timestamp, CancellationToken cancellationToken = default)
        {
            var properties = _channel.CreateBasicProperties();
            properties.Persistent = persistent;
            properties.ContentType = contentType;
            properties.Type = type;
            properties.Timestamp = new global::RabbitMQ.Client.AmqpTimestamp(timestamp);

            _channel.BasicPublish(
                exchange: exchange,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: properties,
                body: body);

            _logger.LogInformation("Published message to exchange {Exchange} routingKey {RoutingKey}", exchange, routingKey);
            return Task.CompletedTask;
        }

        private async Task<dynamic> CreateConnectionAsync()
        {
            var factory = new global::RabbitMQ.Client.ConnectionFactory
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
