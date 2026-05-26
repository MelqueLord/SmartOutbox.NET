using System;

namespace SmartOutbox.SampleApi.Models
{
    public sealed class OrderResponse
    {
        public Guid Id { get; init; }
        public string CustomerName { get; init; } = null!;
        public decimal Amount { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
    }
}
