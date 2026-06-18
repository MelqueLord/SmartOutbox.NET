namespace SmartOutbox.Core.Interfaces
{
    public interface ICorrelationContext
    {
        string? CorrelationId { get; set; }
    }
}
