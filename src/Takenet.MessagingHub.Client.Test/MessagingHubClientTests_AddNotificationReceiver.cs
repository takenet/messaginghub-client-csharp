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
using Takenet.MessagingHub.Client.Receivers;

namespace Takenet.MessagingHub.Client.Test
{
    [TestFixture]
    class MessagingHubClientTests_AddNotificationReceiver
    {
        public Notification SomeNotification => new Notification { Event = Event.Accepted };

        private MessagingHubClient _messagingHubClient;
        private IClientChannel _clientChannel;
        private ISessionFactory _sessionFactory;
        private INotificationReceiver _notificationReceiver;
        private TaskCompletionSource<Message> _tcsMessage;
        private SemaphoreSlim _semaphore;

        [SetUp]
        public void Setup()
        {
            _notificationReceiver = Substitute.For<INotificationReceiver>();

            _clientChannel = Substitute.For<IClientChannel>();
            var presenceCommand = new Command();
            _clientChannel.WhenForAnyArgs(c => c.SendCommandAsync(null)).Do(c =>
                presenceCommand = c.Arg<Command>());
            _clientChannel.ReceiveCommandAsync(Arg.Any<CancellationToken>()).Returns(c => new Command { Id = presenceCommand.Id, Status = CommandStatus.Success });

            var clientChannelFactory = Substitute.For<IClientChannelFactory>();
            clientChannelFactory.CreateClientChannelAsync(null).ReturnsForAnyArgs(_clientChannel);

            var session = new Session { State = SessionState.Established };
            _sessionFactory = Substitute.For<ISessionFactory>();
            _sessionFactory.CreateSessionAsync(null, null, null).ReturnsForAnyArgs(session);

            _messagingHubClient = new MessagingHubClient(clientChannelFactory, _sessionFactory, "msging.net");
        }

        [Test]
        [Ignore]
        public async Task WhenClientAddAMessageReceiverAndReceiveAMessageShouldBeHandledByReceiver()
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
            await _messagingHubClient.StartAsync();

            await Task.Delay(3000);

            //Assert
            _notificationReceiver.ReceivedWithAnyArgs().ReceiveAsync(null);

            _semaphore.DisposeIfDisposable();
        }

        [Test]
        [Ignore]
        public async Task WhenClientAddAMessageReceiverBaseAndReceiveAMessageTheReceiverShouldHandleAndBeSet()
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
            await _messagingHubClient.StartAsync().ConfigureAwait(false);

            await Task.Delay(3000);

            //Assert
            messageReceiver.ReceivedWithAnyArgs().ReceiveAsync(null);
            messageReceiver.MessageSender.ShouldNotBeNull();
            messageReceiver.NotificationSender.ShouldNotBeNull();

            _semaphore.DisposeIfDisposable();
        }
    }
}
