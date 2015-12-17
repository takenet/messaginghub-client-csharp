using Lime.Protocol;
using System;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client.Lime
{
    /// <summary>
    /// Received an envelope from a producer and forwards them to a set of receivers
    /// </summary>
    /// <typeparam name="TEnvelope">Envelope type</typeparam>
    interface IEnvelopeProcessor<TEnvelope> where TEnvelope : Envelope
    {
        void Start();
        Task StopAsync();
        Task<TEnvelope> SendReceiveAsync(TEnvelope envelope, TimeSpan timeout);
    }
}
