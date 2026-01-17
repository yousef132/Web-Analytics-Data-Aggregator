namespace DataAggergator.Application.Dtos
{
    public record AnalyticsRecord
    {
        public string Page { get; init; } = default!;
        public DateTime Date { get; init; } = DateTime.UtcNow;
        public int Users { get; init; } = default!;
        public int Sessions { get; init; } = default!;
        public int Views { get; init; } = default!;
        public decimal PerformanceScore { get; init; }
        public int LCP_ms { get; init; }

        public static List<AnalyticsRecord> AggregateGoogleAnalyticsAndPageSpeed(List<GoogleAnalyticsResponseDto> googleAnalyticsResponseDto, List<PageSpeedResponseDto> pageSpeedResponseDto)
        {
            var result = new List<AnalyticsRecord>();

            var gaDict = googleAnalyticsResponseDto.ToDictionary(ga => (ga.Page, ga.Date));
            var psDict = pageSpeedResponseDto.ToDictionary(ps => (ps.Page, ps.Date));

            var allKeys = gaDict.Keys.Union(psDict.Keys);

            foreach (var key in allKeys)
            {
                gaDict.TryGetValue(key, out var gaRecord);
                psDict.TryGetValue(key, out var psRecord);

                result.Add(new AnalyticsRecord
                {
                    Page = key.Page,
                    Date = key.Date,
                    Users = gaRecord?.Users ?? 0,
                    Sessions = gaRecord?.Sessions ?? 0,
                    Views = gaRecord?.Views ?? 0,
                    PerformanceScore = psRecord?.PerformanceScore ?? 0,
                    LCP_ms = psRecord?.LCP_ms ?? 0
                });
            }

            return result;
        }
    }
}
