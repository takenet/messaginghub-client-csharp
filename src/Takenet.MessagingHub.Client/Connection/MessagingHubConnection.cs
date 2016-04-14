using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Resources;
using Lime.Protocol;
using Takenet.MessagingHub.Client.LimeProtocol;
using Lime.Protocol.Client;
using Lime.Protocol.Network;


namespace Takenet.MessagingHub.Client.Connection
{
    public sealed class MessagingHubConnection : IMessagingHubConnection
    {
        private static readonly TimeSpan ChannelDiscardedDelay = TimeSpan.FromMilliseconds(300);

        private readonly SemaphoreSlim _semaphore;
        private readonly IOnDemandClientChannelFactory _onDemandClientChannelFactory;

        internal MessagingHubConnection(
            TimeSpan sendTimeout,
            int maxConnectionRetries,
            IOnDemandClientChannelFactory onDemandClientChannelFactory)
        {
            _semaphore = new SemaphoreSlim(1);
            MaxConnectionRetries = maxConnectionRetries;
            SendTimeout = sendTimeout;
            _onDemandClientChannelFactory = onDemandClientChannelFactory;
        }

        public bool IsConnected { get; private set; }

        public int MaxConnectionRetries { get; set; }

        public TimeSpan SendTimeout { get; }

        public IOnDemandClientChannel OnDemandClientChannel { get; private set; }

        public async Task ConnectAsync(CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                if (IsConnected)
                    throw new InvalidOperationException("The client is already started");

                CreateOnDemandClientChannel();

                for (var i = 0; i < MaxConnectionRetries; i++)
                {
                    if (await EnsureConnectionIsOkayAsync()) 
                    {
                        IsConnected = true;
                        return;
                    }
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i)), cancellationToken);
                }

                throw new TimeoutException("Could not connect to server!");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task DisconnectAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                if (!IsConnected) throw new InvalidOperationException("The client is not started");

                using (var cancellationTokenSource = new CancellationTokenSource(SendTimeout))
                {
                    await OnDemandClientChannel.FinishAsync(cancellationTokenSource.Token).ConfigureAwait(false);
                }

                OnDemandClientChannel.DisposeIfDisposable();
                IsConnected = false;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private void CreateOnDemandClientChannel()
        {
            OnDemandClientChannel = _onDemandClientChannelFactory.Create();
            OnDemandClientChannel.ChannelCreationFailedHandlers.Add(StopOnLimeExceptionAsync);
            OnDemandClientChannel.ChannelDiscardedHandlers.Add(ChannelDiscarded);
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
            catch (Exception ex)
            {
                Trace.WriteLine($"Exception connecting to Messaging Hub: {ex}");
                return false;
            }
        }

        private static Task ChannelDiscarded(ChannelInformation channelInformation)
        {
            return Task.Delay(ChannelDiscardedDelay);
        }
    }
}
