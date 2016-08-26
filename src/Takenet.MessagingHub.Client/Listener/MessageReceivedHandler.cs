using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Network;
using Takenet.MessagingHub.Client.Messages;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client.Listener
{
    internal class MessageReceivedHandler : EnvelopeReceivedHandler<Message>
    {
        public MessageReceivedHandler(IMessagingHubSender sender, EnvelopeListenerRegistrar registrar, CancellationTokenSource cts) 
            : base(sender, registrar, cts)
        {
        }

        protected override async Task CallReceiversAsync(Message message, CancellationToken cancellationToken)
        {
            try
            {
                await base.CallReceiversAsync(message, cancellationToken);
                await Sender.SendNotificationAsync(message.ToConsumedNotification(), cancellationToken);
            }
            catch (LimeException ex)
            {
                await Sender.SendNotificationAsync(message.ToFailedNotification(ex.Reason), cancellationToken);
                throw;
            }
            catch (Exception ex)
            {
                var reason = new Reason
                {
                    Code = ReasonCodes.APPLICATION_ERROR,
                    Description = ex.Message
                };
                await Sender.SendNotificationAsync(
                    message.ToFailedNotification(reason), CancellationToken.None);
                throw;
            }
        }
    }
}