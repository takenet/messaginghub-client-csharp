using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Lime.Protocol;

namespace Takenet.MessagingHub.Client
{
    [DataContract]
    public class NavigationSession : Document
    {
        private static readonly MediaType MediaType = MediaType.Parse("application/vnd.takenet.navigation-session+json");

        public NavigationSession()
            : base(MediaType)
        {

        }

        [DataMember(Name = "creation")]
        public DateTimeOffset Creation { get; set; }

        [DataMember(Name = "identity")]
        public Identity Identity { get; set; }

        [DataMember(Name = "states")]
        public string[] States { get; set; }

        [DataMember(Name = "variables")]
        public Dictionary<string, string> Variables { get; set; }
    }
}