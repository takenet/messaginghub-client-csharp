using System.Collections.Generic;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client;

namespace $rootnamespace$
{
    public class Startup : IStartable
    {
        private readonly IMessagingHubSender _sender;
        private readonly IDictionary<string, object> _settings;

        public Startup(IMessagingHubSender sender, IDictionary<string, object> settings)
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
