using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartOutbox.Core.Entities;
using SmartOutbox.RabbitMQ;
using SmartOutbox.Worker.Options;

namespace SmartOutbox.Worker.Services
{
    public sealed class OutboxProcessorService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OutboxProcessorService> _logger;
        private readonly OutboxProcessorOptions _options;

        public OutboxProcessorService(
            IServiceProvider serviceProvider,
            IOptions<OutboxProcessorOptions> options,
            ILogger<OutboxProcessorService> logger)
        {
            _serviceProvider = serviceProvider;
            _options = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Outbox processor started with interval {IntervalSeconds}s and max retries {MaxRetries}.", _options.PollingIntervalSeconds, _options.MaxRetryCount);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessBatchAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error while processing outbox messages.");
                }

                await Task.Delay(TimeSpan.FromSeconds(_options.PollingIntervalSeconds), stoppingToken);
            }
        }

        private async Task ProcessBatchAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SmartOutbox.EntityFramework.ApplicationDbContext>();
            var publisher = scope.ServiceProvider.GetRequiredService<IRabbitMqPublisher>();

            var now = DateTimeOffset.UtcNow;
            var messages = await dbContext.OutboxMessages
                .Where(message => message.ProcessedAt == null && message.RetryCount < _options.MaxRetryCount && (message.NextAttemptAt == null || message.NextAttemptAt <= now))
                .OrderBy(message => message.CreatedAt)
                .Take(_options.BatchSize)
                .ToListAsync(cancellationToken);

            if (!messages.Any())
            {
                _logger.LogDebug("No pending outbox messages found.");
                return;
            }

            foreach (var message in messages)
            {
                try
                {
                    await publisher.PublishAsync(message.Type, message.Payload, cancellationToken);
                    message.ProcessedAt = DateTimeOffset.UtcNow;
                    message.Error = null;
                    _logger.LogInformation("Outbox message {MessageId} published successfully.", message.Id);
                }
                catch (Exception ex)
                {
                    message.RetryCount += 1;
                    message.Error = ex.Message;

                    if (message.RetryCount >= _options.MaxRetryCount)
                    {
                        // Dead-letter: mark processed so it won't be retried.
                        message.ProcessedAt = DateTimeOffset.UtcNow;
                        message.NextAttemptAt = null;
                        _logger.LogWarning(ex, "Message {MessageId} has reached max retries and is dead-lettered.", message.Id);
                    }
                    else
                    {
                        // Exponential backoff with jitter
                        var backoffBase = Math.Max(1, _options.BackoffBaseSeconds);
                        var exponent = message.RetryCount - 1;
                        var delaySeconds = backoffBase * Math.Pow(2, exponent);
                        // jitter +/-20%
                        var jitter = (new Random()).NextDouble() * 0.4 - 0.2;
                        delaySeconds = Math.Max(1, delaySeconds * (1 + jitter));
                        message.NextAttemptAt = DateTimeOffset.UtcNow.AddSeconds(delaySeconds);

                        _logger.LogWarning(ex, "Failed to publish message {MessageId}. Retry {RetryCount}/{MaxRetryCount}. Next attempt at {NextAttemptAt}.", message.Id, message.RetryCount, _options.MaxRetryCount, message.NextAttemptAt);
                    }
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
