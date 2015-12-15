using Lime.Protocol;
using Lime.Protocol.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client.Lime
{
    internal interface IEnvelopeProcessorFactory<T> where T : Envelope
    {
        IEnvelopeProcessor<T> Create(IClientChannel clientChannel);
    }
}
