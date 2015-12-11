using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Client;

namespace Takenet.MessagingHub.Client
{
    class SenderWrapper : IMessageSender, ICommandSender, INotificationSender
    {
        IClientChannel clientChannel;

        public SenderWrapper(IClientChannel clientChannel)
        {
            this.clientChannel = clientChannel;
        }

        public Task SendMessageAsync(Message message) => clientChannel.SendMessageAsync(message);

        public Task SendCommandAsync(Command command) => clientChannel.SendCommandAsync(command);

        public Task SendNotificationAsync(Notification notification) => clientChannel.SendNotificationAsync(notification);
    }
}
