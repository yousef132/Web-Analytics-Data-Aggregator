namespace DataAggergator.Infrastructure.Messages
{
    public class FollowUpEmailSentEvent
    {
        public Guid SubscriberId { get; set; }
        public string Email { get; set; }
    }
}
