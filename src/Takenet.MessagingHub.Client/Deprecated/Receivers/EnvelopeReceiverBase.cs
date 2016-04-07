
using System;

namespace Takenet.MessagingHub.Client.Deprecated.Receivers
{
    /// <summary>
    /// Base envelope receiver
    /// </summary>
    /// <remarks>
    /// Senders are automatically injected by MessagingHubClient
    /// </remarks>
    [Obsolete]
    public abstract class EnvelopeReceiverBase
    {
        public IEnvelopeSender EnvelopeSender { get; internal set; }
    }
}