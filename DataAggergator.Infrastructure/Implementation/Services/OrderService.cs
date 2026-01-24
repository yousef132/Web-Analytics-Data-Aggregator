using DataAggergator.Application.Abstractions.Reposioties;
using DataAggergator.Application.Abstractions.Services;
using DataAggergator.Application.Dtos;
using DataAggergator.Domain.Models;

namespace DataAggergator.Infrastructure.Implementation.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOutboxRepository outboxRepository;

        public OrderService(IOutboxRepository outboxRepository)
        {
            this.outboxRepository = outboxRepository;
        }
        public Task CreateOrder(OrderDto order)
        {
            var orderCreatedEvent = new Order
            {
                Name = order.name,
                Price = order.price,
            };
            return outboxRepository.AddToOutbox(orderCreatedEvent);
        }
    }
}
