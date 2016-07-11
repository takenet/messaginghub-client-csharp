using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using NLog;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.Sender;

namespace Playground
{
    public class PlainTextMessageReceiver : IMessageReceiver
    {
        private readonly IMessagingHubSender _sender;
        private readonly ILogger _log;

        public PlainTextMessageReceiver(IMessagingHubSender sender, ILogger log)
        {
            _sender = sender;
            _log = log;
        }

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            _log.Trace($"From: {message.From} \tContent: {message.Content}");

            if (message.Content.ToString() == "Ping!")
            {
                await _sender.SendMessageAsync("Pong!", message.From, cancellationToken);
            }
        }
    }
}
