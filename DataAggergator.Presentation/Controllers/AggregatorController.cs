using DataAggergator.Application.Abstractions.Services;
using DataAggergator.Infrastructure.Messages;
using DataAggergator.Presentation.Middlewares;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Serilog;

namespace DataAggergator.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AggregatorController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IOverViewService _overViewService;

        public AggregatorController(IUnitOfWork unitOfWork,IPublishEndpoint publishEndpoint, IOverViewService overViewService)
        {
            this._unitOfWork = unitOfWork;
            this._publishEndpoint = publishEndpoint;
            this._overViewService = overViewService;
        }
        [HttpGet("aggregate")]
        public async Task<IActionResult> GetAggregatedData(CancellationToken cancellationToken)
        {
            Log.Information("Starting data aggregation process.");
            var result = await _unitOfWork.AggreageResult(cancellationToken);
            var correlationId = Correlation.Current.Value;
            await _publishEndpoint.Publish(new AnalyticsRecordsAggregated(result), context =>
            {
                context.CorrelationId = correlationId; // Attach MassTransit correlation
                context.Headers.Set("X-Correlation-Id", correlationId.ToString()); // Optional: custom header
            }, cancellationToken);
            Log.Information("Data aggregation process completed and message published.");

            return Ok(result);
        }


    }
}
