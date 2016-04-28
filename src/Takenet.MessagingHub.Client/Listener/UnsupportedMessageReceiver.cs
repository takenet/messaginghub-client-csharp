using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Network;

namespace Takenet.MessagingHub.Client.Listener
{
    /// <summary>
    /// Message receiver that automatically respond to any message as an unsupported message
    /// </summary>
    public class UnsupportedMessageReceiver : IMessageReceiver
    {
        public Task ReceiveAsync(Message message, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (message.Id != Guid.Empty)
            {
                throw new LimeException(
                    new Reason
                    {
                        Code = ReasonCodes.MESSAGE_UNSUPPORTED_CONTENT_TYPE,
                        Description = "There's no processor available to handle the received message"
                    });
            }

            return Task.CompletedTask;
        }
    }
}