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
        public async Task ReceiveAsync(Message envelope, IMessagingHubSender sender,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var senderAddress = envelope.From;

            Startup.Destinations.Remove(senderAddress.ToIdentity());
            await sender.SendMessageAsync(
                "The identity was removed from the destinations list.", 
                senderAddress, 
                cancellationToken);
        }
    }
}
