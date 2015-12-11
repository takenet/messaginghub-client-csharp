using Lime.Protocol;
using Lime.Protocol.Client;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol.Security;

namespace Takenet.MessagingHub.Client.Test
{
    public class MessagingHubClientSUT : MessagingHubClient
    {
        internal MessagingHubClientSUT(IClientChannelFactory clientChannelFactory, ISessionFactory session, string hostname) 
            : base(clientChannelFactory, session, hostname)
        {
        }
    }
}
