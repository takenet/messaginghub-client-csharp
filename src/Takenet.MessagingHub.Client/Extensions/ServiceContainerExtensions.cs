using Takenet.Iris.Messaging;
using Takenet.MessagingHub.Client.Extensions.AttendanceForwarding;
using Takenet.MessagingHub.Client.Extensions.Broadcast;
using Takenet.MessagingHub.Client.Extensions.Bucket;
using Takenet.MessagingHub.Client.Extensions.Contacts;
using Takenet.MessagingHub.Client.Extensions.Delegation;
using Takenet.MessagingHub.Client.Extensions.Directory;
using Takenet.MessagingHub.Client.Extensions.EventTracker;
using Takenet.MessagingHub.Client.Extensions.Scheduler;
using Takenet.MessagingHub.Client.Extensions.Session;
using Takenet.MessagingHub.Client.Extensions.Threads;
using Takenet.MessagingHub.Client.Host;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client.Extensions
{
    public static class ServiceContainerExtensions
    {
        internal static IServiceContainer RegisterExtensions(this IServiceContainer serviceContainer)
        {
            TypeRegistration.RegisterAllDocuments();

            var sender = serviceContainer.GetService<IMessagingHubSender>();
            serviceContainer.RegisterService(typeof(IBroadcastExtension), new BroadcastExtension(sender));
            serviceContainer.RegisterService(typeof(IDelegationExtension), new DelegationExtension(sender));
            serviceContainer.RegisterService(typeof(IDirectoryExtension), new DirectoryExtension(sender));
            serviceContainer.RegisterService(typeof(IContactExtension), new ContactExtension(sender));
            var bucketExtension = new BucketExtension(sender);
            serviceContainer.RegisterService(typeof(ISchedulerExtension), new SchedulerExtension(sender));
            serviceContainer.RegisterService(typeof(IEventTrackExtension), new EventTrackExtension(sender));
            serviceContainer.RegisterService(typeof(IBucketExtension), bucketExtension);
            serviceContainer.RegisterService(typeof(ISessionManager), new SessionManager(bucketExtension));
            serviceContainer.RegisterService(typeof(IAttendanceExtension), new AttendanceExtension(sender));
            serviceContainer.RegisterService(typeof(IThreadExtension), new ThreadExtension(sender));

            return serviceContainer;
        }
    }
}
