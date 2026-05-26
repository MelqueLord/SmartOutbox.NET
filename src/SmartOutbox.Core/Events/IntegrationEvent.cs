using System;

namespace SmartOutbox.Core.Events
{
    public abstract class IntegrationEvent
    {
        protected IntegrationEvent()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTimeOffset.UtcNow;
        }

        public Guid Id { get; }
        public DateTimeOffset CreatedAt { get; }
        public string EventType => GetType().Name;
    }
}
