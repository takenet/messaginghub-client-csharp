using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Takenet.MessagingHub.Client.Host;

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

        public string TextSplitterType { get; set; }

        public TextcMessageReceiverContextCommandSettings Context { get; set; }

        public string MatchNotFoundReturnText { get; set; }

        public DocumentDefinition MatchNotFoundReturn { get; set; }

        public string ExceptionHandlerType { get; set; }

        public string OutState { get; set; }

        public string[] PreProcessorTypes { get; set; }

        public static TextcMessageReceiverSettings ParseFromSettings(IDictionary<string, object> settings)
        {
            var jObject = JObject.FromObject(settings);
            return jObject.ToObject<TextcMessageReceiverSettings>(_serializer);
        }
    }
}