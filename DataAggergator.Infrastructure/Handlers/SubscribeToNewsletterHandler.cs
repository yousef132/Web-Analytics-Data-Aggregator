using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAggergator.Infrastructure.Commands;
using DataAggergator.Infrastructure.Messages;
using DataAggergator.Domain.Models;
using DataAggergator.Infrastructure;
using MassTransit;
using Serilog;

namespace DataAggergator.Infrastructure.Handlers
{
    public class SubscribeToNewsletterHandler(
     AnalyticsDbContext dbContext)
     : IConsumer<SubscribeToNewsLetterCommand>
    {
        public async Task Consume(ConsumeContext<SubscribeToNewsLetterCommand> context)
        {
            using (Serilog.Context.LogContext.PushProperty(
                       "CorrelationId", context.CorrelationId))
            {
                Log.Information(
                    "Handling SubscribeToNewsLetterCommand for Email {Email}",
                    context.Message.Email);

                var subscriber = new Subscriber
                {
                    Id = Guid.NewGuid(),
                    Email = context.Message.Email,
                    SubscribedOnUtc = DateTime.UtcNow
                };

                dbContext.Subscribers.Add(subscriber);
                await dbContext.SaveChangesAsync();

                using (Serilog.Context.LogContext.PushProperty(
                           "SubscriberId", subscriber.Id))
                {
                    Log.Information(
                        "Subscriber created successfully");

                    await context.Publish(
                        new SubscriberCreatedEvent
                        {
                            SubscriberId = subscriber.Id,
                            Email = subscriber.Email
                        },
                        publishContext =>
                        {
                            publishContext.CorrelationId = context.CorrelationId;
                        });
                }
            }
        }
    }

}
