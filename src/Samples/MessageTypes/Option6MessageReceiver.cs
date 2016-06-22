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
    public class Option6MessageReceiver : IMessageReceiver
    {
        private readonly IMessagingHubSender _sender;

        public Option6MessageReceiver(IMessagingHubSender sender)
        {
            _sender = sender;
        }

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            

            //await _sender.SendMessageAsync(composite, message.From, cancellationToken);
        }
    }
}
