using DataAggergator.Application.Abstractions.Reposioties;
using DataAggergator.Application.Abstractions.Services;
using DataAggergator.Application.Dtos;

namespace DataAggergator.Infrastructure.Implementation.Services
{
    internal class OverViewService(IDailyStatsRepository _dailyStatsRepository, IRawDataRepository _rawDataRepository) 
        : IOverViewService
    {
        public async Task<TopLevelOverviewDto?> GetTopLevelOverview(CancellationToken cancellationToken)
        {
            var topLevelOverview = await _dailyStatsRepository.GetTopLevelOverview(cancellationToken);
            if (topLevelOverview is null)
                return null;
            return new TopLevelOverviewDto
            {
                AvgPerformanceScore = Math.Round(topLevelOverview.AvgPerformance, 2),
                TotalSessions = topLevelOverview.TotalSessions,
                TotalUsers = topLevelOverview.TotalUsers,
                TotalViews = topLevelOverview.TotalViews,
            };
        }

        public async Task<List<AllPagesOverViewDto>?> GetAllPagesOverView(CancellationToken cancellationToken)
        {
            var allPagesOverView = await _rawDataRepository.GetPageOverView(cancellationToken);
            return allPagesOverView?.Select(po => new AllPagesOverViewDto
            {
                AvgPerformanceScore = Math.Round(po.PerformanceScore, 2),
                Page = po.Page,
                TotalSessions = po.Sessions,
                TotalUsers = po.Users,
                TotalViews = po.Views,
            }).ToList();
                
         }
    }
}
