using System;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Receivers;
using Takenet.Textc;

namespace Takenet.MessagingHub.Client.Textc
{
    public class TextMessageReceiver : MessageReceiverBase
    {
        public const string ID_VARIABLE_NAME = "$id";
        public const string FROM_VARIABLE_NAME = "$from";
        public const string TO_VARIABLE_NAME = "$to";
        public const string PP_VARIABLE_NAME = "$pp";
        public const string TYPE_VARIABLE_NAME = "$type";

        private readonly ITextProcessor _textProcessor;
        private readonly Func<Message, MessageReceiverBase, Task> _matchNotFoundHandler;

        internal TextMessageReceiver(ITextProcessor textProcessor, Func<Message, MessageReceiverBase, Task> matchNotFoundHandler)
        {
            if (textProcessor == null) throw new ArgumentNullException(nameof(textProcessor));
            _textProcessor = textProcessor;
            _matchNotFoundHandler = matchNotFoundHandler;
        }

        public override async Task ReceiveAsync(Message message)
        {
            try
            {
                // TODO: Implement a session manager
                var context = new RequestContext();
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
