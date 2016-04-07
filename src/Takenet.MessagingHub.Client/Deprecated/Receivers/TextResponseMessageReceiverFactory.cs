using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client.Host;

namespace Takenet.MessagingHub.Client.Deprecated.Receivers
{
    [Obsolete]
    public class TextResponseMessageReceiverFactory : IFactory<IMessageReceiver>
    {
        public Task<IMessageReceiver> CreateAsync(IServiceProvider serviceProvider, IDictionary<string, object> settings)
        {
            if (!settings.ContainsKey("text")) throw new ArgumentException("The key 'text' was not found");
            return Task.FromResult<IMessageReceiver>(new TextResponseMessageReceiver((string) settings["text"]));
        }
    }
}