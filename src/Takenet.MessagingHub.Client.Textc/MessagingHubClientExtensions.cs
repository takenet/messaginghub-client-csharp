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
        /// <param name="senderBuilder"></param>
        /// <param name="outputProcessor"></param>
        /// <returns></returns>
        public static TextcMessageReceiverBuilder NewTextcMessageReceiverBuilder(this MessagingHubSenderBuilder senderBuilder, IOutputProcessor outputProcessor = null)
        {
            return new TextcMessageReceiverBuilder(senderBuilder, outputProcessor);
        }

        public static TextcMessageReceiverBuilder NewTextcMessageReceiverBuilder(this MessagingHubClientBuilder clientBuilder, IOutputProcessor outputProcessor = null)
        {
            return new TextcMessageReceiverBuilder(clientBuilder.AsMessagingSenderBuilder(), outputProcessor);
        }
    }
}