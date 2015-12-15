using Lime.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lime.Protocol.Client;

namespace Takenet.MessagingHub.Client.Lime
{
    internal class CommandProcessorFactory : IEnvelopeProcessorFactory<Command>
    {
        public IEnvelopeProcessor<Command> Create(IClientChannel clientChannel)
        {
            return new CommandProcessor(clientChannel);
        }
    }
}
