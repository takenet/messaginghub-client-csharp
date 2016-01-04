using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client
{
    public interface IMessagingHubSender : IWorker, IEnvelopeSender
    {
    }
}
