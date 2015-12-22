using System;
using System.Threading.Tasks;
using Lime.Protocol.Client;
using Lime.Protocol.Network;
using System.Threading;
using Lime.Transport.Tcp;
using Lime.Protocol;
using Lime.Protocol.Security;

namespace Takenet.MessagingHub.Client
{
    /// <summary>
    /// Implementation for <see cref="IClientChannelFactory"/>
    /// </summary>
    internal class ClientChannelFactory : IClientChannelFactory
    {
        public async Task<IClientChannel> CreateClientChannelAsync(Uri endpoint)
        {
            var transport = new TcpTransport(traceWriter: new TraceWriter());
            var sendTimeout = TimeSpan.FromSeconds(3);

            using (var cancellationTokenSource = new CancellationTokenSource(sendTimeout))
            {
                await transport.OpenAsync(endpoint, cancellationTokenSource.Token);
            }

            var clientChannel = new ClientChannel(transport, sendTimeout);

            return clientChannel;
        }

        public Task<IPersistentClientChannel> CreatePersistentClientChannelAsync(Uri endpoint, TimeSpan sendTimeout, Identity identity, Authentication authentication, ISessionFactory sessionFactory)
        {
            var transport = new TcpTransport(traceWriter: new TraceWriter());
            
            var clientChannel = new PersistentClientChannel(transport, sendTimeout, endpoint, identity, authentication, sessionFactory);

            return Task.FromResult<IPersistentClientChannel>(clientChannel);
        }
    }
}
