using Lime.Protocol;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client.LimeProtocol
{
    internal interface IPersistentLimeSession
    {
        Task StartAsync();

        Task StopAsync();

        event EventHandler SessionEstabilished;

        Task<Message> ReceiveMessageAsync(CancellationToken cancellationToken);
        Task SendMessageAsync(Message message);

        Task<Command> ReceiveCommandAsync(CancellationToken cancellationToken);
        Task SendCommandAsync(Command command);

        Task<Notification> ReceiveNotificationAsync(CancellationToken cancellationToken);
        Task SendNotificationAsync(Notification notification);

        Task SetResourceAsync<TResource>(LimeUri uri, TResource resource, CancellationToken cancellationToken, Func<Command, Task> unrelatedCommandHandler = null) where TResource : Document;
    }
}
