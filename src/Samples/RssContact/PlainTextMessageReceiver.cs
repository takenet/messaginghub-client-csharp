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
using RssContact;
using Takenet.Iris.Messaging.Contents;
using Takenet.MessagingHub.Client.Extensions.AttendanceForwarding;

namespace RssContact
{
    public class PlainTextMessageReceiver : IMessageReceiver
    {
        private const string _operator = "5531988356247";
        private readonly IEventTrackExtension _eventTracker;
        private readonly IMessagingHubSender _sender;
        private readonly IAttendanceExtension _attendance;

        public PlainTextMessageReceiver(
            IMessagingHubSender sender, 
            IEventTrackExtension eventTracker,
            IAttendanceExtension attendanceExtension)
        {
            _sender = sender;
            _eventTracker = eventTracker;
            _attendance = attendanceExtension;
        }

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            Trace.WriteLine($"From: {message.From} \tContent: {message.Content}");

            await _eventTracker.AddAsync(MessageReceived.Category, MessageReceived.Action, cancellationToken);
            try
            {
                Uri feedUri;
                if (!Uri.TryCreate(message.Content.ToString(), UriKind.Absolute, out feedUri))
                {
                    await _attendance.ForwardMessageToAttendantAsync(message, _operator, cancellationToken);
                    //await SendMessageAsync("URL inválida", message.From, cancellationToken);
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
                Trace.WriteLine(e);
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
