using Lime.Protocol;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client.Lime
{
    internal abstract class EnvelopeProcessor<T>
        where T: Envelope
    {

        protected ConcurrentDictionary<Guid, TaskCompletionSource<T>> _activeRequests;
        private Task _listenner;
        private CancellationTokenSource _cancellationTokenSource;

        public EnvelopeProcessor() { }

        public void Start()
        {
            _activeRequests = new ConcurrentDictionary<Guid, TaskCompletionSource<T>>();
            _cancellationTokenSource = new CancellationTokenSource();
            _listenner = Task.Run(Listen);
        }

        public async Task StopAsync()
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
                await _listenner;
            }
        }

        public async Task<T> SendReceive(T envelope, TimeSpan timeout)
        {
            if (envelope == null) throw new ArgumentNullException("Envelope");
            if (timeout <= TimeSpan.Zero) throw new ArgumentException("Timeout value must be positive");
            
            var taskCompletionSource = new TaskCompletionSource<T>();
            _activeRequests.TryAdd(envelope.Id, taskCompletionSource);
            taskCompletionSource.Task.ContinueWith(e =>
            {
                TaskCompletionSource<T> result;
                _activeRequests.TryRemove(envelope.Id, out result);
            });
            
            using (var cancellationTokenSource = new CancellationTokenSource(timeout))
            {
                cancellationTokenSource.Token.Register(() => taskCompletionSource.TrySetCanceled());

                try
                {
                    await Send(envelope, cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    TaskCompletionSource<T> result;
                    _activeRequests.TryRemove(envelope.Id, out result);
                    throw new TimeoutException("Timeout expired sending the envelope");
                }
                
                try
                {
                    return await taskCompletionSource.Task;
                }
                catch (OperationCanceledException)
                {
                    TaskCompletionSource<T> result;
                    _activeRequests.TryRemove(envelope.Id, out result);
                    throw new TimeoutException("Timeout expired waiting for response envelope");
                }
            }
        }

        private async Task Listen()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                T envelope;

                try
                {
                    envelope = await Receive(_cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    if (_cancellationTokenSource.IsCancellationRequested) break;
                    throw;
                }

                TaskCompletionSource<T> taskCompletionSource;
                if (_activeRequests.TryGetValue(envelope.Id, out taskCompletionSource))
                {
                    taskCompletionSource.TrySetResult(envelope);
                }
            }
        }

        protected abstract Task<T> Receive(CancellationToken cancellationToken);
        protected abstract Task Send(T envelope, CancellationToken cancellationToken);
    }
}
