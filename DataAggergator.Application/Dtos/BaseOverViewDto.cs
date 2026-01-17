using System.Text.Json.Serialization;

namespace DataAggergator.Application.Dtos
{
    public record BaseOverViewDto
    {
        public int TotalSessions { get; set; }
        public int TotalUsers { get; set; }
        public int TotalViews { get; set; }

        public decimal AvgPerformanceScore { get; set; }
    }

}
