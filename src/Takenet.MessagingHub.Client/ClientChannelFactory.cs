using System;
using System.Threading.Tasks;
using Lime.Protocol.Client;
using Lime.Protocol.Network;
using System.Threading;
using Lime.Transport.Tcp;
using Lime.Protocol;
using Lime.Protocol.Security;
using Lime.Protocol.Serialization.Newtonsoft;

namespace Takenet.MessagingHub.Client
{
    /// <summary>
    /// Implementation for <see cref="IClientChannelFactory"/>
    /// </summary>
    internal class ClientChannelFactory : IClientChannelFactory
    {
        public Task<IPersistentClientChannel> CreatePersistentClientChannelAsync(Uri endpoint, TimeSpan sendTimeout, Identity identity, Authentication authentication)
        {
            var transport = new TcpTransport(traceWriter: new TraceWriter(), envelopeSerializer: new JsonNetSerializer());
            
            var clientChannel = new PersistentClientChannel(transport, sendTimeout, new LimeSessionProvider(endpoint, identity, authentication));

            return Task.FromResult<IPersistentClientChannel>(clientChannel);
        }
    }
}
