using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Takenet.MessagingHub.Client.Textc
{
    public class TextcMessageReceiverSettings
    {
        private static readonly JsonSerializer _serializer;

        static TextcMessageReceiverSettings()
        {
            _serializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }

        public TextcMessageReceiverCommandSettings[] Commands { get; set; }

        public string ScorerType { get; set; }

        public static TextcMessageReceiverSettings ParseFromSettings(IDictionary<string, object> settings)
        {
            var jObject = JObject.FromObject(settings);
            return jObject.ToObject<TextcMessageReceiverSettings>(_serializer);
        }
    }
}