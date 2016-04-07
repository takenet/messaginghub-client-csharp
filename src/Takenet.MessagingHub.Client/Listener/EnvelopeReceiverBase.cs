
namespace Takenet.MessagingHub.Client.Listener
{
    public abstract class EnvelopeReceiverBase
    {
        public Sender.IMessagingHubSender EnvelopeSender { get; internal set; }
    }
}