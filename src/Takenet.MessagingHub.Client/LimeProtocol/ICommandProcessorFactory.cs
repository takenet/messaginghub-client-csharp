using Lime.Protocol.Client;

namespace Takenet.MessagingHub.Client.LimeProtocol
{
    /// <summary>
    /// Factory for command processors
    /// </summary>
    internal interface ICommandProcessorFactory
    {
        /// <summary>
        /// Creates an envelope processor
        /// </summary>
        /// <param name="clientChannel">Client channel used to send and receive the envelopes</param>
        /// <returns></returns>
        ICommandProcessor Create(IPersistentLimeSession clientChannel);
    }
}
