using DataAggergator.Domain.Models;

namespace DataAggergator.Application.Abstractions.Reposioties
{
    public interface IDailyStatsRepository
    {
        Task<DailyStats?> GetTopLevelOverview(CancellationToken cancellationToken);
    }
}
