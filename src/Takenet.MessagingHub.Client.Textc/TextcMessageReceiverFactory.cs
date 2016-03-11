using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Takenet.MessagingHub.Client.Host;
using Takenet.MessagingHub.Client.Receivers;
using Takenet.Textc.Csdl;
using Takenet.Textc.Processors;
using TypeUtil = Lime.Protocol.Serialization.TypeUtil;

namespace Takenet.MessagingHub.Client.Textc
{
    public class TextcMessageReceiverFactory : IFactory<IMessageReceiver>
    {
        public Task<IMessageReceiver> CreateAsync(IServiceProvider serviceProvider, IDictionary<string, object> settings)
        {
            var builder = new TextcMessageReceiverBuilder(serviceProvider.GetService<MessagingHubSenderBuilder>());
            if (settings != null)
            {
                foreach (var setting in settings)
                {
                    var syntax = CsdlParser.Parse(setting.Key);
                    builder = builder
                        .ForSyntax(syntax)
                        .ProcessWith(o =>
                        {
                            var dictionary = setting.Value as IDictionary<string, object>;
                            if (dictionary == null && setting.Value is JObject)
                            {
                                dictionary = ((JObject)setting.Value).ToObject<Dictionary<string, object>>();
                            }
                            if (dictionary?["processor"] == null || dictionary["method"] == null)
                            {
                                throw new ArgumentException("The syntax values must be a dictionary with the 'processor' and 'method' keys");
                            }
                            var processorTypeName = (string)dictionary["processor"];
                            var methodName = (string)dictionary["method"];
                            var processorType =
                                TypeUtil.GetAllLoadedTypes()
                                    .FirstOrDefault(t => t.Name.Equals(processorTypeName, StringComparison.OrdinalIgnoreCase)) ??
                                Type.GetType(processorTypeName, true, true);

                            var processor = serviceProvider.GetService(processorType) ?? Activator.CreateInstance(processorType);
                            var method = processor.GetType().GetMethod(methodName);
                            if (method == null || method.ReturnType != typeof(Task))
                            {
                                return new ReflectionCommandProcessor(processor, methodName, true, o, syntax);
                            }

                            return new ReflectionCommandProcessor(processor, methodName, true, syntaxes: syntax);
                        });
                }
            }

            return Task.FromResult<IMessageReceiver>(builder.Build());
        }
    }
}
