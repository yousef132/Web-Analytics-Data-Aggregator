namespace DataAggergator.Infrastructure.Messages
{
    public class SubscriberCreatedEvent
    {
        public Guid SubscriberId { get; set; }
        public string Email { get; set; }
    }
}
