using Lime.Protocol;
using System.Threading;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client.Listener
{
    /// <summary>
    /// Implements a <see cref="Message"/> receiver sends a static document message to the sender when a message is received.
    /// </summary>
    /// <seealso cref="IMessageReceiver" />
    public class SendResponseMessageReceiver : IMessageReceiver
    {
        private readonly IMessagingHubSender _sender;
        private readonly Document _response;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendResponseMessageReceiver"/> class.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="response">The response.</param>
        public SendResponseMessageReceiver(IMessagingHubSender sender, Document response)
        {
            if (sender == null) throw new System.ArgumentNullException(nameof(sender));
            if (response == null) throw new System.ArgumentNullException(nameof(response));

            _sender = sender;
            _response = response;
        }

        public virtual Task ReceiveAsync(Message envelope, CancellationToken cancellationToken = new CancellationToken())
        {
            return _sender.SendMessageAsync(_response, envelope.From, cancellationToken);
        }
    }
}
