using System.Threading.Tasks;
using Lime.Protocol;

namespace Takenet.MessagingHub.Client.Receivers
{
    /// <summary>
    /// Message receiver that automatically respond to any message as an unsupported message
    /// </summary>
    public class UnsupportedMessageReceiver : MessageReceiverBase
    {
        public override Task ReceiveAsync(Message message)
        {
            return EnvelopeSender.SendNotificationAsync(
                message.ToFailedNotification(
                    new Reason
                    {
                        Code = ReasonCodes.MESSAGE_UNSUPPORTED_CONTENT_TYPE,
                        Description = $"{message.Type} messages are not supported"
                    }));
        }
    }
}