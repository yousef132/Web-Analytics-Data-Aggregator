using DataAggergator.Infrastructure.Commands;
using DataAggergator.Presentation.Middlewares;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DataAggergator.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SagaController(IBus bus) : ControllerBase
    {

        [HttpPost]
        public async Task<IActionResult> SendSaga([FromBody] string email)
        {
            //await bus.Publish(new SubscribeToNewsLetterCommand(email));
            var correlationId = Correlation.Current.Value;

            await bus.Publish(
              new SubscribeToNewsLetterCommand(email),
              ctx =>
              {
                  ctx.CorrelationId = correlationId;
              });

            return Ok();
        }
    }
}
