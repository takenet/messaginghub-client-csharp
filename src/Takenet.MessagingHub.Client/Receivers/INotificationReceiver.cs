using Lime.Protocol;

namespace Takenet.MessagingHub.Client.Receivers
{
    /// <summary>
    /// Specialization of <see cref="IEnvelopeReceiver{TEnvelope}"/> for notifications
    /// </summary>
    public interface INotificationReceiver : IEnvelopeReceiver<Notification>
    {
    }
}