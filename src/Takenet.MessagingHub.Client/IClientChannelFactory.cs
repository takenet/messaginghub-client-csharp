using Lime.Protocol.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client
{
    internal interface IClientChannelFactory
    {
        Task<IClientChannel> CreateClientChannelAsync(Uri endpoint);
    }
}
