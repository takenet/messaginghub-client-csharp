// ReSharper disable once CheckNamespace

using Takenet.MessagingHub.Client.Textc;

namespace Takenet.MessagingHub.Client
{
    public static class MessagingHubClientExtensions
    {
        public static TextMessageReceiverBuilder NewTextMessageReceiverBuilder(this MessagingHubClient client)
        {
            return new TextMessageReceiverBuilder(client);
        }
    }
}