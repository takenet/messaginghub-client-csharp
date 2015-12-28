// ReSharper disable once CheckNamespace

using System;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Receivers;
using Takenet.MessagingHub.Client.Text;

namespace Takenet.MessagingHub.Client
{
    public static class MessagingHubClientExtensions
    {
        public static TextMessageReceiverBuilder BuildTextMessageReceiver(this MessagingHubClient client)
        {
            return new TextMessageReceiverBuilder(client);
        }
    }
}