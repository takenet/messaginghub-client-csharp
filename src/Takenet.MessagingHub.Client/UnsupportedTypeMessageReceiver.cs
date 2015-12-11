using System;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Takenet.MessagingHub.Client
{
    internal class UnsupportedTypeMessageReceiver : MessageReceiverBase
    {
        public override Task ReceiveAsync(Message message)
        {
            return NotificationSender.SendNotificationAsync(
                message.ToFailedNotification(
                    new Reason
                    {
                        Code = ReasonCodes.MESSAGE_UNSUPPORTED_CONTENT_TYPE,
                        Description = $"{message.Type.ToString()} messages are not supported"
                    }));
        }
    }
}