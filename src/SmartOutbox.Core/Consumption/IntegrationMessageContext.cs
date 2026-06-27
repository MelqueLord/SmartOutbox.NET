namespace SmartOutbox.Core.Consumption
{
    public sealed record IntegrationMessageContext(
        string MessageId,
        string EventType,
        string? CorrelationId,
        DateTimeOffset ReceivedAt);
}
