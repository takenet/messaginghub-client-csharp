using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Client;

namespace Takenet.MessagingHub.Client.Lime
{
    internal class CommandProcessor : EnvelopeProcessor<Command>
    {
        IClientChannel _clientChannel;

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

        public override Task<Command> SendReceiveAsync(Command command, TimeSpan timeout)
        {
            return base.SendReceiveAsync(command, timeout);
        }
    }
}
