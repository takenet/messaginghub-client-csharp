using Lime.Protocol;
using System;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Receivers;

namespace Echo
{
    public class EchoMessageReceiver : MessageReceiverBase
    {
        public override async Task ReceiveAsync(Message message)
        {
            var sender = message.From;
            var id = message.Id;

            //Client is sending received notifications automatically
            //await EnvelopeSender.SendNotificationAsync(new Notification { Event = Event.Received, To = sender, Id = id });

            //Fire and forget messages
            if (Guid.Equals(id, Guid.Empty))
            {
                return;
            }
            
            var echoMessage = message;
            echoMessage.To = sender;
            echoMessage.From = null;
            echoMessage.Pp = null;
            echoMessage.Id = Guid.NewGuid();

            await EnvelopeSender.SendMessageAsync(echoMessage);
            
            await EnvelopeSender.SendNotificationAsync(new Notification { Event = Event.Consumed, To = sender, Id = id });
        }
    }
}
