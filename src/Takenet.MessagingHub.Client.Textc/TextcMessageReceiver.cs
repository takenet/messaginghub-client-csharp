using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.Messages;
using Takenet.MessagingHub.Client.Sender;
using Takenet.Textc;

namespace Takenet.MessagingHub.Client.Textc
{
    public class TextcMessageReceiver : IMessageReceiver
    {
        private readonly IMessagingHubSender _sender;
        private readonly ITextProcessor _textProcessor;
        private readonly IContextProvider _contextProvider;
        private readonly Func<Message, IMessageReceiver, Task> _matchNotFoundHandler;
        private readonly TimeSpan _processTimeout;

        private static readonly TimeSpan DefaultProcessTimeout = TimeSpan.FromSeconds(60);

        public TextcMessageReceiver(IMessagingHubSender sender, ITextProcessor textProcessor, IContextProvider contextProvider, Func<Message, IMessageReceiver, Task> matchNotFoundHandler = null, TimeSpan? processTimeout = null)
        {
            if (textProcessor == null) throw new ArgumentNullException(nameof(textProcessor));
            if (contextProvider == null) throw new ArgumentNullException(nameof(contextProvider));
            _sender = sender;
            _textProcessor = textProcessor;
            _contextProvider = contextProvider;
            _matchNotFoundHandler = matchNotFoundHandler;
            _processTimeout = processTimeout ?? DefaultProcessTimeout;
        }

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            var context = _contextProvider.GetContext(message.Pp ?? message.From, message.To);

            try
            {
                context.SetMessageId(message.Id);
                context.SetMessageFrom(message.From);
                context.SetMessageTo(message.To);
                context.SetMessagePp(message.Pp);
                context.SetMessageType(message.Type);
                context.SetMessageContent(message.Content);
                context.SetMessageMetadata(message.Metadata);

                using (var cts = new CancellationTokenSource(_processTimeout))
                {
                    await
                        _textProcessor.ProcessAsync(message.Content.ToString(), context, cts.Token)
                            .ConfigureAwait(false);
                }
            }
            catch (MatchNotFoundException)
            {
                if (_matchNotFoundHandler != null)
                {
                    await _matchNotFoundHandler(message, this).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            finally
            {
                if (context.HasToClearSession())
                {
                    context.Clear();
                }
            }
        }
    }
}
