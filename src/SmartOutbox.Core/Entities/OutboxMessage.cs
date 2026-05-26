using System;

namespace SmartOutbox.Core.Entities
{
    public sealed class OutboxMessage
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = null!;
        public string Payload { get; set; } = null!;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? ProcessedAt { get; set; }
        public int RetryCount { get; set; }
        public string? Error { get; set; }
        // NextAttemptAt allows workers to implement backoff and schedule next retry.
        public DateTimeOffset? NextAttemptAt { get; set; }
    }
}
