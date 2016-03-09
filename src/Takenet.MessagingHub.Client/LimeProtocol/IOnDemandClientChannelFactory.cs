using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lime.Protocol.Client;

namespace Takenet.MessagingHub.Client.LimeProtocol
{
    internal interface IOnDemandClientChannelFactory
    {
        IOnDemandClientChannel Create(IEstablishedClientChannelBuilder establishedClientChannelBuilder);
    }
}
