using Lime.Protocol;
using Lime.Protocol.Client;
using NSubstitute;
using NUnit.Framework;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client.Lime;
using Takenet.MessagingHub.Client.Receivers;

namespace Takenet.MessagingHub.Client.Test
{
    [TestFixture]
    class MessagingHubClientTests_AddNotificationReceiver : MessagingHubClientTestBase
    {
        public Notification SomeNotification => new Notification { Event = Event.Accepted };

        INotificationReceiver _notificationReceiver;
        SemaphoreSlim _semaphore;

        [SetUp]
        protected override void Setup()
        {
            base.Setup();
            _notificationReceiver = Substitute.For<INotificationReceiver>();
        }

        [Test]
        [Ignore]
        public void WhenClientAddAMessageReceiverAndReceiveAMessageShouldBeHandledByReceiver()
        {
            //Arrange
            _messagingHubClient.UsingAccount("login", "pass");
            _messagingHubClient.AddNotificationReceiver(_notificationReceiver);

            _semaphore = new SemaphoreSlim(1);

            _clientChannel.ReceiveNotificationAsync(Arg.Any<CancellationToken>()).ReturnsForAnyArgs(async (callInfo) =>
            {
                await _semaphore.WaitAsync();
                return SomeNotification;
            });

            //Act
            _messagingHubClient.StartAsync().Wait();

            Task.Delay(3000).Wait();

            //Assert
            _notificationReceiver.ReceivedWithAnyArgs().ReceiveAsync(null);

            _semaphore.DisposeIfDisposable();
        }

        [Test]
        [Ignore]
        public void WhenClientAddAMessageReceiverBaseAndReceiveAMessageTheReceiverShouldHandleAndBeSet()
        {
            //Arrange

            var messageReceiver = Substitute.For<MessageReceiverBase>();

            _messagingHubClient.UsingAccount("login", "pass");
            _messagingHubClient.AddMessageReceiver(messageReceiver);

            _semaphore = new SemaphoreSlim(1);

            _clientChannel.ReceiveNotificationAsync(Arg.Any<CancellationToken>()).ReturnsForAnyArgs(async (_) =>
            {
                await _semaphore.WaitAsync().ConfigureAwait(false);
                return SomeNotification;
            });

            //Act
            _messagingHubClient.StartAsync().Wait();

            Task.Delay(3000).Wait();

            //Assert
            messageReceiver.ReceivedWithAnyArgs().ReceiveAsync(null);
            messageReceiver.MessageSender.ShouldNotBeNull();
            messageReceiver.NotificationSender.ShouldNotBeNull();

            _semaphore.DisposeIfDisposable();
        }
    }
}
