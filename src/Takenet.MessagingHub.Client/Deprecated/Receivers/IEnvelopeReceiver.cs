using System;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Takenet.MessagingHub.Client.Deprecated.Receivers
{
    /// <summary>
    /// Receive envelopes from Messaging Hub
    /// </summary>
    /// <typeparam name="TEnvelope"></typeparam>
    [Obsolete]
    public interface IEnvelopeReceiver<in TEnvelope>
        where TEnvelope : Envelope
    {
        /// <summary>
        /// Receives an envelope
        /// </summary>
        /// <param name="envelope">Envelope type</param>
        /// <returns>Task representing the receive operation</returns>
        Task ReceiveAsync(TEnvelope envelope);
    }
}