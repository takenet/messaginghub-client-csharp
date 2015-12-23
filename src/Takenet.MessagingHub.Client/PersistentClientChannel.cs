using Lime.Protocol;
using Lime.Protocol.Client;
using Lime.Protocol.Network;
using Lime.Protocol.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client
{
    internal class PersistentClientChannel : ClientChannel, IPersistentClientChannel
    {
        private bool _isConnected => Transport.IsConnected && State == SessionState.Established;

        private Identity _identity;
        private Authentication _authentication;
        private ISessionFactory _sessionFactory;
        private SemaphoreSlim _connectionSemaphore;
        private TimeSpan _sendTimeout;
        private Uri _endPoint;
        private Task _watchConnectionTask;
        private CancellationTokenSource _cancellationTokenSource;

        private static TimeSpan _watchConnectionDelay => TimeSpan.FromSeconds(2);
        private static TimeSpan _reconnectDelay => TimeSpan.FromSeconds(2);

        internal PersistentClientChannel(ITransport transport, TimeSpan sendTimeout, Uri endPoint, Identity identity, Authentication authentication, ISessionFactory sessionFactory, int buffersLimit = 5, bool fillEnvelopeRecipients = false, bool autoReplyPings = true, bool autoNotifyReceipt = false, TimeSpan? remotePingInterval = default(TimeSpan?), TimeSpan? remoteIdleTimeout = default(TimeSpan?))
            : base(transport, sendTimeout, buffersLimit, fillEnvelopeRecipients, autoReplyPings, autoNotifyReceipt, remotePingInterval, remoteIdleTimeout)
        {
            _identity = identity;
            _authentication = authentication;
            _sessionFactory = sessionFactory;
            _sendTimeout = sendTimeout;
            _endPoint = endPoint;
        }

        public async Task StartAsync()
        {
            _connectionSemaphore = new SemaphoreSlim(1);
            using (var cancellationTokenSource = new CancellationTokenSource(_sendTimeout))
            {
                await Connect(cancellationTokenSource.Token).ConfigureAwait(false);
            }
            _cancellationTokenSource = new CancellationTokenSource();
            _watchConnectionTask = Task.Run(() => WatchConnection(_cancellationTokenSource.Token));
        }

        public async Task StopAsync()
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
            }

            await _watchConnectionTask;

            using (var cancellationTokenSource = new CancellationTokenSource(_sendTimeout))
            {
                await Disconnect(cancellationTokenSource.Token);
            }

            if (_connectionSemaphore != null)
            {
                _connectionSemaphore.Dispose();
            }
        }

        public override Task<Message> ReceiveMessageAsync(CancellationToken cancellationToken)
        {
            return ReceiveAsync(cancellationToken, base.ReceiveMessageAsync);
        }

        public override Task<Command> ReceiveCommandAsync(CancellationToken cancellationToken)
        {
            return ReceiveAsync(cancellationToken, base.ReceiveCommandAsync);
        }

        public override Task<Notification> ReceiveNotificationAsync(CancellationToken cancellationToken)
        {
            return ReceiveAsync(cancellationToken, base.ReceiveNotificationAsync);
        }

        public override Task SendMessageAsync(Message message)
        {
            return SendAsync(message, base.SendMessageAsync);
        }

        public override Task SendCommandAsync(Command command)
        {
            return SendAsync(command, base.SendCommandAsync);
        }

        public override Task SendNotificationAsync(Notification notification)
        {
            return SendAsync(notification, base.SendNotificationAsync);
        }

        private async Task<T> ReceiveAsync<T>(CancellationToken cancellationToken, Func<CancellationToken, Task<T>> func)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!_isConnected)
                {
                    await EstabilishSessionAsync(cancellationToken).ConfigureAwait(false);
                }

                try
                {
                    return await func(cancellationToken).ConfigureAwait(false);
                }
                catch (LimeException)
                {
                    if (_isConnected) throw;
                }
                catch (SocketException)
                {
                    if (_isConnected) throw;
                }
                catch (IOException)
                {
                    if (_isConnected) throw;
                }
            }

            throw new OperationCanceledException();
        }

        private async Task SendAsync<T>(T envelope, Func<T, Task> func)
        {
            if (!_isConnected)
            {
                using (var cancellationTokenSource = new CancellationTokenSource(_sendTimeout))
                {
                    await EstabilishSessionAsync(cancellationTokenSource.Token).ConfigureAwait(false);
                }
            }

            await func(envelope).ConfigureAwait(false);
        }

        private async Task WatchConnection(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!_isConnected)
                {
                    await EstabilishSessionAsync(cancellationToken).ConfigureAwait(false);
                }

                try
                {
                    await Task.Delay(_watchConnectionDelay, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    if (cancellationToken.IsCancellationRequested) continue;
                    throw;
                }
            }
        }

        private async Task EstabilishSessionAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (_isConnected)
                {
                    return;
                }

                try
                {
                    await _connectionSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    if (cancellationToken.IsCancellationRequested) return;
                    throw;
                }
                
                while (!_isConnected && !cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        await Connect(cancellationToken).ConfigureAwait(false);
                    }
                    catch { }

                    try
                    {
                        await Task.Delay(_reconnectDelay, cancellationToken).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        if (cancellationToken.IsCancellationRequested) continue;
                        throw;
                    }
                }
            }
            finally
            {
                _connectionSemaphore.Release();
            }
        }

        private async Task Connect(CancellationToken cancellationToken)
        {
            if (_isConnected)
            {
                return;
            }

            await Transport.OpenAsync(_endPoint, cancellationToken).ConfigureAwait(false);

            await _sessionFactory.CreateSessionAsync(this, _identity, _authentication).ConfigureAwait(false);
        }

        private async Task Disconnect(CancellationToken cancellationToken)
        {
            if (_isConnected)
            {
                await SendFinishingSessionAsync();
            }

            if (Transport.IsConnected)
            {
                await Transport.CloseAsync(cancellationToken);
            }
        }
    }
}
