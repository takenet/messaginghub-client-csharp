using System.Runtime.Serialization;
using Lime.Protocol;

namespace Takenet.MessagingHub.Client.Tester
{
    [DataContract]
    internal class AccountKeyRequest : Document
    {
        public const string MIME_TYPE = "application/vnd.iris.keyRequest+json";
        public static readonly MediaType MediaType = MediaType.Parse(MIME_TYPE);

        public AccountKeyRequest() :
            base(MediaType)
        {

        }

        /// <summary>
        /// The account alternative address, in case of requesting to send the access key to an alternative address, like an email or cellphone.
        /// </summary>
        [DataMember(Name = "alternativeAddress")]
        public Identity AlternativeAddress { get; set; }

        /// <summary>
        /// The key descriptive purpose.
        /// </summary>
        [DataMember(Name = "purpose")]
        public string Purpose { get; set; }

        /// <summary>
        /// The key time to live, in milliseconds.
        /// </summary>
        [DataMember(Name = "ttl")]
        public long? Ttl { get; set; }
    }
}
