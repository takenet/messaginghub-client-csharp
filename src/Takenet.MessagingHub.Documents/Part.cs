using Lime.Protocol;
using System.Runtime.Serialization;

namespace Takenet.MessagingHub.Documents
{
    [DataContract]
    public abstract class Part
    {
        /// <summary>
        /// MIME type of this part
        /// </summary>
        [DataMember(Name = "type")]
        public MediaType Type { get; set; }
    }
}