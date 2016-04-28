using System.Threading;
using Lime.Protocol;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.Sender;

namespace Switcher
{
    public class OptOutMessageReceiver : IMessageReceiver
    {
        private readonly IMessagingHubSender _sender;

        public OptOutMessageReceiver(IMessagingHubSender sender)
        {
            _sender = sender;
        }

        public async Task ReceiveAsync(Message envelope, 
            CancellationToken cancellationToken = new CancellationToken())
        {
            var senderAddress = envelope.From;

            Startup.Destinations.Remove(senderAddress.Name);
            await _sender.SendMessageAsync(
                "The identity was removed from the destinations list.", 
                senderAddress, 
                cancellationToken);
        }
    }
}
