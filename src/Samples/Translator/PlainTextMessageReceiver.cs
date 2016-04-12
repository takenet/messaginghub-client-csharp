using Lime.Protocol;
using System;
using System.Threading;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.Sender;

namespace Translator
{
    public class PlainTextMessageReceiver : MessageReceiverBase
    {
        public override async Task ReceiveAsync(MessagingHubSender sender, Message message, CancellationToken token)
        {
            Console.WriteLine($"From: {message.From} \tContent: {message.Content}");
            await sender.SendMessageAsync("Pong!", message.From, token);
        }
    }
}
