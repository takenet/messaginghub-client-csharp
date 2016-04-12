using Lime.Protocol;
using System;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client.Sender;
using Takenet.MessagingHub.Client.Listener;

namespace $rootnamespace$
{
    public class PlainTextMessageReceiver : MessageReceiverBase
    {
        public override async Task ReceiveAsync(IMessagingHubSender sender, Message message, CancellationToken token)
        {
            Console.WriteLine($"From: {message.From} \tContent: {message.Content}");
            await sender.SendMessageAsync("Pong!", message.From, token);
        }
    }
}
