using Lime.Protocol;
using Lime.Protocol.Client;

namespace Takenet.MessagingHub.Client.Lime
{
    /// <summary>
    /// Factory for envelope processors
    /// </summary>
    /// <typeparam name="TEnvelope">Envelope type</typeparam>
    internal interface IEnvelopeProcessorFactory<TEnvelope> where TEnvelope : Envelope
    {
        /// <summary>
        /// Creates an envelope processor
        /// </summary>
        /// <param name="clientChannel">Client channel used to send and receive the envelopes</param>
        /// <returns></returns>
        IEnvelopeProcessor<TEnvelope> Create(IClientChannel clientChannel);
    }
}
