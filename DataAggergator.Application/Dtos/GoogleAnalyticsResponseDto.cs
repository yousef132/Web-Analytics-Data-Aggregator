namespace DataAggergator.Application.Dtos
{
    public record GoogleAnalyticsResponseDto
    {
        public DateTime Date { get; init; } = DateTime.UtcNow;
        public string Page { get; init; } = default!;
        public int Users { get; init; }
        public int Sessions { get; init; }
        public int Views { get; init; }
    }
}
