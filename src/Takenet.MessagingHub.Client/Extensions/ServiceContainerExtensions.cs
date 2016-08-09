using Takenet.MessagingHub.Client.Extensions.Broadcast;
using Takenet.MessagingHub.Client.Extensions.Bucket;
using Takenet.MessagingHub.Client.Extensions.Delegation;
using Takenet.MessagingHub.Client.Extensions.Scheduler;
using Takenet.MessagingHub.Client.Extensions.Session;
using Takenet.MessagingHub.Client.Host;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client.Extensions
{
    public static class ServiceContainerExtensions
    {
        internal static IServiceContainer RegisterExtensions(this IServiceContainer serviceContainer)
        {
            var sender = serviceContainer.GetService<IMessagingHubSender>();
            serviceContainer.RegisterService(typeof(IBroadcastExtension), new BroadcastExtension(sender));
            serviceContainer.RegisterService(typeof(IDelegationExtension), new DelegationExtension(sender));
            var bucketExtension = new BucketExtension(sender);
            serviceContainer.RegisterService(typeof(IBucketExtension), new BucketExtension(sender));
            serviceContainer.RegisterService(typeof(ISchedulerExtension), new SchedulerExtension(sender));
            serviceContainer.RegisterService(typeof(ISessionManager), new SessionManager(bucketExtension));

            return serviceContainer;
        }
    }
}
