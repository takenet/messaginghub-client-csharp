using Lime.Protocol;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.Sender;

namespace RssContact
{
    public class PlainTextMessageReceiver : MessageReceiverBase
    {
        public override async Task ReceiveAsync(IMessagingHubSender sender, Message message, CancellationToken token)
        {
            Debug.WriteLine($"From: {message.From} \tContent: {message.Content}");
            try
            {
                Uri feedUri;
                if (!Uri.TryCreate(message.Content.ToString(), UriKind.Absolute, out feedUri))
                {
                    await sender.SendMessageAsync("URL inválida", message.From, token);
                    return;
                }

                var user = message.From.ToIdentity().ToString();
                var items = FeedParser.Get(feedUri);
                foreach (var item in items)
                {
                    await sender.SendMessageAsync($"[{item.Title}]\n{item.Content}", message.From, token);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                await sender.SendMessageAsync("Houve um erro ao processar o feed.\nPor favor, tenta novamente.", message.From, token);
            }
        }
    }
}
