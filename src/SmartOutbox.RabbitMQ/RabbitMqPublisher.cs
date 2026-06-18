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
        private readonly IRabbitMqClient _client;
        private readonly RabbitMqOptions _options;
        private readonly ILogger<RabbitMqPublisher> _logger;

        public RabbitMqPublisher(IRabbitMqClient client, IOptions<RabbitMqOptions> options, ILogger<RabbitMqPublisher> logger)
        {
            _client = client;
            _options = options.Value;
            _logger = logger;
        }

        public async Task PublishAsync(
            string eventType,
            string payload,
            string messageId,
            string? correlationId = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(eventType))
            {
                throw new ArgumentException("Event type is required.", nameof(eventType));
            }
            if (string.IsNullOrWhiteSpace(payload))
            {
                throw new ArgumentException("Payload is required.", nameof(payload));
            }
            if (string.IsNullOrWhiteSpace(messageId))
            {
                throw new ArgumentException("Message id is required.", nameof(messageId));
            }

            var body = Encoding.UTF8.GetBytes(payload);
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            await _client.PublishAsync(_options.ExchangeName, eventType, body, "application/json", eventType, messageId, correlationId, true, timestamp, cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Published event {EventType} with message id {MessageId} to exchange {ExchangeName}.", eventType, messageId, _options.ExchangeName);
        }

        public void Dispose()
        {
            try
            {
                _client?.Dispose();
            }
            catch
            {
                // ignored
            }
        }
    }
}
