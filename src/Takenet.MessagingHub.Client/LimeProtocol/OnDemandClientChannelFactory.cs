using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lime.Protocol.Client;

namespace Takenet.MessagingHub.Client.LimeProtocol
{
    internal class OnDemandClientChannelFactory : IOnDemandClientChannelFactory
    {
        public IOnDemandClientChannel Create(IEstablishedClientChannelBuilder establishedClientChannelBuilder)
        {
            return new OnDemandClientChannel(establishedClientChannelBuilder);
        }
    }
}
