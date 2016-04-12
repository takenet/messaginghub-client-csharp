using System.Collections.Generic;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.Sender;

namespace Buscape
{
    public class Startup : IStartable
    {
        private readonly MessagingHubSender _sender;
        private readonly IDictionary<string, object> _settings;

        public Startup(MessagingHubSender sender, IDictionary<string, object> settings)
        {
            _sender = sender;
            _settings = settings;
        }

        public Task StartAsync()
        {
            return Task.CompletedTask;
        }
    }
}
