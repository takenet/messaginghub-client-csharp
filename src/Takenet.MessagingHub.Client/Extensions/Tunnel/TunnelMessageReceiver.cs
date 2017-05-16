using Lime.Protocol;
using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client.Extensions.Tunnel
{
    public class TunnelMessageReceiver : TunnelEnvelopeReceiver<Message>, IMessageReceiver
    {
        public TunnelMessageReceiver(IMessagingHubSender sender)
            : base(sender.SendMessageAsync)
        {
        }
    }
}
