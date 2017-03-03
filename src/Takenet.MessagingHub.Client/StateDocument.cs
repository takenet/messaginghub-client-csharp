using Lime.Protocol;
using System;
using System.Runtime.Serialization;

namespace Takenet.MessagingHub.Client
{
    [DataContract]
    public class StateDocument : Document
    {
        public static readonly MediaType MediaType = MediaType.Parse("application/vnd.takenet.state+json");

        public StateDocument() : base(MediaType)
        {
        }

        [DataMember(Name = "state")]
        public string State { get; set; }
    }
}
