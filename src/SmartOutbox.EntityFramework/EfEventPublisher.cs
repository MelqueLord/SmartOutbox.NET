using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartOutbox.Core.Entities;
using SmartOutbox.Core.Events;
using SmartOutbox.Core.Interfaces;

namespace SmartOutbox.EntityFramework
{
    public sealed class EfEventPublisher : IEventPublisher
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IJsonSerializerService _serializer;

        public EfEventPublisher(ApplicationDbContext dbContext, IJsonSerializerService serializer)
        {
            _dbContext = dbContext;
            _serializer = serializer;
        }

        public async Task PublishAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
        {
            if (integrationEvent == null)
            {
                throw new ArgumentNullException(nameof(integrationEvent));
            }

            var payload = _serializer.Serialize(integrationEvent);

            var outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = integrationEvent.EventType,
                Payload = payload,
                CreatedAt = DateTimeOffset.UtcNow,
                RetryCount = 0
            };

            await _dbContext.OutboxMessages.AddAsync(outboxMessage, cancellationToken);
            // NOTE: Do not call SaveChanges here. The caller (application layer) must
            // commit the unit of work so both domain and outbox are persisted atomically.
        }
    }
}
