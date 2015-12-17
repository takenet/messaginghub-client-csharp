using Lime.Protocol;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client.Receivers
{
    /// <summary>
    /// Proxy used to receive envelopes from Messaging Hub
    /// </summary>
    /// <typeparam name="TEnvelope"></typeparam>
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