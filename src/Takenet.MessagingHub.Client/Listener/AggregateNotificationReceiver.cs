using Lime.Protocol;

namespace Takenet.MessagingHub.Client.Listener
{

    /// <summary>
    /// Implements a <see cref="Notification"/> receiver that call multiple receivers.
    /// </summary>
    /// <seealso cref="Takenet.MessagingHub.Client.Listener.AggregateEnvelopeReceiver{Lime.Protocol.Notification}" />
    /// <seealso cref="Takenet.MessagingHub.Client.Listener.INotificationReceiver" />
    public class AggregateNotificationReceiver : AggregateEnvelopeReceiver<Notification>, INotificationReceiver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateNotificationReceiver"/> class.
        /// </summary>
        /// <param name="receivers">The receivers.</param>
        public AggregateNotificationReceiver(params INotificationReceiver[] receivers)
            : base(receivers)
        {

        }
    }
}
