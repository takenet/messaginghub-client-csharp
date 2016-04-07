using Lime.Protocol;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System;

namespace Takenet.MessagingHub.Documents
{
    /// <summary>
    /// Document containing text plus links to additional multimedia content
    /// </summary>
    [DataContract]
    public class Multipart : Document
    {
        public const string MIME_TYPE = "application/vnd.messagingHub.multipart+json";
        public static MediaType MediaType = MediaType.Parse(MIME_TYPE);

        public Multipart() 
            : base(MediaType)
        {
        }

        [DataMember(Name = "text")]
        public TextPart Text { get; set; }

        [DataMember(Name = "attachments")]
        public AttachmentPart[] Attachments { get; set; }
    }
}
