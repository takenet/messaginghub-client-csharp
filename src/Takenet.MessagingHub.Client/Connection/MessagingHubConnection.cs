using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.LimeProtocol;
using Lime.Protocol.Client;
using Lime.Protocol.Network;
using Lime.Protocol.Util;

namespace Takenet.MessagingHub.Client.Connection
{
    public sealed class MessagingHubConnection : IMessagingHubConnection
    {
        private static readonly TimeSpan ChannelDiscardedDelay = TimeSpan.FromSeconds(5);

        private readonly SemaphoreSlim _semaphore;
        private readonly IOnDemandClientChannelFactory _onDemandClientChannelFactory;
        private bool _isDisconnecting;
        private int _channelCount;

        internal MessagingHubConnection(
            TimeSpan sendTimeout,
            int maxConnectionRetries,
            IOnDemandClientChannelFactory onDemandClientChannelFactory,
            int channelCount)
        {
            _semaphore = new SemaphoreSlim(1);
            MaxConnectionRetries = maxConnectionRetries;
            SendTimeout = sendTimeout;
            _onDemandClientChannelFactory = onDemandClientChannelFactory;
            _channelCount = channelCount;
        }

        public bool IsConnected => OnDemandClientChannel?.IsEstablished ?? false;

        public int MaxConnectionRetries { get; set; }

        public TimeSpan SendTimeout { get; }

        public IOnDemandClientChannel OnDemandClientChannel { get; private set; }

        public async Task ConnectAsync(CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                if (IsConnected) throw new InvalidOperationException("The client is already started");

                CreateOnDemandClientChannel();
                using (var cts = new CancellationTokenSource(SendTimeout))
                using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token))
                {
                    await OnDemandClientChannel.EstablishAsync(linkedCts.Token).ConfigureAwait(false);
                }

                _isDisconnecting = false;
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
                if (OnDemandClientChannel != null &&
                    OnDemandClientChannel.IsEstablished)
                {
                    _isDisconnecting = true;

                    using (var cts = new CancellationTokenSource(SendTimeout))
                    using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token))
                    {
                        await OnDemandClientChannel.FinishAsync(linkedCts.Token).ConfigureAwait(false);
                    }
                }

                OnDemandClientChannel?.DisposeIfDisposable();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private void CreateOnDemandClientChannel()
        {
            OnDemandClientChannel = _onDemandClientChannelFactory.Create(_channelCount);
            OnDemandClientChannel.ChannelCreatedHandlers.Add(ChannelCreatedAsync);
            OnDemandClientChannel.ChannelCreationFailedHandlers.Add(ChannelCreationFailedAsync);
            OnDemandClientChannel.ChannelDiscardedHandlers.Add(ChannelDiscardedAsync);
            OnDemandClientChannel.ChannelOperationFailedHandlers.Add(ChannelOperationFailedAsync);
        }

        private Task ChannelCreatedAsync(ChannelInformation channelInformation)
        {
            Trace.TraceInformation("Channel '{0}' created", channelInformation.SessionId);
            return Task.CompletedTask;
        }
        /// <summary>
        /// In this context, a LimeException usually means that some credential information is wrong,
        /// and should be checked.
        /// </summary>
        /// <param name="failedChannelInformation">Information about the failure</param>
        private async Task<bool> ChannelCreationFailedAsync(FailedChannelInformation failedChannelInformation)
        {
            Trace.TraceError("Channel '{0}' operation failed: {1}", failedChannelInformation.SessionId, failedChannelInformation.Exception);
            if (failedChannelInformation.Exception is LimeException) return false;            
            await Task.Delay(ChannelDiscardedDelay).ConfigureAwait(false);
            return !_isDisconnecting;
        }        

        private Task ChannelDiscardedAsync(ChannelInformation channelInformation)
        {
            Trace.TraceInformation("Channel '{0}' discarded", channelInformation.SessionId);
            if (_isDisconnecting) return Task.CompletedTask;
            return Task.Delay(ChannelDiscardedDelay);
        }

        private Task<bool> ChannelOperationFailedAsync(FailedChannelInformation failedChannelInformation)
        {
            Trace.TraceError("Channel '{0}' operation failed: {1}", failedChannelInformation.SessionId, failedChannelInformation.Exception);
            if (_isDisconnecting) return TaskUtil.FalseCompletedTask;
            return TaskUtil.TrueCompletedTask;
        }
    }
}
