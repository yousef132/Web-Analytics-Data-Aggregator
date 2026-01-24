using DataAggergator.Application.Abstractions.Services;
using Microsoft.Extensions.Logging;

namespace DataAggergator.Infrastructure.Implementation.Services
{
    public sealed class EmailService(ILogger<EmailService> logger) : IEmailService
    {
        public Task SendWelcomeEmailAsync(string email)
        {
            logger.LogInformation("Sending welcome email to {Email}", email);

            return Task.CompletedTask;
        }

        public Task SendFollowUpEmailAsync(string email)
        {
            logger.LogInformation("Sending follow-up email to {Email}", email);

            return Task.CompletedTask;
        }
    }
}
