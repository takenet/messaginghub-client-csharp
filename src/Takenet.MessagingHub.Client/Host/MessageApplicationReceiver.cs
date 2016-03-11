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
    }
}