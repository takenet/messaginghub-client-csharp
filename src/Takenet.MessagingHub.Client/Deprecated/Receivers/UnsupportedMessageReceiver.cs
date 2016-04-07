using System;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Takenet.MessagingHub.Client.Deprecated.Receivers
{
    /// <summary>
    /// Message receiver that automatically respond to any message as an unsupported message
    /// </summary>
    [Obsolete]
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