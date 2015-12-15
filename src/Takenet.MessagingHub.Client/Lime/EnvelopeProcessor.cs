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
    internal abstract class EnvelopeProcessor<T> : IEnvelopeProcessor<T>
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
            _listenner = Task.Run(ListenAsync);
        }

        public async Task StopAsync()
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
                await _listenner;
                _cancellationTokenSource.Dispose();
            }
        }

        public virtual async Task<T> SendReceiveAsync(T envelope, TimeSpan timeout)
        {
            if (envelope == null) throw new ArgumentNullException(nameof(envelope));
            if (timeout <= TimeSpan.Zero) throw new ArgumentException("Timeout value must be positive");
            
            TaskCompletionSource<T> taskCompletionSource;

            if(envelope.Id == null || envelope.Id == Guid.Empty)
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

            taskCompletionSource = new TaskCompletionSource<T>();
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
                    await SendAsync(envelope, cancellationTokenSource.Token);
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

        private async Task ListenAsync()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                T envelope;

                try
                {
                    envelope = await ReceiveAsync(_cancellationTokenSource.Token);
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

        protected abstract Task<T> ReceiveAsync(CancellationToken cancellationToken);
        protected abstract Task SendAsync(T envelope, CancellationToken cancellationToken);
    }
}
