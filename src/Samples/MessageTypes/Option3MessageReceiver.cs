using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.Sender;
using Takenet.MessagingHub.Client;

namespace MessageTypes
{
    public class Option3MessageReceiver : IMessageReceiver
    {
        private readonly IMessagingHubSender _sender;

        public Option3MessageReceiver(IMessagingHubSender sender)
        {
            _sender = sender;
        }

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            var webLink = CreateWebLink();
            await _sender.SendMessageAsync(webLink, message.From, cancellationToken);
        }

        internal static WebLink CreateWebLink()
        {
            var url = new Uri("https://pt.wikipedia.org/wiki/Caf%C3%A9");
            var previewUri =
                new Uri(
                    "https://upload.wikimedia.org/wikipedia/commons/thumb/c/c5/Roasted_coffee_beans.jpg/200px-Roasted_coffee_beans.jpg");

            var webLink = new WebLink
            {
                Text = "Café, a bebida sagrada!",
                PreviewUri = previewUri,
                Uri = url
            };
            return webLink;
        }
    }
}
