using Lime.Protocol;
using Lime.Protocol.Client;

namespace Takenet.MessagingHub.Client.Lime
{
    internal class CommandProcessorFactory : IEnvelopeProcessorFactory<Command>
    {
        public IEnvelopeProcessor<Command> Create(IClientChannel clientChannel)
        {
            return new CommandProcessor(clientChannel);
        }
    }
}
