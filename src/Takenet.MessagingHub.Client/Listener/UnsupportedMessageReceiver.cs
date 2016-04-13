using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Messages;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client.Listener
{
    /// <summary>
    /// Message receiver that automatically respond to any message as an unsupported message
    /// </summary>
    public class UnsupportedMessageReceiver : IMessageReceiver
    {
        public Task ReceiveAsync(IMessagingHubSender sender, Message message, CancellationToken cancellationToken)
        {
            var reason = new Reason
            {
                Code = ReasonCodes.MESSAGE_UNSUPPORTED_CONTENT_TYPE,
                Description = $"{message.Type} messages are not supported"
            };
            return sender.SendNotificationAsync(message.ToFailedNotification(reason), cancellationToken);
        }
    }
}