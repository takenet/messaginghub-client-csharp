using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.Sender;
using Takenet.MessagingHub.Client;

namespace MessageTypes
{
    public class Option1MessageReceiver : IMessageReceiver
    {
        private readonly IMessagingHubSender _sender;

        public Option1MessageReceiver(IMessagingHubSender sender)
        {
            _sender = sender;
        }

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            var inspiringText = CreatePlainText();
            await _sender.SendMessageAsync(inspiringText, message.From, cancellationToken);
        }

        internal static PlainText CreatePlainText()
        {
            var inspiringText = new PlainText {Text = "... Inspiração, e um pouco de café! E isso me basta!"};
            return inspiringText;
        }
    }
}
