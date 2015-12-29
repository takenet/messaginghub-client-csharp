using Lime.Protocol.Client;

namespace Takenet.MessagingHub.Client.Lime
{
    /// <summary>
    /// Implementation for <see cref="ICommandProcessorFactory"/>
    /// </summary>
    internal class CommandProcessorFactory : ICommandProcessorFactory
    {
        public ICommandProcessor Create(IPersistentClientChannel clientChannel)
        {
            return new CommandProcessor(clientChannel);
        }
    }
}
