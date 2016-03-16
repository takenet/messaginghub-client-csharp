using Takenet.MessagingHub.Client.Receivers;

namespace Takenet.MessagingHub.Client.Host
{
    public class MessageApplicationReceiver : ApplicationReceiver
    {
        /// <summary>
        /// Gets or sets the type of the media.
        /// </summary>
        /// <value>
        /// The type of the media.
        /// </value>
        public string MediaType { get; set; }

        /// <summary>
        /// Gets or sets the content filter. It can be a regex.
        /// </summary>
        /// <value>
        /// The text regex.
        /// </value>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the sender filter. It can be a regex.
        /// </summary>
        /// <value>
        /// From.
        /// </value>
        public string Sender { get; set; }
    }
}