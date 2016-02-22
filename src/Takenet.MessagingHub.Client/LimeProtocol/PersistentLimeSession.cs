using Lime.Protocol;
using Lime.Protocol.Client;
using Lime.Protocol.Network;
using Lime.Protocol.Security;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client.LimeProtocol
{
    internal class PersistentLimeSession : IPersistentLimeSession
    {
        private readonly TimeSpan _sendTimeout;
        private readonly ILimeSessionProvider _limeSessionProvider;
        private readonly Uri _endPoint;
        private readonly Identity _identity;
        private readonly Authentication _authentication;
        private readonly IClientChannelFactory _clientChannelFactory;

        private SemaphoreSlim _connectionSemaphore;
        private IClientChannel _clientChannel;
        private Task _watchConnectionTask;
        private CancellationTokenSource _cancellationTokenSource;

        public event EventHandler SessionEstabilished;

        private bool _isSessionEstablished => _limeSessionProvider.IsSessionEstablished(_clientChannel);

        private static TimeSpan WatchConnectionDelay => TimeSpan.FromSeconds(2);
        private static TimeSpan ReconnectDelay => TimeSpan.FromSeconds(2);

        internal PersistentLimeSession(Uri endPoint, Identity identity, Authentication authentication, TimeSpan sendTimeout,
            IClientChannelFactory clientChannelFactory, ILimeSessionProvider limeSessionProvider)
        {
            _endPoint = endPoint;
            _identity = identity;
            _authentication = authentication;
            _sendTimeout = sendTimeout;
            _clientChannelFactory = clientChannelFactory;
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

            _connectionSemaphore?.Dispose();
        }

        public Task<Message> ReceiveMessageAsync(CancellationToken cancellationToken)
        {
            return ReceiveAsync(cancellationToken, _clientChannel.ReceiveMessageAsync);
        }

        public Task<Command> ReceiveCommandAsync(CancellationToken cancellationToken)
        {
            return ReceiveAsync(cancellationToken, _clientChannel.ReceiveCommandAsync);
        }

        public Task<Notification> ReceiveNotificationAsync(CancellationToken cancellationToken)
        {
            return ReceiveAsync(cancellationToken, _clientChannel.ReceiveNotificationAsync);
        }

        public Task SendMessageAsync(Message message)
        {
            return SendAsync(message, _clientChannel.SendMessageAsync);
        }

        public Task SendCommandAsync(Command command)
        {
            return SendAsync(command, _clientChannel.SendCommandAsync);
        }

        public Task SendNotificationAsync(Notification notification)
        {
            return SendAsync(notification, _clientChannel.SendNotificationAsync);
        }

        private async Task<T> ReceiveAsync<T>(CancellationToken cancellationToken, Func<CancellationToken, Task<T>> func)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
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
                catch (OperationCanceledException)
                {
                    if (_isSessionEstablished) throw;
                    if (cancellationToken.IsCancellationRequested) throw;
                }

                if (!_isSessionEstablished)
                {
                    await WaitToEstablishSessionAsync(cancellationToken).ConfigureAwait(false);
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
                    await Task.Delay(WatchConnectionDelay, cancellationToken).ConfigureAwait(false);
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
                        await Task.Delay(ReconnectDelay, cancellationToken).ConfigureAwait(false);
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
            _clientChannel?.DisposeIfDisposable();

            _clientChannel = await _clientChannelFactory.CreateClientChannelAsync(_sendTimeout).ConfigureAwait(false);

            await _limeSessionProvider.EstablishSessionAsync(_clientChannel, _endPoint, _identity, _authentication, cancellationToken).ConfigureAwait(false);

            if (_isSessionEstablished && SessionEstabilished != null)
            {
                SessionEstabilished(this, EventArgs.Empty);
            }
        }

        private async Task EndSession(CancellationToken cancellationToken)
        {
            await _limeSessionProvider.FinishSessionAsync(_clientChannel, cancellationToken).ConfigureAwait(false);

            _clientChannel?.DisposeIfDisposable();
        }

        public Task SetResourceAsync<TResource>(LimeUri uri, TResource resource, CancellationToken cancellationToken, Func<Command, Task> unrelatedCommandHandler = null) where TResource : Document
        {
            return _clientChannel.SetResourceAsync(uri, resource, cancellationToken);
        }
    }
}
