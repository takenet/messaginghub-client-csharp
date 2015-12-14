using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Client;
using System;

namespace Takenet.MessagingHub.Client.Senders
{
    class SenderWrapper : IMessageSender, ICommandSender, INotificationSender
    {
        IClientChannel clientChannel;

        public SenderWrapper(IClientChannel clientChannel)
        {
            this.clientChannel = clientChannel;
        }

        public Task SendMessageAsync(Message message) => clientChannel.SendMessageAsync(message);

        public Task<Command> SendCommandAsync(Command command) { throw new NotImplementedException(); }

        public Task SendNotificationAsync(Notification notification) => clientChannel.SendNotificationAsync(notification);
    }
}
