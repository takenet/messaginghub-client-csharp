using System;
using Takenet.MessagingHub.Client.Extensions.AttendanceForwarding;
using Takenet.MessagingHub.Client.Extensions.Broadcast;
using Takenet.MessagingHub.Client.Extensions.Bucket;
using Takenet.MessagingHub.Client.Extensions.Contacts;
using Takenet.MessagingHub.Client.Extensions.Delegation;
using Takenet.MessagingHub.Client.Extensions.Directory;
using Takenet.MessagingHub.Client.Extensions.EventTracker;
using Takenet.MessagingHub.Client.Extensions.Profile;
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
            Lime.Messaging.Registrator.RegisterDocuments();
            Iris.Messaging.Registrator.RegisterDocuments();
            Registrator.RegisterDocuments();

            Func<IMessagingHubSender> senderFactory = () => serviceContainer.GetService<IMessagingHubSender>();
            serviceContainer.RegisterService(typeof(IBroadcastExtension), () => new BroadcastExtension(senderFactory()));
            serviceContainer.RegisterService(typeof(IDelegationExtension), () => new DelegationExtension(senderFactory()));
            serviceContainer.RegisterService(typeof(IDirectoryExtension), () => new DirectoryExtension(senderFactory()));
            serviceContainer.RegisterService(typeof(IContactExtension), () => new ContactExtension(senderFactory()));
            Func<IBucketExtension> bucketExtensionFactory = () => new BucketExtension(senderFactory());
            serviceContainer.RegisterService(typeof(ISchedulerExtension), () => new SchedulerExtension(senderFactory()));
            serviceContainer.RegisterService(typeof(IEventTrackExtension), () => new EventTrackExtension(senderFactory()));
            serviceContainer.RegisterService(typeof(IProfileExtension), () => new ProfileExtension(senderFactory()));
            serviceContainer.RegisterService(typeof(IBucketExtension), bucketExtensionFactory);
            serviceContainer.RegisterService(typeof(ISessionManager), () => new SessionManager(bucketExtensionFactory()));
            serviceContainer.RegisterService(typeof(IAttendanceExtension), () => new AttendanceExtension(senderFactory()));
            serviceContainer.RegisterService(typeof(IThreadExtension), () => new ThreadExtension(senderFactory()));

            return serviceContainer;
        }
    }
}
