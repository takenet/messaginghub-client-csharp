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
    public class MessagingHubConnection : IWorker
    {
        public TimeSpan SendTimeout { get; }
        internal IOnDemandClientChannel OnDemandClientChannel { get; private set; }

        private readonly SemaphoreSlim _semaphore;
        private readonly IEstablishedClientChannelBuilder _establishedClientChannelBuilder;
        private readonly IOnDemandClientChannelFactory _onDemandClientChannelFactory;

        private ChannelListener _channelListener;

        private static readonly TimeSpan ChannelDiscardedDelay = TimeSpan.FromMilliseconds(300);

        internal MessagingHubConnection(
            TimeSpan sendTimeout, 
            IOnDemandClientChannelFactory onDemandClientChannelFactory,
            IEstablishedClientChannelBuilder establishedClientChannelBuilder)
        {
            _semaphore = new SemaphoreSlim(1);
            SendTimeout = sendTimeout;
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
                
                OnDemandClientChannel = _onDemandClientChannelFactory.Create(_establishedClientChannelBuilder);
                OnDemandClientChannel.ChannelCreationFailedHandlers.Add(StopOnLimeExceptionAsync);
                OnDemandClientChannel.ChannelDiscardedHandlers.Add(ChannelDiscarded);

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

                using (var cancellationTokenSource = new CancellationTokenSource(SendTimeout))
                {
                    await OnDemandClientChannel.FinishAsync(cancellationTokenSource.Token).ConfigureAwait(false);
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

                OnDemandClientChannel.DisposeIfDisposable();
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
                using (var cancellationTokenSource = new CancellationTokenSource(SendTimeout))
                {
                    var command = new Command
                    {
                        Method = CommandMethod.Get,
                        Uri = new LimeUri(UriTemplates.PING)
                    };

                    var result = await OnDemandClientChannel.ProcessCommandAsync(command, cancellationTokenSource.Token).ConfigureAwait(false);
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
