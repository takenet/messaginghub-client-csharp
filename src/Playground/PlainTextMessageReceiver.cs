using System;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Receivers;

namespace Takenet.MessagingHub.Client.Playground
{
    /// <summary>
    /// Example of a plain text message receiver
    /// </summary>
    public class PlainTextMessageReceiver : MessageReceiverBase
    {
        public override async Task ReceiveAsync(Message message)
        {
            Console.WriteLine(message.Content.ToString());
            await EnvelopeSender.SendMessageAsync("Thanks for your message!", message.From);
        }


    }
}