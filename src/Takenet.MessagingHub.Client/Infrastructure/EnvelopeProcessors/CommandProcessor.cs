using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Client;

namespace Takenet.MessagingHub.Client.Lime
{
    /// <summary>
    /// Send and receive commands
    /// </summary>
    internal class CommandProcessor : EnvelopeProcessor<Command>
    {
        private readonly IClientChannel _clientChannel;

        public CommandProcessor(IClientChannel clientChannel)
        {
            _clientChannel = clientChannel;
        }

        protected override Task<Command> ReceiveAsync(CancellationToken cancellationToken)
        {
            return _clientChannel.ReceiveCommandAsync(cancellationToken);
        }

        protected override Task SendAsync(Command envelope, CancellationToken cancellationToken)
        {
            return _clientChannel.SendCommandAsync(envelope);
        }

        public override Task<Command> SendAsync(Command command, TimeSpan timeout)
        {
            return base.SendAsync(command, timeout);
        }
    }
}
