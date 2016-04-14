using Lime.Protocol;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.Sender;

namespace RssContact
{
    public class PlainTextMessageReceiver : IMessageReceiver
    {
        public async Task ReceiveAsync(Message message, IMessagingHubSender sender, CancellationToken cancellationToken)
        {
            Debug.WriteLine($"From: {message.From} \tContent: {message.Content}");
            try
            {
                Uri feedUri;
                if (!Uri.TryCreate(message.Content.ToString(), UriKind.Absolute, out feedUri))
                {
                    await sender.SendMessageAsync("URL inválida", message.From, cancellationToken);
                    return;
                }

                var user = message.From.ToIdentity().ToString();
                var items = FeedParser.Get(feedUri);
                foreach (var item in items)
                {
                    await sender.SendMessageAsync($"[{item.Title}]\n{item.Content}", message.From, cancellationToken);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                await sender.SendMessageAsync("Houve um erro ao processar o feed.\nPor favor, tenta novamente.", message.From, cancellationToken);
            }
        }
    }
}
