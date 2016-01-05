
namespace Takenet.MessagingHub.Client.Receivers
{
    /// <summary>
    /// Base envelope receiver
    /// </summary>
    /// <remarks>
    /// Senders are automatically injected by MessagingHubClient
    /// </remarks>
    public abstract class EnvelopeReceiverBase
    {
        public IEnvelopeSender EnvelopeSender { get; internal set; }
    }
}