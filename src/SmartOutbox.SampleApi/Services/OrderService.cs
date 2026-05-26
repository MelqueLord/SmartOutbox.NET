using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartOutbox.Core.Entities;
using SmartOutbox.Core.Events;
using SmartOutbox.Core.Interfaces;
using SmartOutbox.EntityFramework;

namespace SmartOutbox.SampleApi.Services
{
    public sealed class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IEventPublisher _eventPublisher;

        public OrderService(ApplicationDbContext dbContext, IEventPublisher eventPublisher)
        {
            _dbContext = dbContext;
            _eventPublisher = eventPublisher;
        }

        public async Task<Order> CreateOrderAsync(string customerName, decimal amount, CancellationToken cancellationToken = default)
        {
            var order = new Order
            {
                Id = Guid.NewGuid(),
                CustomerName = customerName,
                Amount = amount,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            _dbContext.Orders.Add(order);

            var integrationEvent = new OrderCreatedEvent(order.Id, order.CustomerName, order.Amount);
            // Register outbox message in the same DbContext transaction. EfEventPublisher
            // will not call SaveChanges; the application controls the commit.
            await _eventPublisher.PublishAsync(integrationEvent, cancellationToken);

            // Persist both Order and OutboxMessage atomically.
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return order;
        }

        public Task<Order?> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return _dbContext.Orders.FirstOrDefaultAsync(order => order.Id == id, cancellationToken);
        }
    }
}
