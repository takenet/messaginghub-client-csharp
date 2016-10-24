using Takenet.MessagingHub.Client.Connection;

namespace Takenet.MessagingHub.Client
{
    public class MessagingHubClientBuilder : MessagingHubConnectionBuilder<MessagingHubClientBuilder>
    {
        public MessagingHubClientBuilder()
            : this(new TcpTransportFactory())
        {
            
        }

        public MessagingHubClientBuilder(ITransportFactory transportFactory) 
            : base(transportFactory)
        {
        }

        /// <summary>
        /// Builds a <see cref="IMessagingHubClient"/> with the configured parameters
        /// </summary>
        public new IMessagingHubClient Build()
        {
            var connection = base.Build();
            var client = new MessagingHubClient(connection, this.AutoNotify);
            return client;
        }
    }
}
