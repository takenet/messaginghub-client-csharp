using Lime.Messaging.Contents;
using Lime.Protocol;
using System;
using System.Threading;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client.Listener
{
    /// <summary>
    /// Implements a <see cref="Message"/> receiver that send a <see cref="ChatState"/> message the message sender with the <see cref="ChatStateEvent.Composing"/> state.
    /// </summary>
    /// <seealso cref="Takenet.MessagingHub.Client.Listener.IMessageReceiver" />
    public class ComposingMessageReceiver : IMessageReceiver
    {
        private readonly IMessagingHubSender _sender;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComposingMessageReceiver"/> class.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public ComposingMessageReceiver(IMessagingHubSender sender)
        {
            if (sender == null) throw new ArgumentNullException(nameof(sender));
            _sender = sender;
        }

        public virtual Task ReceiveAsync(Message envelope, CancellationToken cancellationToken = new CancellationToken())
        {
            return _sender.SendMessageAsync(
                new Message
                {
                    To = envelope.From,
                    Content = new ChatState
                    {
                        State = ChatStateEvent.Composing
                    }
                },
                cancellationToken);
        }
    }
}
