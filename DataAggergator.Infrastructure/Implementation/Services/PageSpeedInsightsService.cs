using System.Text.Json;
using DataAggergator.Application.Abstractions.Services;
using DataAggergator.Application.Dtos;
using Serilog;

namespace DataAggergator.Infrastructure.Implementation.Services
{
    internal class PageSpeedInsightsService : IPageSpeedInsightsService
    {
        private readonly string _filePath;
        public PageSpeedInsightsService()
        {
            _filePath = "MockData/pagespeedinsights.json";
        }
        public async Task<List<PageSpeedResponseDto>> GetPageSpeedInsights()
        {
            await using var stream = File.OpenRead(_filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true 
            };

            var pageSpeedInsights = await System.Text.Json.JsonSerializer
                .DeserializeAsync<List<PageSpeedResponseDto>>(stream, options);

            Log.Information("Fetched PageSpeed Insights data from JSON file.");
            if (pageSpeedInsights == null)
            {
                var ex = new Exception("Failed to fetch page speed insights data.");
                Log.Error(ex, "Failed to deserialize Google Analytics data from JSON file.");
                throw ex;
            }

            return pageSpeedInsights;
        }
    }
}
