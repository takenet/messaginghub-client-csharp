using Lime.Protocol;
using Lime.Protocol.Client;

namespace Takenet.MessagingHub.Client.Lime
{
    internal class MessageProcessorFactory : IEnvelopeProcessorFactory<Message>
    {
        public IEnvelopeProcessor<Message> Create(IClientChannel clientChannel)
        {
            return new MessageProcessor(clientChannel);
        }
    }
}
