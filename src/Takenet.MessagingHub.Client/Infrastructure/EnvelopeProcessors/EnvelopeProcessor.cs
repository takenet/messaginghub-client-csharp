using Lime.Protocol;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client.Lime
{
    /// <summary>
    /// Base class for envelope processors
    /// </summary>
    /// <typeparam name="TEnvelope">Envelope type</typeparam>
    internal abstract class EnvelopeProcessor<TEnvelope> : IEnvelopeProcessor<TEnvelope>
        where TEnvelope: Envelope
    {
        private ConcurrentDictionary<Guid, TaskCompletionSource<TEnvelope>> _activeRequests;
        private Task _listenner;
        private CancellationTokenSource _cancellationTokenSource;

        public void StartReceiving()
        {
            _activeRequests = new ConcurrentDictionary<Guid, TaskCompletionSource<TEnvelope>>();
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

        public virtual async Task<TEnvelope> SendAsync(TEnvelope envelope, TimeSpan timeout)
        {
            if (envelope == null) throw new ArgumentNullException(nameof(envelope));
            if (timeout <= TimeSpan.Zero) throw new ArgumentException("Timeout value must be positive");
            
            TaskCompletionSource<TEnvelope> taskCompletionSource;

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

            taskCompletionSource = new TaskCompletionSource<TEnvelope>();
            _activeRequests.TryAdd(envelope.Id, taskCompletionSource);
            await taskCompletionSource.Task.ContinueWith(e =>
            {
                TaskCompletionSource<TEnvelope> result;
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
                    TaskCompletionSource<TEnvelope> result;
                    _activeRequests.TryRemove(envelope.Id, out result);
                    throw new TimeoutException("Timeout expired sending the envelope");
                }
                
                try
                {
                    return await taskCompletionSource.Task;
                }
                catch (OperationCanceledException)
                {
                    TaskCompletionSource<TEnvelope> result;
                    _activeRequests.TryRemove(envelope.Id, out result);
                    throw new TimeoutException("Timeout expired waiting for response envelope");
                }
            }
        }

        private async Task ListenForEnvelopesAsync()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                TEnvelope envelope;

                try
                {
                    envelope = await ReceiveAsync(_cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    if (_cancellationTokenSource.IsCancellationRequested)
                        break;

                    throw;
                }

                TaskCompletionSource<TEnvelope> taskCompletionSource;
                if (_activeRequests.TryGetValue(envelope.Id, out taskCompletionSource))
                {
                    taskCompletionSource.TrySetResult(envelope);
                }
            }
        }

        /// <summary>
        /// Receives an envelope
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        protected abstract Task<TEnvelope> ReceiveAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Sends an envelope
        /// </summary>
        /// <param name="envelope">Envelope</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        protected abstract Task SendAsync(TEnvelope envelope, CancellationToken cancellationToken);
    }
}
