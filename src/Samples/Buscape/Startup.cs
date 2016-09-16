using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.Sender;

namespace Buscape
{
    public class Startup : IStartable
    {
        private readonly IMessagingHubSender _sender;

        public Startup(IMessagingHubSender sender)
        {
            _sender = sender;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
