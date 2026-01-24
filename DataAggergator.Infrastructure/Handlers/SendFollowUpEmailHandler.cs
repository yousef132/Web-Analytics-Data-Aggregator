using DataAggergator.Application.Abstractions.Services;
using DataAggergator.Infrastructure.Commands;
using DataAggergator.Infrastructure.Messages;
using MassTransit;
using Serilog;

namespace DataAggergator.Infrastructure.Handlers;

public class SendFollowUpEmailHandler(IEmailService emailService)
    : IConsumer<SendFollowUpEmailCommand>
{
    public async Task Consume(ConsumeContext<SendFollowUpEmailCommand> context)
    {
        using (Serilog.Context.LogContext.PushProperty(
                   "CorrelationId", context.CorrelationId))
        using (Serilog.Context.LogContext.PushProperty(
                   "SubscriberId", context.Message.SubscriberId))
        {
            Log.Information(
                "Sending follow-up email to {Email}",
                context.Message.Email);

            await emailService.SendFollowUpEmailAsync(context.Message.Email);

            Log.Information(
                "Follow-up email sent successfully");

            await context.Publish(
                new FollowUpEmailSentEvent
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
