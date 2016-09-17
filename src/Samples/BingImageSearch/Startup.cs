using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Sender;
using Takenet.MessagingHub.Client.Listener;
using System.Diagnostics;
using System;

namespace BingImageSearch
{
    public class Startup : IStartable
    {
        private readonly IMessagingHubSender _sender;
        public static IDictionary<string, object> Settings;

        public Startup(IMessagingHubSender sender, IDictionary<string, object> settings)
        {
            _sender = sender;
            Settings = settings;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
			Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
            return Task.CompletedTask;
        }
    }
}
