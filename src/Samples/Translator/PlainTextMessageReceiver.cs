using Lime.Protocol;
using System;
using System.Threading;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.Sender;

namespace Translator
{
    public class PlainTextMessageReceiver : IMessageReceiver
    {
        public async Task ReceiveAsync(IMessagingHubSender sender, Message message, CancellationToken cancellationToken)
        {
            Console.WriteLine($"From: {message.From} \tContent: {message.Content}");
            await sender.SendMessageAsync("Pong!", message.From, cancellationToken);
        }
    }
}
