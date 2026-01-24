using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAggergator.Infrastructure.Commands;
using DataAggergator.Infrastructure.Messages;
using MassTransit;

namespace DataAggergator.Infrastructure.Sagas
{
    // orchestration of the newsletter onboarding process
    internal class NewsletterOnboardingSaga : MassTransitStateMachine<NewsLetterOnBoardingSagaData>
    {
        public State Welcoming { get; set; }   
        public State FollowingUp { get; set; } 
        public State Onboarding { get; set; }  


        public Event<SubscriberCreatedEvent> SubscriberCreated { get; set; }
        public Event<WelcomeEmailSentEvent> WelcomeEmailSent { get; set; }
        public Event<FollowUpEmailSentEvent> FollowUpEmailSent { get; set; }


        public NewsletterOnboardingSaga()
        {
            InstanceState(x => x.CurrentState);

            Event(() => SubscriberCreated,
                e => e.CorrelateById(ctx => ctx.Message.SubscriberId));

            Event(() => WelcomeEmailSent,
                e => e.CorrelateById(ctx => ctx.Message.SubscriberId));

            Event(() => FollowUpEmailSent,
                e => e.CorrelateById(ctx => ctx.Message.SubscriberId));


            Initially(
                When(SubscriberCreated)
                    .Then(context =>
                    {
                        context.Saga.SubscriberId = context.Message.SubscriberId;
                        context.Saga.Email = context.Message.Email;
                    })
                    .TransitionTo(Welcoming)
                    .Publish(context => new SendWelcomeEmailCommand(context.Message.SubscriberId, context.Message.Email)));

            During(Welcoming,
                When(WelcomeEmailSent)
                    .Then(context => context.Saga.WelcomeEmailSent = true)
                    .TransitionTo(FollowingUp)
                    .Publish(context => new SendFollowUpEmailCommand(context.Message.SubscriberId, context.Message.Email)));

            During(FollowingUp,
                When(FollowUpEmailSent)
                    .Then(context =>
                    {
                        context.Saga.FollowUpEmailSent = true;
                        context.Saga.OnboardingCompleted = true;
                    })
                    .TransitionTo(Onboarding)
                    .Publish(context => new OnBoardingCompletedEvent
                    {
                        SubscriberId = context.Message.SubscriberId,
                        Email = context.Message.Email
                    })
                    .Finalize());
        }

    }
}
