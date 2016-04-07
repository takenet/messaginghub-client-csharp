using System;

namespace Takenet.MessagingHub.Client.Deprecated
{
    /// <summary>
    /// Allow a client application to connect, send an receive messages, commands and notifications in the Messaging Hub.
    /// <a src="http://messaginghub.io" />
    /// </summary>
    [Obsolete]
    public interface IMessagingHubClient : IMessagingHubSender, IEnvelopeReceiver
    {
        
    }
}
