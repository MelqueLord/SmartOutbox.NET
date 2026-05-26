using System;

namespace SmartOutbox.Core.Events
{
    public sealed class OrderCreatedEvent : IntegrationEvent
    {
        public OrderCreatedEvent(Guid orderId, string customerName, decimal amount)
        {
            OrderId = orderId;
            CustomerName = customerName;
            Amount = amount;
        }

        public Guid OrderId { get; }
        public string CustomerName { get; }
        public decimal Amount { get; }
    }
}
