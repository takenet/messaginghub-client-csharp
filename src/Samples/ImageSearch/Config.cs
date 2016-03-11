using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ImageSearch
{
    public class Config
    {
        [JsonProperty("messagingHubLogin")]
        public string MessagingHubLogin { get; set; }

        [JsonProperty("messagingHubApiKey")]
        public string MessagingHubApiKey { get; set; }

        [JsonProperty("bingApiKey")]
        public string BingApiKey { get; set; }
    }
}
