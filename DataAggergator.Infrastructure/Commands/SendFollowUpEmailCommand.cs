namespace DataAggergator.Infrastructure.Commands
{
    public record SendFollowUpEmailCommand(Guid SubscriberId, string Email);
}
