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
        public IClientChannel ClientChannel { get; private set; }
        public Session Session { get; private set; }
        public bool ClientChannelCreated { get; private set; }

        public MessagingHubClientSUT(string hostname) : base(hostname)
        {
            ClientChannel = Substitute.For<IClientChannel>();
            Session = new Session
            {
                State = SessionState.Established
            };
        }

        public void SetSessionResult(SessionState state, Reason reason = null)
        {
            Session.State = state;
            Session.Reason = reason;
        }

        internal override Task<IClientChannel> CreateAndOpenAsync()
        {
            ClientChannelCreated = true;
            return Task.FromResult(ClientChannel);
        }

        internal override Task<Session> EstablishSession(Authentication authentication)
        {
            return Task.FromResult(Session);
        }
    }
}
