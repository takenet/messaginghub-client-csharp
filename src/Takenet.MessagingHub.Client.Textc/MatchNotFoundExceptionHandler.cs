using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Sender;
using Takenet.Textc;

namespace Takenet.MessagingHub.Client.Textc
{
    public class MatchNotFoundExceptionHandler : IExceptionHandler
    {
        private readonly IMessagingHubSender _sender;
        private readonly Document _messageContent;

        public MatchNotFoundExceptionHandler(IMessagingHubSender sender, Document messageContent)
        {
            _sender = sender;
            _messageContent = messageContent;
        }

        public async Task<bool> HandleExceptionAsync(Exception exception, Message message, IRequestContext context, CancellationToken cancellationToken)
        {
            if (exception is MatchNotFoundException)
            {
                await _sender.SendMessageAsync(_messageContent, message.GetSender(), cancellationToken);
                return true;
            }
            return false;

        }

    }
}
