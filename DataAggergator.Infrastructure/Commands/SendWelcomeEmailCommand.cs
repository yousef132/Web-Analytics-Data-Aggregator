using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAggergator.Infrastructure.Commands
{
    public record SendWelcomeEmailCommand(Guid SubscriberId,string Email);
}
