using DataAggergator.Application.Abstractions.Services;
using DataAggergator.Application.Dtos;
using Serilog;

namespace DataAggergator.Infrastructure.Implementation.Services
{
    internal class UnitOfWork : IUnitOfWork
    {
        private readonly IGoogleAnalyticsService _googleAnalyticsService;
        private readonly IPageSpeedInsightsService _pageSpeedInsightsService;

        public UnitOfWork(IGoogleAnalyticsService googleAnalyticsService, IPageSpeedInsightsService pageSpeedInsightsService)
        {
            this._googleAnalyticsService = googleAnalyticsService;
            this._pageSpeedInsightsService = pageSpeedInsightsService;
        }
        public async Task<List<AnalyticsRecord>> AggreageResult(CancellationToken cancellationToken)
        {
            Log.Information("Starting to fetch data from Google Analytics and Page Speed Insights.");
            cancellationToken.ThrowIfCancellationRequested();
            Task<List<GoogleAnalyticsResponseDto>> googleAnalyticsTask = _googleAnalyticsService.GetGoogleAnalytics();
            Task<List<PageSpeedResponseDto>> pageSpeedInsightsTask = _pageSpeedInsightsService.GetPageSpeedInsights();

            // Wait for both to complete
            await Task.WhenAll(googleAnalyticsTask, pageSpeedInsightsTask);

            var googleAnalyticsResult = await googleAnalyticsTask;
            var pageSpeedResult = await pageSpeedInsightsTask;
            Log.Information("Fetched {GoogleAnalyticsCount} Google Analytics records and {PageSpeedCount} Page Speed records.", googleAnalyticsResult.Count, pageSpeedResult.Count);

            return  AnalyticsRecord.AggregateGoogleAnalyticsAndPageSpeed(googleAnalyticsResult, pageSpeedResult);
        }
    }
}
