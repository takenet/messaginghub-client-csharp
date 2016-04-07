using Lime.Protocol;
using System.Runtime.Serialization;

namespace Takenet.MessagingHub.Documents
{
    /// <summary>
    /// Text part of a <see cref="Multipart"/> document
    /// </summary>
    public class TextPart : Part
    {
        [DataMember(Name = "content")]
        public string Content { get; set; }

        public static implicit operator PlainDocument(TextPart textDocument) => 
            new PlainDocument(textDocument.Content, textDocument.Type);

        public static implicit operator TextPart(PlainDocument plainDocument) => 
            new TextPart { Content = plainDocument.Value, Type = plainDocument.GetMediaType() };
    }
}