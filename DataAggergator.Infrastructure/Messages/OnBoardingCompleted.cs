namespace DataAggergator.Infrastructure.Messages
{
    public class OnBoardingCompletedEvent
    {
        public Guid SubscriberId { get; init; }

        public string Email { get; init; }
    }
}
