using System.Threading;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client.Listener
{
    internal class NotificationReceivedHandler : EnvelopeReceivedHandler<Notification>
    {
        public NotificationReceivedHandler(IMessagingHubSender sender, EnvelopeListenerRegistrar registrar, CancellationTokenSource cts) 
            : base(sender, registrar, cts)
        {
        }
    }
}