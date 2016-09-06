using System;
using System.Runtime.Serialization;
using Lime.Protocol;

namespace Takenet.MessagingHub.Client.Extensions.Scheduler
{
    [DataContract]
    public class Schedule : Document
    {
        public const string MIME_TYPE = "application/vnd.iris.schedule+json";
        public static readonly MediaType MediaType = MediaType.Parse(MIME_TYPE);

        public const string WHEN = "when";
        public const string MESSAGE = "message";

        public Schedule()
            : base(MediaType)
        {

        }

        [DataMember(Name = WHEN)]
        public DateTimeOffset When { get; set; }

        [DataMember(Name = MESSAGE)]
        public Message Message { get; set; }
    }
}