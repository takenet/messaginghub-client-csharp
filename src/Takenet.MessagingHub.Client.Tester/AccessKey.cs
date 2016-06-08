using System;
using System.Runtime.Serialization;
using Lime.Protocol;

namespace Takenet.MessagingHub.Client.Tester
{
    [DataContract]
    internal class AccessKey : Document
    {
        public const string MIME_TYPE = "application/vnd.iris.accessKey+json";
        public static readonly MediaType MediaType = MediaType.Parse(MIME_TYPE);

        public AccessKey()
            : base(MediaType)
        {
        }

        /// <summary>
        /// The key id, for further reference.
        /// </summary>
        [DataMember(Name = "id")]
        public Guid? Id { get; set; }

        /// <summary>
        /// The account identity of the key.
        /// </summary>
        [DataMember(Name = "account")]
        public Identity Account { get; set; }

        /// <summary>
        /// The base64 representation of the actual key. This value can be retrieved only in the moment of the creation.
        /// </summary>
        [DataMember(Name = "key")]
        public string Key { get; set; }

        /// <summary>
        /// The key descriptive purpose.
        /// </summary>
        [DataMember(Name = "purpose")]
        public string Purpose { get; set; }

        /// <summary>
        /// The key expiration date.
        /// </summary>
        [DataMember(Name = "expiration")]
        public DateTimeOffset? Expiration { get; set; }

        /// <summary>
        /// The account that required the access key, in case of a guest access requesting an key for an alternative address.
        /// </summary>
        [DataMember(Name = "requirer")]
        public Node Requirer { get; set; }

        public override string ToString()
        {
            return $"{Account}:{Key}:{Requirer}";
        }

        public static AccessKey Parse(string value)
        {
            var values = value.Split(':');

            return new AccessKey()
            {
                Account = Identity.Parse(values[0]),
                Key = values[1],
                Requirer = Node.Parse(values[2])
            };
        }
    }
}
