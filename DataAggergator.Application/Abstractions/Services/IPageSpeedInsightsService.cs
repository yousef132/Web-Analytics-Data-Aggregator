using DataAggergator.Application.Dtos;

namespace DataAggergator.Application.Abstractions.Services
{
    public interface IPageSpeedInsightsService
    {
        Task<List<PageSpeedResponseDto>> GetPageSpeedInsights();
    }
}
