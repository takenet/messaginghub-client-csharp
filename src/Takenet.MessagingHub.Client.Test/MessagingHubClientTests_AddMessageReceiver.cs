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
    class MessagingHubClientTests_AddMessageReceiver
    {
        public Message SomeMessage => new Message { Content = new PlainDocument(MediaTypes.PlainText) };

        private MessagingHubClient _messagingHubClient;
        private IClientChannel _clientChannel;
        private ISessionFactory _sessionFactory;
        private IMessageReceiver _messageReceiver;
        private IEnvelopeProcessorFactory<Command> _envelopeProcessorFactory;
        private IEnvelopeProcessor<Command> _commandProcessor;
        private SemaphoreSlim _semaphore;

        [SetUp]
        public void Setup()
        {
            _messageReceiver = Substitute.For<IMessageReceiver>();

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

            _commandProcessor = Substitute.For<IEnvelopeProcessor<Command>>();
            _envelopeProcessorFactory = Substitute.For<IEnvelopeProcessorFactory<Command>>();
            _envelopeProcessorFactory.Create(null).ReturnsForAnyArgs(_commandProcessor);

            _messagingHubClient = new MessagingHubClient(clientChannelFactory, _sessionFactory, _envelopeProcessorFactory, "msging.net");
        }

        [Test]
        public void WhenClientAddAMessageReceiverAndReceiveAMessageShouldBeHandledByReceiver()
        {
            //Arrange
            _messagingHubClient.UsingAccount("login", "pass");
            _messagingHubClient.AddMessageReceiver(_messageReceiver);

            _semaphore = new SemaphoreSlim(1);

            _clientChannel.ReceiveMessageAsync(new CancellationTokenSource().Token).ReturnsForAnyArgs(async (callInfo) =>
            {
                await _semaphore.WaitAsync();
                return SomeMessage;
            });

            //Act
            _messagingHubClient.StartAsync().Wait();

            Task.Delay(3000).Wait();

            //Assert
            _messageReceiver.ReceivedWithAnyArgs().ReceiveAsync(null);

            _semaphore.DisposeIfDisposable();
        }

        [Test]
        public void WhenClientAddAMessageReceiverAndReceiveAMessageShouldBeHandledByReceiverWhenStopped()
        {
            //Arrange
            _messagingHubClient.UsingAccount("login", "pass");
            _messagingHubClient.AddMessageReceiver(_messageReceiver);

            _semaphore = new SemaphoreSlim(1);

            _clientChannel.ReceiveMessageAsync(Arg.Any<CancellationToken>()).ReturnsForAnyArgs(async (callInfo) =>
            {
                await _semaphore.WaitAsync(callInfo.Arg<CancellationToken>());
                return new Message { Content = new PlainDocument(MediaTypes.PlainText) };
            });

            //Act
            _messagingHubClient.StartAsync().Wait();
            
            _messagingHubClient.StopAsync().Wait();

            Task.Delay(3000).Wait();

            //Assert
            _messageReceiver.ReceivedWithAnyArgs().ReceiveAsync(null);

            _semaphore.DisposeIfDisposable();
        }

        [Test]
        public void WhenClientAddAMessageReceiverBaseAndReceiveAMessageTheReceiverShouldHandleAndBeSet()
        {
            //Arrange

            var messageReceiver = Substitute.For<MessageReceiverBase>();

            _messagingHubClient.UsingAccount("login", "pass");
            _messagingHubClient.AddMessageReceiver(messageReceiver);

            _semaphore = new SemaphoreSlim(1);

            _clientChannel.ReceiveMessageAsync(new CancellationTokenSource().Token).ReturnsForAnyArgs(async (_) =>
            {
                await _semaphore.WaitAsync().ConfigureAwait(false);
                return SomeMessage;
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
