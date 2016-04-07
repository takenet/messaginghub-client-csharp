using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;

namespace Takenet.MessagingHub.Client.Sender
{
    /// <summary>
    /// Extension methods for <see cref="IMessagingHubSender"/>
    /// </summary>
    public static class MessagingHubSenderExtensions
    {
        public static Task SendMessageAsync(this IMessagingHubSender sender, string content, string to, CancellationToken token)
            => sender.SendMessageAsync(content, Node.Parse(to), token);

        public static Task SendMessageAsync(this IMessagingHubSender sender, string content, Node to, CancellationToken token)
            => sender.SendMessageAsync(CreatePlainTextContent(content), to, token);

        public static Task SendMessageAsync(this IMessagingHubSender sender, Document content, string to, CancellationToken token)
        => sender.SendMessageAsync(content, Node.Parse(to), token);

        public static Task SendMessageAsync(this IMessagingHubSender sender, Document content, Node to, CancellationToken token)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            var message = new Message
            {
                To = to,
                Content = content
            };
            return sender.SendMessageAsync(message, token);
        }

        private static PlainText CreatePlainTextContent(string content) => new PlainText { Text = content };
    }
}