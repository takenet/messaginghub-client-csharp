using Lime.Protocol;
using System;
using System.Threading;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client.Extensions.Threads
{
    public class ThreadExtension : ExtensionBase, IThreadExtension
    {
        public const string THREADS_URI = "/threads";

        public ThreadExtension(IMessagingHubSender sender) 
            : base(sender)
        {
        }

        public Task<DocumentCollection> GetThreadsAsync(CancellationToken cancellationToken)
        {
            var requestCommand = CreateGetCommandRequest(THREADS_URI);
            return ProcessCommandAsync<DocumentCollection>(requestCommand, cancellationToken);
        }

        public Task<DocumentCollection> GetThreadAsync(Identity identity, CancellationToken cancellationToken = default(CancellationToken))
        {
            return GetThreadAsync(identity, 20, cancellationToken);
        }

        public Task<DocumentCollection> GetThreadAsync(Identity identity, int take, CancellationToken cancellationToken = default(CancellationToken))
        {
            var requestUri = $"{THREADS_URI}/{identity}?$take={take}";
            var requestCommand = CreateGetCommandRequest(requestUri);
            return ProcessCommandAsync<DocumentCollection>(requestCommand, cancellationToken);
        }
    }
}
