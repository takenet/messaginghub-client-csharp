using System;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Receivers;
using Takenet.Textc;

namespace Takenet.MessagingHub.Client.Textc
{
    public class TextcMessageReceiver : MessageReceiverBase
    {
        private readonly ITextProcessor _textProcessor;
        private readonly IContextProvider _contextProvider;
        private readonly Func<Message, MessageReceiverBase, Task> _matchNotFoundHandler;

        public TextcMessageReceiver(ITextProcessor textProcessor, IContextProvider contextProvider, Func<Message, MessageReceiverBase, Task> matchNotFoundHandler = null)
        {
            if (textProcessor == null) throw new ArgumentNullException(nameof(textProcessor));
            if (contextProvider == null) throw new ArgumentNullException(nameof(contextProvider));
            _textProcessor = textProcessor;
            _contextProvider = contextProvider;
            _matchNotFoundHandler = matchNotFoundHandler;
        }

        public override async Task ReceiveAsync(Message message)
        {
            try
            {
                var context = _contextProvider.GetContext(message.Pp ?? message.From, message.To);
                context.SetMessageId(message.Id);
                context.SetMessageFrom(message.From);
                context.SetMessageTo(message.To);
                context.SetMessagePp(message.Pp);
                context.SetMessageType(message.Type);
                context.SetMessageContent(message.Content);
                context.SetMessageMetadata(message.Metadata);
                await _textProcessor.ProcessAsync(message.Content.ToString(), context).ConfigureAwait(false);
            }
            catch (MatchNotFoundException)
            {
                if (_matchNotFoundHandler != null)
                {
                    await _matchNotFoundHandler(message, this).ConfigureAwait(false);
                }
            }
        }
    }
}
