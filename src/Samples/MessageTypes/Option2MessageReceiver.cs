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
    public class Option2MessageReceiver : IMessageReceiver
    {
        private readonly IMessagingHubSender _sender;

        public Option2MessageReceiver(IMessagingHubSender sender)
        {
            _sender = sender;
        }

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            var mediaLink = CreateMediaLink();

            await _sender.SendMessageAsync(mediaLink, message.From, cancellationToken);
        }

        internal static MediaLink CreateMediaLink()
        {
            var imageUrl = new Uri("http://eglu.pontofrio.com.br/wp-content/uploads/2013/05/guia-do-solteiro-cafe.jpg");

            var mediaLink = new MediaLink()
            {
                Size = 34113,
                Type = MediaType.Parse("image/jpg"),
                PreviewUri = imageUrl,
                Uri = imageUrl
            };
            return mediaLink;
        }
    }
}
