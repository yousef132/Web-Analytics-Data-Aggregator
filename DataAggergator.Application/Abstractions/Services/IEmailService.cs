namespace DataAggergator.Application.Abstractions.Services
{
    public interface IEmailService
    {
        Task SendWelcomeEmailAsync(string email);

        Task SendFollowUpEmailAsync(string email);
    }

}
