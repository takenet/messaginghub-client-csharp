using Lime.Protocol.Listeners;
using Lime.Protocol.Server;
using System.Threading;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client
{
    public interface IMessagingHubClient : IMessagingHubSender, IStoppable
    {
        /// <summary>
        /// Starts the client with the specified listener instance.
        /// </summary>
        /// <param name="channelListener">The listener for consuming received envelopes.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task StartAsync(IChannelListener channelListener, CancellationToken cancellationToken);
    }
}