using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.Sender;
using Takenet.MessagingHub.Client;

namespace MessageTypes
{
    public class Option4MessageReceiver : IMessageReceiver
    {
        private readonly IMessagingHubSender _sender;

        public Option4MessageReceiver(IMessagingHubSender sender)
        {
            _sender = sender;
        }

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            var location = CreateLocation();
            await _sender.SendMessageAsync(location, message.From, cancellationToken);
        }

        internal static Location CreateLocation()
        {
            var location = new Location
            {
                Latitude = -22.121944,
                Longitude = -45.128889,
                Altitude = 1143
            };
            return location;
        }
    }
}
