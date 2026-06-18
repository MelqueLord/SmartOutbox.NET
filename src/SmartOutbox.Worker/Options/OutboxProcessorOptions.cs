namespace SmartOutbox.Worker.Options
{
    public sealed class OutboxProcessorOptions
    {
        public int PollingIntervalSeconds { get; set; } = 5;
        public int MaxRetryCount { get; set; } = 5;
        public int BatchSize { get; set; } = 20;
        public int BackoffBaseSeconds { get; set; } = 5;
        public int MaxBackoffSeconds { get; set; } = 300;
    }
}
