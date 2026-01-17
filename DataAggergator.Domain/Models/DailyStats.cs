namespace DataAggergator.Domain.Models
{
    public class DailyStats
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public int TotalUsers { get; set; }
        public int TotalSessions { get; set; }  
        public int TotalViews { get; set; }
        public decimal AvgPerformance { get; set; }

    }
}
