using Lime.Protocol;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client.Receivers
{
    /// <summary>
    /// Proxy used to send envelopes to Messaging Hub
    /// </summary>
    /// <typeparam name="TEnvelope"></typeparam>
    public interface IEnvelopeSender<TEnvelope>
        where TEnvelope : Envelope
    {
        /// <summary>
        /// Sends an envelope
        /// </summary>
        /// <param name="envelope">Envelope type</param>
        /// <returns>Task representing the send operation</returns>
        Task<TEnvelope> SendAsync(TEnvelope envelope);
    }
}