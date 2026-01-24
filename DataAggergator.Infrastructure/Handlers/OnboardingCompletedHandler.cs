using DataAggergator.Infrastructure.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace DataAggergator.Infrastructure.Handlers;

public class OnboardingCompletedHandler(ILogger<OnboardingCompletedHandler> logger) : IConsumer<OnBoardingCompletedEvent>
{
    public Task Consume(ConsumeContext<OnBoardingCompletedEvent> context)
    {
        logger.LogInformation("Onboarding completed");

        return Task.CompletedTask;
    }
}
