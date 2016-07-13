using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Network;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client.Extensions.Delegation
{
    public class DelegationExtension : IDelegationExtension
    {
        private readonly IMessagingHubSender _messagingHubSender;

        public DelegationExtension(IMessagingHubSender messagingHubSender)
        {
            _messagingHubSender = messagingHubSender;
        }

        public async Task DelegateAsync(Identity target, EnvelopeType[] envelopeTypes = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            var uri = "/delegations";
            
            var setRequestCommand = new Command()
            {
                Method = CommandMethod.Set,
                Uri = new LimeUri(uri),
                Resource = new Lime.Messaging.Resources.Delegation()
                {
                    EnvelopeTypes = envelopeTypes,
                    Target = target.ToNode()
                }
            };

            var setResponseCommand = await _messagingHubSender.SendCommandAsync(
                setRequestCommand,
                cancellationToken);
            if (setResponseCommand.Status != CommandStatus.Success)
            {
                throw new LimeException(
                    setResponseCommand.Reason ??
                    new Reason() { Code = ReasonCodes.COMMAND_PROCESSING_ERROR, Description = "An error occurred" });
            }
        }

        public Task UndelegateAsync(Identity target, EnvelopeType[] envelopeTypes = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }
    }
}