namespace DataAggergator.Domain.Models
{
    public class RawData
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Page { get; set; } = default!;
        public int Users { get; set; }
        public int Sessions { get; set; }
        public int Views { get; set; }  
        public decimal PerformanceScore { get; set; }
        public decimal LCP_ms { get; set; }
        public DateTime ReceivedAt { get; set; }

    }
}
