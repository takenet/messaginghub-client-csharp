using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.Sender;
using Takenet.MessagingHub.Client.Extensions.EventTracker;
using System.Diagnostics;

namespace Extensions
{
    public class EventTrackMessageReceiver : IMessageReceiver
    {
        private readonly IEventTrackExtension _eventTrackExtension;
        private readonly IMessagingHubSender _sender;

        public EventTrackMessageReceiver(IMessagingHubSender sender, IEventTrackExtension eventTrackExtension)
        {
            _sender = sender;
            _eventTrackExtension = eventTrackExtension;
        }

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            await _eventTrackExtension.AddAsync("featureX", "used");
        }
    }
}
