using Lime.Protocol.Client;

namespace Takenet.MessagingHub.Client.Lime
{
    internal class CommandProcessorFactory : ICommandProcessorFactory
    {
        public ICommandProcessor Create(IClientChannel clientChannel)
        {
            return new CommandProcessor(clientChannel);
        }
    }
}
