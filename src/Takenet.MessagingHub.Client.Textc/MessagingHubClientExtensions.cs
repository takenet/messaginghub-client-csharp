using Takenet.MessagingHub.Client.Connection;
using Takenet.MessagingHub.Client.Textc;
using Takenet.Textc.Processors;

// ReSharper disable once CheckNamespace
namespace Takenet.MessagingHub.Client
{
    public static class MessagingHubClientExtensions
    {
        /// <summary>
        /// Creates a new builder for the <see cref="TextcMessageReceiver"/> class.
        /// </summary>
        /// <param name="clientBuilder"></param>
        /// <param name="outputProcessor"></param>
        /// <returns></returns>
        public static TextcMessageReceiverBuilder NewTextcMessageReceiverBuilder(this MessagingHubClientBuilder clientBuilder, IOutputProcessor outputProcessor = null)
        {
            return new TextcMessageReceiverBuilder(clientBuilder, outputProcessor);
        }
    }
}