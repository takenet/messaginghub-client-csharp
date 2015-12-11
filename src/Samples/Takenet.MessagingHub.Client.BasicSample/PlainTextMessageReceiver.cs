using System;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Takenet.MessagingHub.Client.BasicSample
{
    public class PlainTextMessageReceiver : MessageReceiverBase
    {
        public override async Task ReceiveAsync(Message message)
        {
            Console.WriteLine(message.Content.ToString());
            await Sender.SendMessageAsync("Obrigado pela sua mensagem", message.From);
        }
    }
}