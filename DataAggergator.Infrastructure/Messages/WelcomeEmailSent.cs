namespace DataAggergator.Infrastructure.Messages
{
    public class WelcomeEmailSentEvent
    {
        public Guid SubscriberId { get; set; }
        public string Email { get; set; }
    }
}
