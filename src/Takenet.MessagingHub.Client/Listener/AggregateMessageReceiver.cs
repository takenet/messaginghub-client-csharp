using Lime.Protocol;

namespace Takenet.MessagingHub.Client.Listener
{

    /// <summary>
    /// Implements a <see cref="Message"/> receiver that call multiple receivers.
    /// </summary>
    /// <seealso cref="Takenet.MessagingHub.Client.Listener.AggregateEnvelopeReceiver{Lime.Protocol.Message}" />
    /// <seealso cref="Takenet.MessagingHub.Client.Listener.IMessageReceiver" />
    public class AggregateMessageReceiver : AggregateEnvelopeReceiver<Message>, IMessageReceiver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateMessageReceiver"/> class.
        /// </summary>
        /// <param name="receivers">The receivers.</param>
        public AggregateMessageReceiver(params IMessageReceiver[] receivers)
            : base(receivers)
        {

        }
    }
}
