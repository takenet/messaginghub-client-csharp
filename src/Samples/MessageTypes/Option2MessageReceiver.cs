using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Lime.Protocol.Serialization;
using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.Sender;
using Takenet.MessagingHub.Client;

namespace MessageTypes
{
    public class Option2MessageReceiver : IMessageReceiver
    {
        private readonly IMessagingHubSender _sender;

        public Option2MessageReceiver(IMessagingHubSender sender)
        {
            _sender = sender;
        }

        public Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            var mediaLink = CreateMediaLink();
            return _sender.SendMessageAsync(mediaLink, message.From, cancellationToken);
        }

        internal static MediaLink CreateMediaLink()
        {
            var imageUri = new Uri("https://upload.wikimedia.org/wikipedia/commons/thumb/4/45/A_small_cup_of_coffee.JPG/200px-A_small_cup_of_coffee.JPG");

            var mediaLink = new MediaLink
            {
                Text = "Café, o que mais seria?",
                Size = 6679,
                Type = MediaType.Parse("image/jpeg"),
                PreviewUri = imageUri,
                Uri = imageUri
            };

            return mediaLink;
        }
    }
}
