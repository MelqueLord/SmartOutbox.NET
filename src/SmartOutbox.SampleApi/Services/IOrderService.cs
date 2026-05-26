using System;
using System.Threading;
using System.Threading.Tasks;
using SmartOutbox.Core.Entities;

namespace SmartOutbox.SampleApi.Services
{
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(string customerName, decimal amount, CancellationToken cancellationToken = default);
        Task<Order?> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
