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

        protected override Task<Command> Receive(CancellationToken cancellationToken)
        {
            return _clientChannel.ReceiveCommandAsync(cancellationToken);
        }

        protected override Task Send(Command envelope, CancellationToken cancellationToken)
        {
            return _clientChannel.SendCommandAsync(envelope);
        }
    }
}
