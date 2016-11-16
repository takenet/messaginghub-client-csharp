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
        private readonly bool _autoNotifiy;

        public MessageReceivedHandler(IMessagingHubSender sender, bool autoNotifiy, EnvelopeListenerRegistrar registrar, CancellationTokenSource cts)
            : base(sender, registrar, cts)
        {
            _autoNotifiy = autoNotifiy;
        }

        protected override async Task CallReceiversAsync(Message message, CancellationToken cancellationToken)
        {
            try
            {
                if (_autoNotifiy)
                {
                    await Sender.SendNotificationAsync(message.ToReceivedNotification(), cancellationToken);
                }

                await base.CallReceiversAsync(message, cancellationToken);

                if (_autoNotifiy)
                {
                    await Sender.SendNotificationAsync(message.ToConsumedNotification(), cancellationToken);
                }
            }
            catch (Exception ex)
            {
                Reason reason = null;
                if (ex is LimeException)
                {
                    reason = ((LimeException)ex).Reason;
                }
                else
                {
                    reason = new Reason
                    {
                        Code = ReasonCodes.APPLICATION_ERROR,
                        Description = ex.Message
                    };
                }

                if (_autoNotifiy)
                {
                    await Sender.SendNotificationAsync(message.ToFailedNotification(reason), CancellationToken.None);
                }
                throw;
            }
        }
    }
}