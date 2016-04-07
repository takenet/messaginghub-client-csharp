using Lime.Protocol;
using System.Runtime.Serialization;

namespace Takenet.MessagingHub.Documents
{
    /// <summary>
    /// Markdown formatted content
    /// </summary>
    [DataContract]
    public class MarkdownText : Document
    {
        public const string MIME_TYPE = "text/markdown";
        public static MediaType MediaType = MediaType.Parse(MIME_TYPE);

        public MarkdownText() 
            : base(MediaType)
        {
        }

        public MarkdownText(string content) 
            : this()
        {
            Content = content;
        }

        [DataMember(Name = "content")]
        public string Content { get; set; }

        public static implicit operator TextPart(MarkdownText markdownText) =>
            new TextPart { Content = markdownText.Content, Type = markdownText.GetMediaType() };
    }
}
