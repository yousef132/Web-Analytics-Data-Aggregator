using System.Text.Json;
using DataAggergator.Application.Abstractions.Services;
using DataAggergator.Application.Dtos;
using Serilog;

namespace DataAggergator.Infrastructure.Implementation.Services
{
    internal class GoogleAnalyticsService : IGoogleAnalyticsService
    {
        private readonly string _filePath;
        public GoogleAnalyticsService()
        {
            _filePath = "MockData/googleanalytics.json";
        }
        public async Task<List<GoogleAnalyticsResponseDto>> GetGoogleAnalytics()
        {

            await using var stream = File.OpenRead(_filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            // Deserialize asynchronously from the stream
            var googleAnalyticsData = await System.Text.Json.JsonSerializer
                .DeserializeAsync<List<GoogleAnalyticsResponseDto>>(stream, options);

            Log.Information("Fetched Google Analytics data from JSON file.");
                if (googleAnalyticsData == null)
                {
                    var ex = new Exception("Failed to fetch google analytics data.");
                    Log.Error(ex, "Failed to deserialize Google Analytics data from JSON file.");
                    throw ex;
                }

            return googleAnalyticsData;
        }
    }
}
