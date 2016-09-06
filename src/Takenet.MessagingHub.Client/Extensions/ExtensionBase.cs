using System;
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

            EnsuseSuccess(responseCommand);
        }

        protected async Task<T> ProcessCommandAsync<T>(Command requestCommand, CancellationToken cancellationToken) where T : Document
        {
            if (requestCommand.Method != CommandMethod.Get) throw new ArgumentException("Invalid command method. The expected is 'get'.", nameof(requestCommand));

            var responseCommand = await Sender
                .SendCommandAsync(requestCommand, cancellationToken)
                .ConfigureAwait(false);

            EnsuseSuccess(responseCommand);

            return responseCommand.Resource as T;
        }

        protected Command CreateSetCommandRequest<T>(T resource, string uriPath, Node to = null) where T : Document =>
            new Command()
            {
                To = to,
                Method = CommandMethod.Set,
                Uri = new LimeUri(uriPath),
                Resource = resource
            };

        protected Command CreateGetCommandRequest(string uriPath, Node to = null) =>
            new Command()
            {
                To = to,
                Method = CommandMethod.Get,
                Uri = new LimeUri(uriPath)
            };

        private static void EnsuseSuccess(Command responseCommand)
        {
            if (responseCommand.Status != CommandStatus.Success)
            {
                throw new LimeException(
                    responseCommand.Reason ??
                    new Reason() { Code = ReasonCodes.COMMAND_PROCESSING_ERROR, Description = "An error occurred" });
            }
        }
    }
}