namespace DataAggergator.Domain.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;
        public decimal Price { get; set; }  
    }
}
