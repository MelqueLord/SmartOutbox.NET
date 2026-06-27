namespace SmartOutbox.Core.Consumption
{
    public enum IntegrationEventConsumeResult
    {
        Processed,
        SkippedDuplicate
    }
}
