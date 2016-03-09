using Lime.Protocol;
using System;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Receivers;

namespace $rootnamespace$
{
    public class PlainTextMessageReceiver : MessageReceiverBase
    {
        public async override Task ReceiveAsync(Message message)
        {
            Console.WriteLine($"From: {message.From} \tContent: {message.Content}");
            await EnvelopeSender.SendMessageAsync("Obrigado por sua mensagem", message.From);
            await EnvelopeSender.SendNotificationAsync(message.ToConsumedNotification());
        }
    }
}
