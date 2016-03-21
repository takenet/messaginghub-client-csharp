using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Takenet.MessagingHub.Client.Host;
using Takenet.MessagingHub.Client.Receivers;
using Takenet.Textc.Csdl;
using Takenet.Textc.Processors;
using Takenet.Textc.Scorers;

namespace Takenet.MessagingHub.Client.Textc
{
    public class TextcMessageReceiverFactory : IFactory<IMessageReceiver>
    {
        public static IDictionary<Type, object> ProcessorInstancesDictionary = new Dictionary<Type, object>();

        public async Task<IMessageReceiver> CreateAsync(IServiceProvider serviceProvider, IDictionary<string, object> settings)
        {
            var builder = new TextcMessageReceiverBuilder(serviceProvider.GetService<MessagingHubSenderBuilder>());
            if (settings != null)
            {
                var textcMessageReceiverSettings = TextcMessageReceiverSettings.ParseFromSettings(settings);
                if (textcMessageReceiverSettings.Commands != null)
                {
                    builder = SetupCommands(serviceProvider, settings, textcMessageReceiverSettings.Commands, builder);
                }

                if (textcMessageReceiverSettings.ScorerType != null)
                {
                    builder = await SetupScorerAsync(serviceProvider, settings, textcMessageReceiverSettings.ScorerType, builder).ConfigureAwait(false);
                }
            }

            return builder.Build();
        }

        private static TextcMessageReceiverBuilder SetupCommands(IServiceProvider serviceProvider, IDictionary<string, object> settings, TextcMessageReceiverCommandSettings[] commandSettings, TextcMessageReceiverBuilder builder)
        {
            foreach (var commandSetting in commandSettings)
            {
                var syntaxes = commandSetting.Syntaxes.Select(CsdlParser.Parse).ToArray();
                if (syntaxes.Length > 0)
                {
                    builder = builder
                        .ForSyntaxes(syntaxes)
                        .ProcessWith(o =>
                        {
                            var processorTypeName = commandSetting.ProcessorType;
                            var methodName = commandSetting.Method;
                            var processorType = Bootstrapper.ParseTypeName(processorTypeName);
                            object processor;
                            if (!ProcessorInstancesDictionary.TryGetValue(processorType, out processor))
                            {
                                processor =
                                    Bootstrapper.CreateAsync<object>(processorType, serviceProvider, settings).Result;
                                ProcessorInstancesDictionary.Add(processorType, processor);
                            }

                            var method = processorType.GetMethod(methodName);
                            if (method == null || method.ReturnType != typeof (Task))
                            {
                                return new ReflectionCommandProcessor(processor, methodName, true, o, syntaxes);
                            }

                            return new ReflectionCommandProcessor(processor, methodName, true, syntaxes: syntaxes);
                        });
                }
            }
            return builder;
        }

        private static async Task<TextcMessageReceiverBuilder> SetupScorerAsync(IServiceProvider serviceProvider, IDictionary<string, object> settings,
            string scorerType, TextcMessageReceiverBuilder builder)
        {
            IExpressionScorer scorer;
            if (scorerType.Equals(nameof(MatchCountExpressionScorer)))
            {
                scorer = new MatchCountExpressionScorer();
            }
            else if (scorerType.Equals(nameof(RatioExpressionScorer)))
            {
                scorer = new RatioExpressionScorer();
            }
            else
            {
                scorer = await Bootstrapper.CreateAsync<IExpressionScorer>(scorerType, serviceProvider, settings).ConfigureAwait(false);
            }
            builder = builder.WithExpressionScorer(scorer);
            return builder;
        }
    }

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

    public class TextcMessageReceiverCommandSettings
    {
        public string[] Syntaxes { get; set; }

        public string ProcessorType { get; set; }

        public string Method { get; set; }
    }
}
