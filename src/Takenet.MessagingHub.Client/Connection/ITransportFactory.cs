using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lime.Protocol.Network;

namespace Takenet.MessagingHub.Client.Connection
{
    public interface ITransportFactory
    {
        ITransport Create(Uri endpoint);
    }
}
