using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Network;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client.Listener
{
    /// <summary>
    /// Message receiver that automatically respond to any message as an unsupported message
    /// </summary>
    public class UnsupportedMessageReceiver : IMessageReceiver
    {
        public Task ReceiveAsync(Message message, IMessagingHubSender sender, CancellationToken cancellationToken = default(CancellationToken))
        {
            var reason = new Reason
            {
                Code = ReasonCodes.MESSAGE_UNSUPPORTED_CONTENT_TYPE,
                Description = $"{message.Type} messages are not supported"
            };
            throw new LimeException(reason);
        }
    }
}