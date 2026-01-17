using DataAggergator.Application.Dtos;

namespace DataAggergator.Application.Abstractions.Services
{
    public interface IGoogleAnalyticsService
    {
        Task<List<GoogleAnalyticsResponseDto>> GetGoogleAnalytics();
    }
}
