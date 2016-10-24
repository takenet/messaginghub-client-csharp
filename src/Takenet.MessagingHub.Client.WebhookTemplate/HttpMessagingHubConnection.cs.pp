using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Lime.Protocol.Client;
using Takenet.MessagingHub.Client.Connection;
using Takenet.MessagingHub.Client.Host;
using Lime.Protocol.Serialization;

namespace $rootnamespace$
{
    public class HttpMessagingHubConnection : IMessagingHubConnection
    {
        public bool IsConnected { get; private set; }

        public TimeSpan SendTimeout { get; private set; }

        public int MaxConnectionRetries { get; set; }

        public IOnDemandClientChannel OnDemandClientChannel { get; }

        public HttpMessagingHubConnection(IEnvelopeBuffer envelopeBuffer, IEnvelopeSerializer serializer, Application applicationSettings)
        {
            OnDemandClientChannel = new HttpOnDemandClientChannel(envelopeBuffer, serializer, applicationSettings);
            SendTimeout = TimeSpan.FromMilliseconds(applicationSettings.SendTimeout <= 0 ? 10000 : applicationSettings.SendTimeout);
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