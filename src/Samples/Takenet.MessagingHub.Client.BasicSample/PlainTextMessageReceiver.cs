using System;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Receivers;

namespace Takenet.MessagingHub.Client.BasicSample
{
    public class PlainTextMessageReceiver : MessageReceiverBase
    {
        public override async Task ReceiveAsync(Message message)
        {
            Console.WriteLine(message.Content.ToString());
            await MessageSender.SendMessageAsync("Obrigado pela sua mensagem", message.From);
        }
    }
}