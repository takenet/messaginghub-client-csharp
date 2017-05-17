using Lime.Protocol;
using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client.Extensions.Tunnel
{
    public class TunnelNotificationReceiver : TunnelEnvelopeReceiver<Notification>, INotificationReceiver
    {
        public TunnelNotificationReceiver(IMessagingHubSender sender)
            : base(sender.SendNotificationAsync)
        {
        }
    }
}
