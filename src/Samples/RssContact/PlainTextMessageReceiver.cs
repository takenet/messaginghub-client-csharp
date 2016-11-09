using Lime.Protocol;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.Sender;
using Takenet.MessagingHub.Client.Extensions.EventTracker;
using static RssContact.InteractionEvents;

namespace RssContact
{
    public class PlainTextMessageReceiver : IMessageReceiver
    {
        private readonly IEventTrackExtension _eventTracker;
        private readonly IMessagingHubSender _sender;

        public PlainTextMessageReceiver(IMessagingHubSender sender, IEventTrackExtension eventTracker)
        {
            _sender = sender;
            _eventTracker = eventTracker;
        }

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            Debug.WriteLine($"From: {message.From} \tContent: {message.Content}");
            await _eventTracker.AddAsync(MessageReceived.Category, MessageReceived.Action, cancellationToken);
            try
            {
                Uri feedUri;
                if (!Uri.TryCreate(message.Content.ToString(), UriKind.Absolute, out feedUri))
                {
                    await SendMessageAsync("URL inválida", message.From, cancellationToken);
                    await _eventTracker.AddAsync(MessageReceivedFailed.Category, MessageReceivedFailed.Action, cancellationToken);
                    return;
                }

                var user = message.From.ToIdentity().ToString();
                var items = FeedParser.Get(feedUri);
                foreach (var item in items)
                {
                    await SendMessageAsync($"[{item.Title}]\n{item.Content}", message.From, cancellationToken);
                }
                await _eventTracker.AddAsync(MessageReceivedSuccess.Category, MessageReceivedSuccess.Action, cancellationToken);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                await SendMessageAsync("Houve um erro ao processar o feed.\nPor favor, tenta novamente.", message.From, cancellationToken);
                await _eventTracker.AddAsync(MessageReceivedFailed.Category, MessageReceivedFailed.Action, cancellationToken);
            }
        }

        private Task SendMessageAsync(string text, Node to, CancellationToken cancellationToken)
        {
            return Task.WhenAll(
                _sender.SendMessageAsync(text, to, cancellationToken),
                _eventTracker.AddAsync(MessageSent.Category, MessageSent.Action, cancellationToken));
        }
    }
}
