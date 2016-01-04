using Lime.Protocol;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client
{
    /// <summary>
    /// Allow a client application to connect, send an receive messages, commands and notifications in the Messaging Hub.
    /// <a src="http://messaginghub.io" />
    /// </summary>
    public interface IMessagingHubClient : IMessagingHubSender, IEnvelopeReceiver
    {
        
    }
}
