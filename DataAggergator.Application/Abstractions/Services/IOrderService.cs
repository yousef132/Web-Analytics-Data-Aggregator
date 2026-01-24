using DataAggergator.Application.Dtos;

namespace DataAggergator.Application.Abstractions.Services
{
    public interface IOrderService
    {
        Task CreateOrder (OrderDto order);
    }
}
