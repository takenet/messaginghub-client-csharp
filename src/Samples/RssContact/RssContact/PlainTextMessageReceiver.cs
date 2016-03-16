using Lime.Protocol;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Receivers;

namespace RssContact
{
    public class PlainTextMessageReceiver : MessageReceiverBase
    {
        public async override Task ReceiveAsync(Message message)
        {
            Debug.WriteLine($"From: {message.From} \tContent: {message.Content}");
            try
            {
                Uri feedUri;
                if (!Uri.TryCreate(message.Content.ToString(), UriKind.Absolute, out feedUri))
                {
                    await EnvelopeSender.SendMessageAsync("URL inválida", message.From);
                    await EnvelopeSender.SendNotificationAsync(message.ToConsumedNotification());
                    return;
                }

                var user = message.From.ToIdentity().ToString();
                var items = FeedParser.Get(feedUri);
                foreach (var item in items)
                {
                    await EnvelopeSender.SendMessageAsync($"[{item.Title}]\n{item.Content}", message.From);
                }
                await EnvelopeSender.SendNotificationAsync(message.ToConsumedNotification());
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                await EnvelopeSender.SendMessageAsync("Houve um erro ao processar o feed.\nPor favor, tenta novamente.", message.From);
                await EnvelopeSender.SendNotificationAsync(message.ToConsumedNotification());
            }
        }
    }
}
