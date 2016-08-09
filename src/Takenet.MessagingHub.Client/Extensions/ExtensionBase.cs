using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Network;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client.Extensions
{
    public abstract class ExtensionBase
    {
        protected readonly IMessagingHubSender Sender;

        protected ExtensionBase(IMessagingHubSender sender)
        {
            Sender = sender;
        }

        protected async Task ProcessCommandAsync(Command requestCommand, CancellationToken cancellationToken)
        {
            var responseCommand = await Sender
                .SendCommandAsync(requestCommand, cancellationToken)
                .ConfigureAwait(false);

            if (responseCommand.Status != CommandStatus.Success)
            {
                throw new LimeException(
                    responseCommand.Reason ??
                    new Reason() { Code = ReasonCodes.COMMAND_PROCESSING_ERROR, Description = "An error occurred" });
            }
        }

        protected Command CreateSetCommandRequest<T>(T resource, string uriPath, Node to = null) where T : Document =>
            new Command()
            {
                To = to,
                Method = CommandMethod.Set,
                Uri = new LimeUri(uriPath),
                Resource = resource
            };
    }
}