using System.Collections.Concurrent;

namespace SmartOutbox.Core.Consumption
{
    public sealed class InMemoryProcessedMessageStore : IProcessedMessageStore
    {
        private readonly ConcurrentDictionary<string, DateTimeOffset> _processedMessages = new(StringComparer.Ordinal);

        public Task<bool> HasProcessedAsync(string messageId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_processedMessages.ContainsKey(messageId));
        }

        public Task MarkProcessedAsync(string messageId, DateTimeOffset processedAt, CancellationToken cancellationToken = default)
        {
            _processedMessages.TryAdd(messageId, processedAt);
            return Task.CompletedTask;
        }
    }
}
