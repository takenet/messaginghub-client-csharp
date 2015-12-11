using Lime.Protocol;
using Lime.Protocol.Client;
using System.Threading.Tasks;
using System;

namespace Takenet.MessagingHub.Client
{
    class SenderWrapper : IMessageSender, INotificationSender
    {
        IClientChannel clientChannel;

        public SenderWrapper(IClientChannel clientChannel)
        {
            this.clientChannel = clientChannel;
        }

        public Task SendMessageAsync(Message message) => clientChannel.SendMessageAsync(message);

        public Task SendNotificationAsync(Notification notification) => clientChannel.SendNotificationAsync(notification);
    }
}
