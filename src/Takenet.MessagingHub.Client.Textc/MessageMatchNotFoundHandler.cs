using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Sender;
using Takenet.Textc;

namespace Takenet.MessagingHub.Client.Textc
{
    public class MessageMatchNotFoundHandler : IMatchNotFoundHandler
    {
        private readonly IMessagingHubSender _sender;
        private readonly string _messageContent;

        public MessageMatchNotFoundHandler(IMessagingHubSender sender, string messageContent)
        {
            _sender = sender;
            _messageContent = messageContent;
        }

        public Task OnMatchNotFoundAsync(Message message, IRequestContext context, CancellationToken cancellationToken)
        {
            return _sender.SendMessageAsync(_messageContent, message.GetSender(), cancellationToken);
        }
    }
}
