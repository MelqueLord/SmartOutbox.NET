namespace SmartOutbox.Core.Consumption
{
    public interface IProcessedMessageStore
    {
        Task<bool> HasProcessedAsync(string messageId, CancellationToken cancellationToken = default);

        Task MarkProcessedAsync(string messageId, DateTimeOffset processedAt, CancellationToken cancellationToken = default);
    }
}
