using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Takenet.MessagingHub.Client.Host;
using Takenet.MessagingHub.Client.Receivers;
using Takenet.Textc.Csdl;
using Takenet.Textc.Processors;
using Takenet.Textc.Scorers;
using TypeUtil = Lime.Protocol.Serialization.TypeUtil;

namespace Takenet.MessagingHub.Client.Textc
{
    public class TextcMessageReceiverFactory : IFactory<IMessageReceiver>
    {
        public async Task<IMessageReceiver> CreateAsync(IServiceProvider serviceProvider, IDictionary<string, object> settings)
        {
            var builder = new TextcMessageReceiverBuilder(serviceProvider.GetService<MessagingHubSenderBuilder>());
            if (settings != null)
            {
                if (settings.ContainsKey("syntaxes"))
                {
                    builder = SetupSyntaxes(serviceProvider, settings, builder);
                }                

                if (settings.ContainsKey("scorer"))
                {
                    builder = await SetupScorerAsync(serviceProvider, settings, builder).ConfigureAwait(false);
                }
            }

            return builder.Build();
        }

        private static TextcMessageReceiverBuilder SetupSyntaxes(IServiceProvider serviceProvider, IDictionary<string, object> settings, 
            TextcMessageReceiverBuilder builder)
        {
            var syntaxes = GetArrayFromJson<IDictionary<string, object>>(settings["syntaxes"]);

            foreach (var syntaxDictionary in syntaxes)
            {
                var syntaxSetting = syntaxDictionary.First();

                var syntax = CsdlParser.Parse(syntaxSetting.Key);
                builder = builder
                    .ForSyntax(syntax)
                    .ProcessWith(o =>
                    {
                        var dictionary = GetDictionaryFromJson(syntaxSetting.Value);
                        if (dictionary?["processor"] == null || dictionary["method"] == null)
                        {
                            throw new ArgumentException(
                                "The syntax values must be a dictionary with the 'processor' and 'method' keys");
                        }
                        var processorTypeName = (string) dictionary["processor"];
                        var methodName = (string) dictionary["method"];                        
                        var processor = Bootstrapper.CreateAsync<object>(processorTypeName, serviceProvider, settings).Result;                        
                        var method = processor.GetType().GetMethod(methodName);
                        if (method == null || method.ReturnType != typeof (Task))
                        {
                            return new ReflectionCommandProcessor(processor, methodName, true, o, syntax);
                        }

                        return new ReflectionCommandProcessor(processor, methodName, true, syntaxes: syntax);
                    });
            }
            return builder;
        }

        private static async Task<TextcMessageReceiverBuilder> SetupScorerAsync(IServiceProvider serviceProvider, IDictionary<string, object> settings,
            TextcMessageReceiverBuilder builder)
        {
            var scorerTypeName = (string) settings["scorer"];

            IExpressionScorer scorer;
            if (scorerTypeName.Equals(nameof(MatchCountExpressionScorer)))
            {
                scorer = new MatchCountExpressionScorer();
            }
            else if (scorerTypeName.Equals(nameof(RatioExpressionScorer)))
            {
                scorer = new RatioExpressionScorer();
            }
            else
            {
                scorer = await Bootstrapper.CreateAsync<IExpressionScorer>(scorerTypeName, serviceProvider, settings).ConfigureAwait(false);
            }
            builder = builder.WithExpressionScorer(scorer);
            return builder;
        }

        private static T[] GetArrayFromJson<T>(object value)
        {
            var jArray = value as JArray;
            if (jArray != null) return jArray.ToObject<T[]>();
            return value as T[];
        }

        private static IDictionary<string, object> GetDictionaryFromJson(object value)
        {
            var jObject = value as JObject;
            if (jObject != null) return jObject.ToObject<Dictionary<string, object>>();
            return value as IDictionary<string, object>;

        }
    }
}
