namespace SmartOutbox.Worker.Options
{
    public sealed class OutboxProcessorOptions
    {
        public int PollingIntervalSeconds { get; set; } = 5;
        public int MaxRetryCount { get; set; } = 5;
        public int BatchSize { get; set; } = 20;
        // Base backoff in seconds used for exponential backoff calculation.
        public int BackoffBaseSeconds { get; set; } = 5;
    }
}
