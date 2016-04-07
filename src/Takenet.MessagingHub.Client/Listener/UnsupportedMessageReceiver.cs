using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Takenet.MessagingHub.Client.Listener
{
    /// <summary>
    /// Message receiver that automatically respond to any message as an unsupported message
    /// </summary>
    public class UnsupportedMessageReceiver : MessageReceiverBase
    {
        public override Task ReceiveAsync(Message message, CancellationToken token)
        {
            var reason = new Reason
            {
                Code = ReasonCodes.MESSAGE_UNSUPPORTED_CONTENT_TYPE,
                Description = $"{message.Type} messages are not supported"
            };
            return EnvelopeSender.SendNotificationAsync(message.ToFailedNotification(reason), token);
        }
    }
}