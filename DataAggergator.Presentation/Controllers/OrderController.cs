using DataAggergator.Application.Abstractions.Services;
using DataAggergator.Application.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DataAggergator.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService orderService;

        public OrderController(IOrderService orderService)
        {
            this.orderService = orderService;
        }

        [HttpPost]

        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateOrder([FromBody]OrderDto order,CancellationToken cancellationToken)
        {
            await orderService.CreateOrder(order);
            return Ok();
        }
    }
}
