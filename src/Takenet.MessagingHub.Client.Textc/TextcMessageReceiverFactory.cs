using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lime.Protocol;
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
                    var syntaxBuilder = builder
                        .ForSyntaxes(syntaxes);

                    if (!string.IsNullOrEmpty(commandSetting.ReturnText))
                    {
                        builder = syntaxBuilder.Return(() => commandSetting.ReturnText.AsCompletedTask());
                    }
                    else if (commandSetting.ReturnJson != null)
                    {
                        var mediaType = MediaType.Parse(commandSetting.ReturnJsonMediaType ?? "application/json");
                        var document = new JsonDocument(commandSetting.ReturnJson, mediaType);
                        builder = syntaxBuilder.Return(() => document.AsCompletedTask());
                    }
                    else if (!string.IsNullOrEmpty(commandSetting.ProcessorType) && !string.IsNullOrEmpty(commandSetting.Method))
                    {
                        builder = syntaxBuilder
                            .ProcessWith(o =>
                            {
                                var processorTypeName = commandSetting.ProcessorType;
                                var methodName = commandSetting.Method;
                                var processorType = Bootstrapper.ParseTypeName(processorTypeName);
                                object processor;
                                if (!ProcessorInstancesDictionary.TryGetValue(processorType, out processor))
                                {
                                    processor =
                                        Bootstrapper.CreateAsync<object>(processorType, serviceProvider, settings)
                                            .Result;
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
}
