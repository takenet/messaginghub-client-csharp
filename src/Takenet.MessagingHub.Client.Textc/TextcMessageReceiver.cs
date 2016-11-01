using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.Messages;
using Takenet.Textc;

namespace Takenet.MessagingHub.Client.Textc
{
    public class TextcMessageReceiver : IMessageReceiver
    {
        private readonly ITextProcessor _textProcessor;
        private readonly IContextProvider _contextProvider;
        private readonly IExceptionHandler _exceptionHandler;

        public TextcMessageReceiver(
            ITextProcessor textProcessor, 
            IContextProvider contextProvider, 
            IExceptionHandler exceptionHandler = null)
        {
            if (textProcessor == null) throw new ArgumentNullException(nameof(textProcessor));
            if (contextProvider == null) throw new ArgumentNullException(nameof(contextProvider));
            _textProcessor = textProcessor;
            _contextProvider = contextProvider;
            _exceptionHandler = exceptionHandler;
        }

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            var context = await _contextProvider.GetContextAsync(message.GetSender(), message.To);
            context.SetMessage(message);

            try
            {
                await
                    _textProcessor.ProcessAsync(message.Content.ToString(), context, cancellationToken)
                        .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (_exceptionHandler == null || 
                    !await _exceptionHandler.HandleExceptionAsync(ex, message, context, cancellationToken).ConfigureAwait(false))
                {
                    throw;
                }
            }
            finally
            {
                if (context.HasToClearSession())
                {
                    context.Clear();
                }

                await _contextProvider.SaveContextAsync(message.GetSender(), message.To, context);
            }
        }
    }
}
