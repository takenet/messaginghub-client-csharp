using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.Sender;

namespace Playground
{
    /// <summary>
    /// Example of a plain text message receiver
    /// </summary>
    public class PlainTextMessageReceiver : MessageReceiverBase
    {
        public override async Task ReceiveAsync(IMessagingHubSender channel, Message message, CancellationToken token)
        {
            Console.WriteLine(message.Content.ToString());
            await channel.SendMessageAsync("Thanks for your message!", message.From, token);
        }
    }
}