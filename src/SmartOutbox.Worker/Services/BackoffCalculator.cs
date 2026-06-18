using SmartOutbox.Worker.Options;

namespace SmartOutbox.Worker.Services
{
    public interface IBackoffCalculator
    {
        TimeSpan CalculateDelay(int retryCount);
    }

    public sealed class BackoffCalculator : IBackoffCalculator
    {
        private readonly OutboxProcessorOptions _options;

        public BackoffCalculator(Microsoft.Extensions.Options.IOptions<OutboxProcessorOptions> options)
        {
            _options = options.Value;
        }

        public TimeSpan CalculateDelay(int retryCount)
        {
            if (retryCount <= 0)
            {
                return TimeSpan.Zero;
            }

            var baseSeconds = Math.Max(1, _options.BackoffBaseSeconds);
            var maxSeconds = Math.Max(baseSeconds, _options.MaxBackoffSeconds);
            var exponent = retryCount - 1;
            var delaySeconds = baseSeconds * Math.Pow(2, exponent);

            return TimeSpan.FromSeconds(Math.Min(delaySeconds, maxSeconds));
        }
    }
}
