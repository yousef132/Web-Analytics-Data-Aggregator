using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit;

namespace DataAggergator.Infrastructure.Sagas
{
    // the state data for the NewsLetterOnBoardingSaga, stored in the database
    public class NewsLetterOnBoardingSagaData : SagaStateMachineInstance
    {
        // pk , to identify the saga instance, it is equel to subscriber id
        //not the logging correlation id published with the messages
        public Guid CorrelationId {  get; set; }
        public string CurrentState { get; set; } = default!;
        public Guid SubscriberId { get; set; } // domain
        public string Email { get; set; } = string.Empty;
        public bool WelcomeEmailSent { get; set; }
        public bool FollowUpEmailSent { get; set; }
        public bool OnboardingCompleted { get; set; }
    }
}
