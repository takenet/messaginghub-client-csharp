using Lime.Protocol;
using Lime.Protocol.Client;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client
{
    class MessageSenderWrapper : IMessageSender
    {
        IClientChannel clientChannel;

        public MessageSenderWrapper(IClientChannel clientChannel)
        {
            this.clientChannel = clientChannel;
        }

        public Task SendMessageAsync(Message message) => clientChannel.SendMessageAsync(message);
    }
}
