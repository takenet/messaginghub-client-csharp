using Takenet.MessagingHub.Client.Senders;

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
        public IMessageSender MessageSender { get; internal set; }
        public ICommandSender CommandSender { get; internal set; }
        public INotificationSender NotificationSender { get; internal set; }
    }
}