using System;
using Lime.Protocol;

namespace Takenet.MessagingHub.Client.Deprecated.Receivers
{
    /// <summary>
    /// Specialization of <see cref="IEnvelopeReceiver{TEnvelope}"/> for notifications
    /// </summary>
    [Obsolete]
    public interface INotificationReceiver : IEnvelopeReceiver<Notification>
    {
    }
}