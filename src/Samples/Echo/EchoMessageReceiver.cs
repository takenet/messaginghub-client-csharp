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
        public async Task ReceiveAsync(IMessagingHubSender sender, Message message, CancellationToken cancellationToken)
        {
            //Fire and forget messages
            if (Guid.Equals(message.Id, Guid.Empty))
                return;
            
            var echoMessage = message;
            echoMessage.To = message.From;
            echoMessage.From = null;
            echoMessage.Pp = null;
            echoMessage.Id = Guid.NewGuid();

            await sender.SendMessageAsync(echoMessage, cancellationToken);
        }
    }
}
