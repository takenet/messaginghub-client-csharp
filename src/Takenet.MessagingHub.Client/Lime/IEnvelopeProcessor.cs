using Lime.Protocol;
using System;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client.Lime
{
    /// <summary>
    /// Send or receive envelopes of a given type
    /// </summary>
    /// <typeparam name="TEnvelope">Envelope type</typeparam>
    interface IEnvelopeProcessor<TEnvelope> where TEnvelope : Envelope
    {
        /// <summary>
        /// Starts listening for envelopes
        /// </summary>
        void StartReceiving();
        
        /// <summary>
        /// Start a task to listening
        /// </summary>
        /// <returns>A task representing the stop operation</returns>
        Task StopReceivingAsync();

        /// <summary>
        /// Send an envelope
        /// </summary>
        /// <param name="envelope">Envelope</param>
        /// <param name="timeout">Send operation timeout</param>
        /// <returns></returns>
        Task<TEnvelope> SendAsync(TEnvelope envelope, TimeSpan timeout);
    }
}
