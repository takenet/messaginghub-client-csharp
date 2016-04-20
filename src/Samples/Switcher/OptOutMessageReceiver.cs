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
        public async Task ReceiveAsync(Message message, IMessagingHubSender envelopeSender,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var sender = message.GetSender();

            Startup.Destinations.Remove(sender.ToIdentity());
            await envelopeSender.SendMessageAsync("The identity was removed from the destinations list.", sender, cancellationToken);
        }
    }
}
