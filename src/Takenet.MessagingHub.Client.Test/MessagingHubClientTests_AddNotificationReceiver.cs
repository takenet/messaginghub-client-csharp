using Lime.Protocol;
using NSubstitute;
using NUnit.Framework;
using Shouldly;
using System.Threading;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client.Receivers;

namespace Takenet.MessagingHub.Client.Test
{
    [TestFixture]
    internal class MessagingHubClientTests_AddNotificationReceiver : MessagingHubClientTestBase
    {
        private Notification SomeNotification => new Notification { Event = Event.Accepted };
        private INotificationReceiver _notificationReceiver;
        private SemaphoreSlim _semaphore;

        [SetUp]
        protected override void Setup()
        {
            base.Setup();
            _notificationReceiver = Substitute.For<INotificationReceiver>();
        }

        [Test]
        public void Add_NotificationReceiver_And_Process_Notification_With_Success()
        {
            //Arrange
            MessagingHubClient.UsingAccount("login", "pass");
            MessagingHubClient.AddNotificationReceiver(_notificationReceiver);

            _semaphore = new SemaphoreSlim(1);

            ClientChannel.ReceiveNotificationAsync(Arg.Any<CancellationToken>()).ReturnsForAnyArgs(async (callInfo) =>
            {
                await _semaphore.WaitAsync();
                return SomeNotification;
            });

            //Act
            MessagingHubClient.StartAsync().Wait();

            Task.Delay(3000).Wait();

            //Assert
            _notificationReceiver.ReceivedWithAnyArgs().ReceiveAsync(null);

            _semaphore.DisposeIfDisposable();
        }

        [Test]
        public void Add_Specific_NotificationReceiver_And_Process_Notification_With_Success()
        {
            //Arrange
            MessagingHubClient.UsingAccount("login", "pass");
            MessagingHubClient.AddNotificationReceiver(_notificationReceiver, Event.Accepted);

            _semaphore = new SemaphoreSlim(1);

            SomeNotification.Event = Event.Accepted;

            ClientChannel.ReceiveNotificationAsync(Arg.Any<CancellationToken>()).ReturnsForAnyArgs(async (callInfo) =>
            {
                await _semaphore.WaitAsync();
                return SomeNotification;
            });

            //Act
            MessagingHubClient.StartAsync().Wait();

            Task.Delay(5000).Wait();

            //Assert
            _notificationReceiver.ReceivedWithAnyArgs().ReceiveAsync(null);

            _semaphore.DisposeIfDisposable();
        }


        [Test]
        public void Add_Base_NotificationReceiver_And_Process_Notification_With_Success()
        {
            //Arrange

            var notificationReceiver = Substitute.For<NotificationReceiverBase>();

            MessagingHubClient.UsingAccount("login", "pass");
            MessagingHubClient.AddNotificationReceiver(notificationReceiver);

            _semaphore = new SemaphoreSlim(1);

            ClientChannel.ReceiveNotificationAsync(Arg.Any<CancellationToken>()).ReturnsForAnyArgs(async (_) =>
            {
                await _semaphore.WaitAsync().ConfigureAwait(false);
                return SomeNotification;
            });

            //Act
            MessagingHubClient.StartAsync().Wait();

            Task.Delay(3000).Wait();

            //Assert
            notificationReceiver.ReceivedWithAnyArgs().ReceiveAsync(null);
            notificationReceiver.MessageSender.ShouldNotBeNull();
            notificationReceiver.NotificationSender.ShouldNotBeNull();
            notificationReceiver.CommandSender.ShouldNotBeNull();

            _semaphore.DisposeIfDisposable();
        }
    }
}
