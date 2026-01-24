using DataAggergator.Application.Abstractions.Services;
using DataAggergator.Infrastructure.Commands;
using DataAggergator.Infrastructure.Messages;
using MassTransit;
using Serilog;

namespace DataAggergator.Infrastructure.Handlers;

public class SendWelcomeEmailHandler(IEmailService emailService)
    : IConsumer<SendWelcomeEmailCommand>
{
    public async Task Consume(ConsumeContext<SendWelcomeEmailCommand> context)
    {
        using (Serilog.Context.LogContext.PushProperty(
                   "CorrelationId", context.CorrelationId))
        using (Serilog.Context.LogContext.PushProperty(
                   "SubscriberId", context.Message.SubscriberId))
        {
            Log.Information(
                "Sending welcome email to {Email}",
                context.Message.Email);

            await emailService.SendWelcomeEmailAsync(context.Message.Email);

            Log.Information(
                "Welcome email sent successfully");

            await context.Publish(
                new WelcomeEmailSentEvent
                {
                    SubscriberId = context.Message.SubscriberId,
                    Email = context.Message.Email
                },
                publishContext =>
                {
                    publishContext.CorrelationId = context.CorrelationId;
                });
        }
    }
}
