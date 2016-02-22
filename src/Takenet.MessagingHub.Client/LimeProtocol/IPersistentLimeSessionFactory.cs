using Lime.Protocol;
using Lime.Protocol.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client.LimeProtocol
{
    internal interface IPersistentLimeSessionFactory
    {
        Task<IPersistentLimeSession> CreatePersistentClientChannelAsync(Uri endpoint, TimeSpan sendTimeout, Identity identity, Authentication authentication, IClientChannelFactory clientChannelFactory, ILimeSessionProvider limeSessionProvider);
    }
}
