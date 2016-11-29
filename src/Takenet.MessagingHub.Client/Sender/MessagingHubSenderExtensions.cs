using Lime.Protocol;
using System;
using System.Threading;
using System.Threading.Tasks;
using Takenet.Iris.Messaging.Contents;

namespace Takenet.MessagingHub.Client.Sender
{
    public static class MessagingHubSenderExtensions
    {
        /// <summary>
        /// Forward a message to a BLIP App account
        /// </summary>
        /// <param name="originalMessage">The message to be forwarded to BLIP</param>
        /// <param name="forwardDestination">The BLIP App number which will received the <paramref name="originalMessage"/></param>
        public static Task ForwardMessageAsync(this IMessagingHubSender sender, Message originalMessage, string forwardDestination, CancellationToken cancellationToken)
        {
            var forwardDestinationNode = GetForwardDestinationNode(forwardDestination);
            var newMessage = new Message
            {
                Id = EnvelopeId.NewId(),
                To = forwardDestinationNode,
                Content = new Forward
                {
                    Attendant = forwardDestinationNode,
                    Customer = originalMessage.From,
                    Content = new DocumentContainer { Value = originalMessage.Content }
                }
            };

            return sender.SendMessageAsync(newMessage, cancellationToken);
        }

        /// <summary>
        /// Forward a reply from a BLIP App account to original sender
        /// </summary>
        /// <param name="replyMessage">The reply message from BLIP App</param>
        public static Task ForwardReplyAsync(this IMessagingHubSender sender, Message replyMessage, CancellationToken cancellationToken)
        {
            var forwardContent = replyMessage.Content as Forward;
            var newMessage = new Message
            {
                Id = replyMessage.Id,
                To = forwardContent.Customer,
                Content = forwardContent.Content.Value
            };

            return sender.SendMessageAsync(newMessage, cancellationToken);
        }

        /// <summary>
        /// Check if a message is a reply from a BLIP App account 
        /// </summary>
        /// <param name="forwardDestination">The BLIP App number which receives messages</param>
        public static bool FromForwarder(this Message message, string forwardDestination)
        {
            return message.Content?.GetMediaType() == Forward.MediaType &&
                   message.From.ToIdentity().Equals(GetForwardDestinationNode(forwardDestination).ToIdentity());

        }

        private static Node GetForwardDestinationNode(string forwardDestination)
        {
            var node = Node.Parse(forwardDestination);
            node.Domain = node.Domain ?? "0mn.io";
            return node;
        }
    }
}
