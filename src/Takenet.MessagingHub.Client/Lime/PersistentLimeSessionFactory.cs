using Lime.Protocol;
using Lime.Protocol.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client
{
    internal class PersistentLimeSessionFactory : IPersistentLimeSessionFactory
    {
        public Task<IPersistentLimeSession> CreatePersistentClientChannelAsync(Uri endpoint, TimeSpan sendTimeout, Identity identity, Authentication authentication, IClientChannelFactory clientChannelFactory, ILimeSessionProvider limeSessionProvider)
        {
            var persistentClientChannel = new PersistentLimeSession(endpoint, identity, authentication, sendTimeout, clientChannelFactory, limeSessionProvider);

            return Task.FromResult<IPersistentLimeSession>(persistentClientChannel);
        }
    }
}
