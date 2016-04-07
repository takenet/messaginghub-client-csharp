using System;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Takenet.MessagingHub.Client.Deprecated.Receivers
{
    /// <summary>
    /// Base message receiver
    /// </summary>
    [Obsolete]
    public abstract class MessageReceiverBase : EnvelopeReceiverBase, IMessageReceiver
    {
        public abstract Task ReceiveAsync(Message message);
    }
}
