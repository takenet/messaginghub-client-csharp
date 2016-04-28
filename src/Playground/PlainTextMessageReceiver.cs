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
        private readonly IMessagingHubSender _sender;

        public PlainTextMessageReceiver(IMessagingHubSender sender)
        {
            _sender = sender;
        }

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            Console.WriteLine(message.Content.ToString());
            await _sender.SendMessageAsync("Thanks for your message!", message.From, cancellationToken);
        }
    }
}