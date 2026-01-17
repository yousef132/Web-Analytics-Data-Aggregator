using DataAggergator.Application.Dtos;

namespace DataAggergator.Application.Abstractions.Services
{
    public interface IOverViewService
    {
        Task<TopLevelOverviewDto?> GetTopLevelOverview(CancellationToken cancellationToken);
        Task<List<AllPagesOverViewDto>?> GetAllPagesOverView(CancellationToken cancellationToken);
    }
}
