using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;

namespace Takenet.MessagingHub.Client.Sender
{
    /// <summary>
    /// Extension methods to simplify the usage of the <see cref="MessagingHubSender"/>
    /// </summary>
    public static class MessagingHubSenderExtensions
    {
        /// <summary>
        /// Send a message through the Messaging Hub
        /// </summary>
        /// <param name="sender">The sender of the message</param>
        /// <param name="content">The content of the message</param>
        /// <param name="to">The destination of the message</param>
        /// <param name="token">A cancellation token to allow the task to be canceled</param>
        public static Task SendMessageAsync(this IMessagingHubSender sender, string content, string to, CancellationToken token = default(CancellationToken))
            => sender.SendMessageAsync(content, Node.Parse(to), token);

        /// <summary>
        /// Send a message through the Messaging Hub
        /// </summary>
        /// <param name="sender">The sender of the message</param>
        /// <param name="content">The content of the message</param>
        /// <param name="to">The destination of the message</param>
        /// <param name="token">A cancellation token to allow the task to be canceled</param>
        public static Task SendMessageAsync(this IMessagingHubSender sender, string content, Node to, CancellationToken token = default(CancellationToken))
            => sender.SendMessageAsync(CreatePlainTextContent(content), to, token);

        /// <summary>
        /// Send a message through the Messaging Hub
        /// </summary>
        /// <param name="sender">The sender of the message</param>
        /// <param name="content">The content of the message</param>
        /// <param name="to">The destination of the message</param>
        /// <param name="token">A cancellation token to allow the task to be canceled</param>
        public static Task SendMessageAsync(this IMessagingHubSender sender, Document content, string to, CancellationToken token = default(CancellationToken))
        => sender.SendMessageAsync(content, Node.Parse(to), token);

        /// <summary>
        /// Send a message through the Messaging Hub
        /// </summary>
        /// <param name="sender">The sender of the message</param>
        /// <param name="content">The content of the message</param>
        /// <param name="to">The destination of the message</param>
        /// <param name="token">A cancellation token to allow the task to be canceled</param>
        public static Task SendMessageAsync(this IMessagingHubSender sender, Document content, Node to, CancellationToken token = default(CancellationToken))
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