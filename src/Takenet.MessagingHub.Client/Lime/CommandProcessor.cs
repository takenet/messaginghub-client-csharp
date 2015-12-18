using Lime.Protocol;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol.Client;

namespace Takenet.MessagingHub.Client.Lime
{
    /// <summary>
    /// Implementation for <see cref="ICommandProcessor"/>
    /// </summary>
    internal class CommandProcessor : ICommandProcessor
    {
        private readonly IClientChannel _clientChannel;

        private ConcurrentDictionary<Guid, TaskCompletionSource<Command>> _activeRequests;
        private Task _listenner;
        private CancellationTokenSource _cancellationTokenSource;

        public CommandProcessor(IClientChannel clientChannel)
        {
            _clientChannel = clientChannel;
        }

        public void StartReceiving()
        {
            _activeRequests = new ConcurrentDictionary<Guid, TaskCompletionSource<Command>>();
            _cancellationTokenSource = new CancellationTokenSource();
            _listenner = Task.Run(ListenForEnvelopesAsync);
        }

        public async Task StopReceivingAsync()
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
                await _listenner;
                _cancellationTokenSource.Dispose();
            }
        }

        public async Task<Command> SendAsync(Command envelope, TimeSpan timeout)
        {
            if (envelope == null) throw new ArgumentNullException(nameof(envelope));
            if (timeout <= TimeSpan.Zero) throw new ArgumentException("Timeout value must be positive");
            
            TaskCompletionSource<Command> taskCompletionSource;

            if (envelope.Id == Guid.Empty)
            {
                envelope.Id = Guid.NewGuid();
            }
            else
            {
                if (_activeRequests.TryGetValue(envelope.Id, out taskCompletionSource))
                {
                    throw new ArgumentException("There is already an active request with the envelope id");
                }
            }

            taskCompletionSource = new TaskCompletionSource<Command>();
            _activeRequests.TryAdd(envelope.Id, taskCompletionSource);
            
            using (var cancellationTokenSource = new CancellationTokenSource(timeout))
            {
                try
                {
                    cancellationTokenSource.Token.Register(() => taskCompletionSource.TrySetCanceled());
                    await SendAsync(envelope);
                    return await taskCompletionSource.Task;
                }
                catch (OperationCanceledException)
                {
                    throw new TimeoutException("Timeout expired");
                }
                finally
                {
                    TaskCompletionSource<Command> result;
                    _activeRequests.TryRemove(envelope.Id, out result);
                }
            }
        }

        private async Task ListenForEnvelopesAsync()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                Command command;

                try
                {
                    command = await ReceiveAsync(_cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    if (_cancellationTokenSource.IsCancellationRequested)
                        break;

                    throw;
                }

                TaskCompletionSource<Command> taskCompletionSource;
                if (_activeRequests.TryGetValue(command.Id, out taskCompletionSource))
                {
                    taskCompletionSource.TrySetResult(command);
                }
            }
        }

        private Task<Command> ReceiveAsync(CancellationToken cancellationToken)
        {
            return _clientChannel.ReceiveCommandAsync(cancellationToken);
        }

        private Task SendAsync(Command command)
        {
            return _clientChannel.SendCommandAsync(command);
        }
    }
}
