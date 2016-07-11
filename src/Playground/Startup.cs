using System.Threading;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Host;
using Takenet.MessagingHub.Client.Sender;
using Takenet.MessagingHub.Client.Listener;

namespace Playground
{
    public class Startup : IStartable
    {
        private readonly IMessagingHubSender _sender;
        private readonly Application _application;

        public Startup(IMessagingHubSender sender, Application application)
        {
            _sender = sender;
            _application = application;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _sender.SendMessageAsync("Ping!", _application.Identifier, cancellationToken);
        }
    }
}
