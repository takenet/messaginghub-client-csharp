using Takenet.MessagingHub.Client.Connection;

namespace Takenet.MessagingHub.Client
{
    public class MessagingHubClientBuilder : MessagingHubConnectionBuilder<MessagingHubClientBuilder>
    {
        /// <summary>
        /// Builds a <see cref="IMessagingHubClient"/> with the configured parameters
        /// </summary>
        public new IMessagingHubClient Build()
        {
            var connection = base.Build();
            var client = new MessagingHubClient(connection);
            return client;
        }
    }
}
