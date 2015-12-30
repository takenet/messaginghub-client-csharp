using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client;

namespace Takenet.VirtualAssistant
{
    public sealed class VirtualAssistantService : IService
    {
        private IMessagingHubClient _messagingHubClient;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _messagingHubClient = new MessagingHubClient()
                .UsingAccessKey(ConfigurationManager.AppSettings["login"], ConfigurationManager.AppSettings["key"])
                .NewTextcMessageReceiverBuilder()                    

            throw new System.NotImplementedException();
        }

        public void Stop()
        {
            throw new System.NotImplementedException();
        }

        public Task Execution { get; private set; }
    }
}