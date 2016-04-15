using Lime.Protocol;
using System;
using System.Threading;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Sender;
using Takenet.MessagingHub.Client.Listener;

namespace Echo
{
    public class PlainTextMessageReceiver : IMessageReceiver
    {
        public async Task ReceiveAsync(Message message, IMessagingHubSender sender, CancellationToken cancellationToken)
        {
            Console.WriteLine($"From: {message.From} \tContent: {message.Content}");
            await sender.SendMessageAsync("Pong!", message.From, cancellationToken);
        }
    }
}
