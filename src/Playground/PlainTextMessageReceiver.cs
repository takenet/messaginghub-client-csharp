using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.Sender;

namespace Playground
{
    /// <summary>
    /// Example of a plain text message receiver
    /// </summary>
    public class PlainTextMessageReceiver : IMessageReceiver
    {
        public async Task ReceiveAsync(Message message, IMessagingHubSender sender, CancellationToken cancellationToken)
        {
            Console.WriteLine(message.Content.ToString());
            await sender.SendMessageAsync("Thanks for your message!", message.From, cancellationToken);
        }
    }
}