using System;
using System.Threading.Tasks;
using Lime.Protocol.Client;
using Lime.Protocol.Network;
using System.Threading;
using Lime.Transport.Tcp;

namespace Takenet.MessagingHub.Client
{
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
    }
}
