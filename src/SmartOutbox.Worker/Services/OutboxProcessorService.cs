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
        private readonly IBackoffCalculator _backoffCalculator;

        public OutboxProcessorService(
            IServiceProvider serviceProvider,
            IOptions<OutboxProcessorOptions> options,
            IBackoffCalculator backoffCalculator,
            ILogger<OutboxProcessorService> logger)
        {
            _serviceProvider = serviceProvider;
            _options = options.Value;
            _backoffCalculator = backoffCalculator;
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
                using var logScope = _logger.BeginScope(new
                {
                    MessageId = message.Id,
                    message.CorrelationId,
                    EventType = message.Type
                });

                try
                {
                    await publisher.PublishAsync(message.Type, message.Payload, message.Id.ToString("D"), message.CorrelationId, cancellationToken);
                    message.ProcessedAt = DateTimeOffset.UtcNow;
                    message.Error = null;
                    _logger.LogInformation("Outbox message published successfully.");
                }
                catch (Exception ex)
                {
                    message.RetryCount += 1;
                    message.Error = ex.Message.Length > 1000 ? ex.Message[..1000] : ex.Message;

                    if (message.RetryCount >= _options.MaxRetryCount)
                    {
                        message.ProcessedAt = DateTimeOffset.UtcNow;
                        message.NextAttemptAt = null;
                        _logger.LogWarning(ex, "Outbox message reached max retries and was moved to terminal failure state.");
                    }
                    else
                    {
                        var delay = _backoffCalculator.CalculateDelay(message.RetryCount);
                        message.NextAttemptAt = DateTimeOffset.UtcNow.Add(delay);

                        _logger.LogWarning(
                            ex,
                            "Failed to publish outbox message. Retry {RetryCount}/{MaxRetryCount}. Next attempt at {NextAttemptAt}.",
                            message.RetryCount,
                            _options.MaxRetryCount,
                            message.NextAttemptAt);
                    }
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
