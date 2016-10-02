using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Lime.Protocol.Client;
using Takenet.MessagingHub.Client.Connection;

namespace Takenet.MessagingHub.Client.WebHost
{
    public class HttpMessagingHubConnection : IMessagingHubConnection
    {
        public bool IsConnected { get; private set; }

        public TimeSpan SendTimeout { get; }

        public int MaxConnectionRetries { get; set; }

        public IOnDemandClientChannel OnDemandClientChannel { get; }

        public HttpMessagingHubConnection(IEnvelopeBuffer envelopeBuffer)
        {
            OnDemandClientChannel = new HttpOnDemandClientChannel(envelopeBuffer);
        }

        public Task ConnectAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            IsConnected = true;
            return Task.CompletedTask;
        }

        public Task DisconnectAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            IsConnected = false;
            return Task.CompletedTask;
        }
    }
}