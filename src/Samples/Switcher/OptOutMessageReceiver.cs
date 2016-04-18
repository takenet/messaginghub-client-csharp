using Lime.Protocol;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Receivers;

namespace Switcher
{
    public class OptOutMessageReceiver : MessageReceiverBase
    {
        public override async Task ReceiveAsync(Message message)
        {
            var sender = message.GetSender();

            Startup.Destinations.Remove(sender.ToIdentity());
            await EnvelopeSender.SendMessageAsync("The identity was removed from the destinations list.", sender);
        }
    }
}
