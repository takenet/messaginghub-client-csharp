using Takenet.MessagingHub.Client.Senders;

namespace Takenet.MessagingHub.Client.Receivers
{
    /// <summary>
    /// Base class offering Senders (automaticly injected by MessagingHubClient)
    /// </summary>
    public abstract class ReceiverBase
    {
        public IMessageSender MessageSender { get; internal set; }
        public ICommandSender CommandSender { get; internal set; }
        public INotificationSender NotificationSender { get; internal set; }
    }
}