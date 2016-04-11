using Lime.Protocol;

namespace Takenet.MessagingHub.Client.Messages
{
    /// <summary>
    /// Represent common media types
    /// </summary>
    public static class MediaTypes
    {
        /// <summary>
        /// Represents any media type
        /// </summary>
        public static MediaType Any { get; } = new MediaType("*", "*");

        /// <summary>
        /// Represents media type 'text/plain'
        /// </summary>
        public static MediaType PlainText { get; } = new MediaType(MediaType.DiscreteTypes.Text, MediaType.SubTypes.Plain);
    }
}
