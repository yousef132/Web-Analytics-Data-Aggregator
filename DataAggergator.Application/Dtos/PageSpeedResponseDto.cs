namespace DataAggergator.Application.Dtos
{
    public record PageSpeedResponseDto
    {
        public DateTime Date { get; init; } = DateTime.UtcNow;
        public string Page { get; init; } = default!;
        public decimal PerformanceScore { get; init; }
        public int LCP_ms { get; init; }
    }
}
