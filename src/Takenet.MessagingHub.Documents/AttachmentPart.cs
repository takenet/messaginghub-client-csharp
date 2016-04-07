using System;
using System.Runtime.Serialization;

namespace Takenet.MessagingHub.Documents
{
    /// <summary>
    /// Additional content on a <see cref="Multipart"/> document
    /// </summary>
    public class AttachmentPart : Part
    {
        [DataMember(Name = "url")]
        public Uri Url { get; set; }
    }
}