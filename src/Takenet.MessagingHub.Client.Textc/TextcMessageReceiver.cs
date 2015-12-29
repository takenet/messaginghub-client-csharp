using System;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Receivers;
using Takenet.Textc;

namespace Takenet.MessagingHub.Client.Textc
{
    public class TextcMessageReceiver : MessageReceiverBase
    {
        public const string ID_VARIABLE_NAME = "$id";
        public const string FROM_VARIABLE_NAME = "$from";
        public const string TO_VARIABLE_NAME = "$to";
        public const string PP_VARIABLE_NAME = "$pp";
        public const string TYPE_VARIABLE_NAME = "$type";

        private readonly ITextProcessor _textProcessor;
        private readonly IContextProvider _contextProvider;
        private readonly Func<Message, MessageReceiverBase, Task> _matchNotFoundHandler;

        public TextcMessageReceiver(ITextProcessor textProcessor, IContextProvider contextProvider, Func<Message, MessageReceiverBase, Task> matchNotFoundHandler = null)
        {
            if (textProcessor == null) throw new ArgumentNullException(nameof(textProcessor));
            _textProcessor = textProcessor;
            _contextProvider = contextProvider;
            _matchNotFoundHandler = matchNotFoundHandler;
        }

        public override async Task ReceiveAsync(Message message)
        {
            try
            {
                var context = _contextProvider.GetContext(message.Pp ?? message.From, message.To);
                context.SetVariable(ID_VARIABLE_NAME, message.Id);
                context.SetVariable(FROM_VARIABLE_NAME, message.From);
                context.SetVariable(TO_VARIABLE_NAME, message.To);
                context.SetVariable(PP_VARIABLE_NAME, message.Pp);
                context.SetVariable(TYPE_VARIABLE_NAME, message.Type);
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
