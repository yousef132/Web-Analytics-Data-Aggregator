using DataAggergator.Application.Abstractions.Services;
using DataAggergator.Application.Dtos;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DataAggergator.Presentation.Controllers
{
    [Route("api/reports")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IOverViewService _overViewService;

        public ReportsController( IOverViewService overViewService)
        {
            this._overViewService = overViewService;
        }
        [HttpGet("overview")]
        [ProducesResponseType(StatusCodes.Status200OK)]

        public async Task<ActionResult<TopLevelOverviewDto>> GetOverView(CancellationToken cancellationToken)
        {
            return Ok(await _overViewService.GetTopLevelOverview(cancellationToken));
        }

        [HttpGet("pages")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<AllPagesOverViewDto>>> GetPagesOverView(CancellationToken cancellationToken)
        {
            return Ok(await _overViewService.GetAllPagesOverView(cancellationToken));
        }
    }
}
