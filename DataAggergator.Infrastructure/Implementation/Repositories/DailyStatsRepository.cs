using DataAggergator.Application.Abstractions.Reposioties;
using DataAggergator.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAggergator.Infrastructure.Implementation.Repositories
{
    internal class DailyStatsRepository(AnalyticsDbContext _context) : IDailyStatsRepository
    {
        public async Task<DailyStats?> GetTopLevelOverview(CancellationToken cancellationToken)
        {
            return await _context.DailyStats
                           .GroupBy(d => 1) // group into one group
                           .Select(g => new DailyStats
                           {
                               TotalUsers = g.Sum(x => x.TotalUsers),
                               TotalSessions = g.Sum(x => x.TotalSessions),
                               TotalViews = g.Sum(x => x.TotalViews),
                               AvgPerformance = g.Average(x => x.AvgPerformance),
                           })
                           .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
