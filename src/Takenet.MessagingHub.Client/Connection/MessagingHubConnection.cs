using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Resources;
using Lime.Protocol;
using Takenet.MessagingHub.Client.LimeProtocol;
using Lime.Protocol.Listeners;
using Lime.Protocol.Client;
using Lime.Protocol.Network;

namespace Takenet.MessagingHub.Client.Connection
{
    /// <summary>
    ///     Default implementation for <see cref="IMessagingHubConnection" /> class.
    /// </summary>
    public class MessagingHubConnection : IMessagingHubConnection
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly TimeSpan _sendTimeout;
        private readonly IEstablishedClientChannelBuilder _establishedClientChannelBuilder;
        private readonly IOnDemandClientChannelFactory _onDemandClientChannelFactory;

        private IOnDemandClientChannel _onDemandClientChannel;
        private ChannelListener _channelListener;

        private static readonly TimeSpan ChannelDiscardedDelay = TimeSpan.FromMilliseconds(300);

        internal MessagingHubConnection(
            TimeSpan sendTimeout, 
            IOnDemandClientChannelFactory onDemandClientChannelFactory,
            IEstablishedClientChannelBuilder establishedClientChannelBuilder)
        {
            _semaphore = new SemaphoreSlim(1);
            _sendTimeout = sendTimeout;
            _establishedClientChannelBuilder = establishedClientChannelBuilder;
            _onDemandClientChannelFactory = onDemandClientChannelFactory;
        }

        public bool Started { get; private set; }

        public virtual async Task StartAsync()
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);

            try
            {
                if (Started)
                    throw new InvalidOperationException("The client is already started");
                
                _onDemandClientChannel = _onDemandClientChannelFactory.Create(_establishedClientChannelBuilder);
                _onDemandClientChannel.ChannelCreationFailedHandlers.Add(StopOnLimeExceptionAsync);
                _onDemandClientChannel.ChannelDiscardedHandlers.Add(ChannelDiscarded);

                for (var i = 0; i < 3; i++)
                {
                    if (await EnsureConnectionIsOkayAsync()) 
                    {
                        Started = true;
                        return;
                    }
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i)));
                }

                throw new TimeoutException("Could not connect to server!");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public virtual async Task StopAsync()
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);

            try
            {
                if (!Started) throw new InvalidOperationException("The client is not started");

                using (var cancellationTokenSource = new CancellationTokenSource(_sendTimeout))
                {
                    await _onDemandClientChannel.FinishAsync(cancellationTokenSource.Token).ConfigureAwait(false);
                }

                if (_channelListener != null)
                {
                    _channelListener.Stop();

                    // TODO: Signal to the listener to stop consuming the envelopes.
                    //await Task.WhenAll(
                    //    _channelListener.CommandListenerTask, 
                    //    _channelListener.MessageListenerTask,
                    //    _channelListener.NotificationListenerTask)
                    //    .ConfigureAwait(false);

                    _channelListener.DisposeIfDisposable();
                }

                _onDemandClientChannel.DisposeIfDisposable();
                Started = false;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// In this context, a LimeException usually means that some credential information is wrong,
        /// and should be checked.
        /// </summary>
        /// <param name="failedChannelInformation">Information about the failure</param>
        private static Task<bool> StopOnLimeExceptionAsync(FailedChannelInformation failedChannelInformation)
        {
            return (!(failedChannelInformation.Exception is LimeException)).AsCompletedTask();
        }

        private async Task<bool> EnsureConnectionIsOkayAsync()
        {
            try
            {
                using (var cancellationTokenSource = new CancellationTokenSource(_sendTimeout))
                {
                    var command = new Command
                    {
                        Method = CommandMethod.Get,
                        Uri = new LimeUri(UriTemplates.PING)
                    };

                    var result = await _onDemandClientChannel.ProcessCommandAsync(command, cancellationTokenSource.Token).ConfigureAwait(false);
                    return result.Status == CommandStatus.Success;
                }
            }
            catch(LimeException)
            {
                // A LimeException usually means that some credential information is wrong, so throw it to allow client to check
                throw;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static Task ChannelDiscarded(ChannelInformation channelInformation)
        {
            return Task.Delay(ChannelDiscardedDelay);
        }
    }
}
