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
    public class Option5MessageReceiver : IMessageReceiver
    {
        private readonly IMessagingHubSender _sender;

        public Option5MessageReceiver(IMessagingHubSender sender)
        {
            _sender = sender;
        }

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            var composite = new DocumentCollection
            {
                ItemType = DocumentContainer.MediaType,
                Items = new[]
                {
                    new DocumentContainer
                    {
                        Value = Option1MessageReceiver.CreatePlainText()
                    },
                    new DocumentContainer
                    {
                        Value = Option2MessageReceiver.CreateMediaLink()
                    },
                    new DocumentContainer
                    {
                        Value = Option3MessageReceiver.CreateWebLink()
                    },
                    new DocumentContainer
                    {
                        Value = Option4MessageReceiver.CreateLocation()
                    }
                }
            };
            await _sender.SendMessageAsync(composite, message.From, cancellationToken);
        }
    }
}
