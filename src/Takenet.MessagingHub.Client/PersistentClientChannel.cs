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
        private bool _isSessionEstablished => _limeSessionProvider.IsSessionEstablished(this);
        
        private SemaphoreSlim _connectionSemaphore;
        private TimeSpan _sendTimeout;
        private Task _watchConnectionTask;
        private CancellationTokenSource _cancellationTokenSource;
        private ILimeSessionProvider _limeSessionProvider;

        public Session Session { get; private set; }

        private static TimeSpan _watchConnectionDelay => TimeSpan.FromSeconds(2);
        private static TimeSpan _reconnectDelay => TimeSpan.FromSeconds(2);

        internal PersistentClientChannel(ITransport transport, TimeSpan sendTimeout, ILimeSessionProvider limeSessionProvider, int buffersLimit = 5, bool fillEnvelopeRecipients = false, bool autoReplyPings = true, bool autoNotifyReceipt = false, TimeSpan? remotePingInterval = default(TimeSpan?), TimeSpan? remoteIdleTimeout = default(TimeSpan?))
            : base(transport, sendTimeout, buffersLimit, fillEnvelopeRecipients, autoReplyPings, autoNotifyReceipt, remotePingInterval, remoteIdleTimeout)
        {
            _sendTimeout = sendTimeout;
            _limeSessionProvider = limeSessionProvider;
        }
        
        public async Task StartAsync()
        {
            _connectionSemaphore = new SemaphoreSlim(1);
            using (var cancellationTokenSource = new CancellationTokenSource(_sendTimeout))
            {
                await EstablishSession(cancellationTokenSource.Token).ConfigureAwait(false);
            }

            if (!_isSessionEstablished)
            {
                throw new Exception("Could not establish session");
            }

            _cancellationTokenSource = new CancellationTokenSource();
            _watchConnectionTask = Task.Run(() => WatchSession(_cancellationTokenSource.Token));
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
                await EndSession(cancellationTokenSource.Token);
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
                if (!_isSessionEstablished)
                {
                    await WaitToEstablishSessionAsync(cancellationToken).ConfigureAwait(false);
                }

                try
                {
                    return await func(cancellationToken).ConfigureAwait(false);
                }
                catch (LimeException)
                {
                    if (_isSessionEstablished) throw;
                }
                catch (SocketException)
                {
                    if (_isSessionEstablished) throw;
                }
                catch (IOException)
                {
                    if (_isSessionEstablished) throw;
                }
            }

            throw new OperationCanceledException();
        }

        private async Task SendAsync<T>(T envelope, Func<T, Task> func)
        {
            if (!_isSessionEstablished)
            {
                using (var cancellationTokenSource = new CancellationTokenSource(_sendTimeout))
                {
                    await WaitToEstablishSessionAsync(cancellationTokenSource.Token).ConfigureAwait(false);

                    if (cancellationTokenSource.IsCancellationRequested)
                    {
                        throw new OperationCanceledException();
                    }
                }
            }

            await func(envelope).ConfigureAwait(false);
        }

        private async Task WatchSession(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!_isSessionEstablished)
                {
                    await WaitToEstablishSessionAsync(cancellationToken).ConfigureAwait(false);
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

        private async Task WaitToEstablishSessionAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (_isSessionEstablished)
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
                
                while (!_isSessionEstablished && !cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        await EstablishSession(cancellationToken).ConfigureAwait(false);
                    }
                    catch { }

                    if (_isSessionEstablished)
                    {
                        break;
                    }

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

                if (cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException();
                }
            }
            finally
            {
                _connectionSemaphore.Release();
            }
        }

        private async Task EstablishSession(CancellationToken cancellationToken)
        {
            var session = await _limeSessionProvider.EstablishSessionAsync(this, cancellationToken);
            State = session.State;
            Session = session;
        }

        private Task EndSession(CancellationToken cancellationToken)
        {
            return _limeSessionProvider.FinishSessionAsync(this, cancellationToken);
        }
    }
}
