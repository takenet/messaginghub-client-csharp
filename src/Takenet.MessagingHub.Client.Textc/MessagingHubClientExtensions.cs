// ReSharper disable once CheckNamespace

using Takenet.MessagingHub.Client.Textc;

namespace Takenet.MessagingHub.Client
{
    public static class MessagingHubClientExtensions
    {
        public static TextcMessageReceiverBuilder NewTextMessageReceiverBuilder(this MessagingHubClient client)
        {
            return new TextcMessageReceiverBuilder(client);
        }
    }
}