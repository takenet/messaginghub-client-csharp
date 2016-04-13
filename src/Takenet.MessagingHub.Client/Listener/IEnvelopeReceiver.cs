using System.Threading;
using Lime.Protocol;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client.Listener
{
    /// <summary>
    /// Receive envelopes from Messaging Hub
    /// </summary>
    /// <typeparam name="TEnvelope"></typeparam>
    public interface IEnvelopeReceiver<in TEnvelope>
        where TEnvelope : Envelope
    {
        /// <summary>
        /// Receives an envelope
        /// </summary>
        /// <param name="sender">A sender that can be used to send envelopes</param>
        /// <param name="envelope">Envelope type</param>
        /// <param name="cancellationToken">A cancellation token to allow the task to be canceled</param>
        /// <returns>Task representing the receive operation</returns>
        Task ReceiveAsync(IMessagingHubSender sender, TEnvelope envelope, CancellationToken cancellationToken = default(CancellationToken));
    }
}