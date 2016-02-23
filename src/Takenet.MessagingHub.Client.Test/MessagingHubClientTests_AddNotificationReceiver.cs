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

        [SetUp]
        protected override void Setup()
        {
            base.Setup();
            _notificationReceiver = Substitute.For<INotificationReceiver>();
        }

        [TearDown]
        protected override void TearDown()
        {
            base.TearDown();
        }

        [Test]
        public async Task Add_NotificationReceiver_And_Process_Notification_With_Success()
        {
            //Arrange
            EnvelopeListenerRegistrar.AddNotificationReceiver(_notificationReceiver);
            await MessagingHubClient.StartAsync();

            //Act
            DispatchNotification();
            await Task.Delay(TIME_OUT);

            //Assert
            _notificationReceiver.ReceivedWithAnyArgs().ReceiveAsync(null);
        }

        [Test]
        public async Task Add_Specific_NotificationReceiver_And_Process_Notification_With_Success()
        {
            //Arrange
            EnvelopeListenerRegistrar.AddNotificationReceiver(_notificationReceiver, Event.Accepted);
            SomeNotification.Event = Event.Accepted;
            await MessagingHubClient.StartAsync();

            //Act
            DispatchNotification();
            await Task.Delay(TIME_OUT);

            //Assert
            _notificationReceiver.ReceivedWithAnyArgs().ReceiveAsync(null);
        }


        [Test]
        public async Task Add_Base_NotificationReceiver_And_Process_Notification_With_Success()
        {
            //Arrange
            var notificationReceiver = Substitute.For<NotificationReceiverBase>();
            EnvelopeListenerRegistrar.AddNotificationReceiver(notificationReceiver);
            await MessagingHubClient.StartAsync();

            //Act
            DispatchNotification();
            await Task.Delay(TIME_OUT);

            //Assert
            notificationReceiver.ReceivedWithAnyArgs().ReceiveAsync(null);
            notificationReceiver.EnvelopeSender.ShouldNotBeNull();
        }

        private void DispatchNotification()
        {
            NotificationProducer.Add(SomeNotification);
        }

    }
}
