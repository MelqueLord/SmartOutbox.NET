using System;

namespace SmartOutbox.Core.Entities
{
    public sealed class Order
    {
        public Guid Id { get; set; }
        public string CustomerName { get; set; } = null!;
        public decimal Amount { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
