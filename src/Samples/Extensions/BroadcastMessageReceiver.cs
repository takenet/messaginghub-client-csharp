using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Messaging.Contents;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.Sender;
using Takenet.MessagingHub.Client.Extensions.Broadcast;
using System.Diagnostics;

namespace Extensions
{
    public class BroadcastMessageReceiver : IMessageReceiver
    {
        private readonly IBroadcastExtension _broadcastExtension;
        private readonly IMessagingHubSender _sender;

        public BroadcastMessageReceiver(IMessagingHubSender sender, IBroadcastExtension broadcastExtension)
        {
            _broadcastExtension = broadcastExtension;
            _sender = sender;
        }

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            var listName = "couponUsers";

            //Add a new distribution list with name couponUsers
            await _broadcastExtension.CreateDistributionListAsync(listName);

            //Add some users to couponUsers list
            await _broadcastExtension.AddRecipientAsync(listName, message.From.ToIdentity());

            //Send a message to couponUsers list users
            await _broadcastExtension.SendMessageAsync(listName, new PlainText { Text = "Olá você ganhou um novo cupom de descontos" });
        }
    }
}
