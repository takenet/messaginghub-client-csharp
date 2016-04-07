
namespace Takenet.MessagingHub.Client.Listener
{
    public abstract class EnvelopeReceiverBase
    {
        public Sender.IMessagingHubSender Sender { get; internal set; }
    }
}