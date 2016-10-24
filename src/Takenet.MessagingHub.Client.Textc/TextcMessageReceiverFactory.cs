using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Host;
using Takenet.MessagingHub.Client.Listener;
using Takenet.Textc;
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
            var builder = new TextcMessageReceiverBuilder(serviceProvider.GetService<Sender.IMessagingHubSender>());
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

                if (textcMessageReceiverSettings.Context != null)
                {
                    builder = await SetupContextAsync(serviceProvider, settings, textcMessageReceiverSettings.Context, builder).ConfigureAwait(false);
                }
            }

            return builder.Build();
        }

        private static readonly Regex ReturnVariablesRegex = new Regex("{[a-zA-Z0-9]+}", RegexOptions.Compiled);

        private static TextcMessageReceiverBuilder SetupCommands(IServiceProvider serviceProvider, IDictionary<string, object> settings, TextcMessageReceiverCommandSettings[] commandSettings, TextcMessageReceiverBuilder builder)
        {
            foreach (var commandSetting in commandSettings)
            {
                var syntaxes = commandSetting.Syntaxes.Select(CsdlParser.Parse).ToArray();
                if (syntaxes.Length > 0)
                {
                    var syntaxBuilder = builder.ForSyntaxes(syntaxes);

                    if (!string.IsNullOrEmpty(commandSetting.ReturnText))
                    {
                        var returnVariables = new List<string>();

                        var returnTextVariableMatches = ReturnVariablesRegex.Matches(commandSetting.ReturnText);
                        if (returnTextVariableMatches.Count > 0)
                        {
                            returnVariables.AddRange(returnTextVariableMatches.Cast<Match>().Select(m => m.Value));
                        }

                        builder = syntaxBuilder.Return((IRequestContext context) =>
                        {
                            var returnText = commandSetting.ReturnText;
                            foreach (var returnVariable in returnVariables)
                            {
                                var returnVariableValue = context
                                    .GetVariable(returnVariable.TrimStart('{').TrimEnd('}'))?.ToString() ?? "";
                                returnText = returnText.Replace(returnVariable, returnVariableValue);
                            }

                            return returnText.AsCompletedTask();
                        });
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
                                var assembly = typeof(TextcMessageReceiverBuilder).Assembly;
                                var path = new FileInfo(assembly.Location).DirectoryName;
                                Lime.Protocol.Serialization.TypeUtil.LoadAssembliesAndReferences(path, assemblyFilter: Lime.Protocol.Serialization.TypeUtil.IgnoreSystemAndMicrosoftAssembliesFilter,
                                    ignoreExceptionLoadingReferencedAssembly: true);
                                var processorType = TypeProvider.Instance.GetType(processorTypeName);
                                object processor;
                                if (!ProcessorInstancesDictionary.TryGetValue(processorType, out processor))
                                {
                                    processor =
                                        Bootstrapper.CreateAsync<object>(processorType, serviceProvider, settings)
                                            .Result;
                                    ProcessorInstancesDictionary.Add(processorType, processor);
                                }

                                var method = processorType.GetMethod(methodName);
                                if (method == null || method.ReturnType != typeof(Task))
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
                scorer = await Bootstrapper.CreateAsync<IExpressionScorer>(scorerType, serviceProvider, settings, TypeProvider.Instance).ConfigureAwait(false);
            }
            builder = builder.WithExpressionScorer(scorer);
            return builder;
        }

        private static async Task<TextcMessageReceiverBuilder> SetupContextAsync(IServiceProvider serviceProvider, IDictionary<string, object> settings,
            TextcMessageReceiverContextCommandSettings context, TextcMessageReceiverBuilder builder)
        {
            var contextProvider = await Bootstrapper.CreateAsync<IContextProvider>(context.Type, serviceProvider, settings, TypeProvider.Instance).ConfigureAwait(false);

            builder = builder.WithContextProvider(contextProvider);
            return builder;
        }
    }
}
