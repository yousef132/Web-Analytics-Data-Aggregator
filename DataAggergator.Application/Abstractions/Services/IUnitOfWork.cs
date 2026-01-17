using DataAggergator.Application.Dtos;

namespace DataAggergator.Application.Abstractions.Services
{
    public interface IUnitOfWork
    {
        Task<List<AnalyticsRecord>> AggreageResult(CancellationToken cancellationToken);
    }
}
