using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.Sender;

namespace Echo
{
    public class EchoMessageReceiver : IMessageReceiver
    {
        private readonly IMessagingHubSender _sender;

        public EchoMessageReceiver(IMessagingHubSender sender)
        {
            _sender = sender;
        }

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            //Fire and forget messages
            if (Guid.Equals(message.Id, Guid.Empty))
                return;
            
            var echoMessage = message;
            echoMessage.To = message.From;
            echoMessage.From = null;
            echoMessage.Pp = null;
            echoMessage.Id = Guid.NewGuid().ToString();

            await _sender.SendMessageAsync(echoMessage, cancellationToken);
        }
    }
}
