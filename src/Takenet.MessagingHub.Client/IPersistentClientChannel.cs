using Lime.Protocol;
using Lime.Protocol.Client;
using Lime.Protocol.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client
{
    internal interface IPersistentClientChannel
    {
        Task StartAsync();

        Task StopAsync();
        
        Task<Message> ReceiveMessageAsync(CancellationToken cancellationToken);
        Task SendMessageAsync(Message message);

        Task<Command> ReceiveCommandAsync(CancellationToken cancellationToken);
        Task SendCommandAsync(Command command);

        Task<Notification> ReceiveNotificationAsync(CancellationToken cancellationToken);
        Task SendNotificationAsync(Notification notification);

        Task SetResourceAsync<TResource>(LimeUri uri, TResource resource, CancellationToken cancellationToken, Func<Command, Task> unrelatedCommandHandler = null) where TResource : Document;
    }
}
