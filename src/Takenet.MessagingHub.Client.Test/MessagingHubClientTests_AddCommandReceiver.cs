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
    class MessagingHubClientTests_AddCommandReceiver
    {
        public Command SomeCommand => new Command { Resource = new PlainDocument(MediaTypes.PlainText) };

        private MessagingHubClient _messagingHubClient;
        private IClientChannel _clientChannel;
        private ISessionFactory _sessionFactory;
        private ICommandReceiver _commandReceiver;
        private TaskCompletionSource<Message> _tcsMessage;
        private SemaphoreSlim _semaphore;

        [SetUp]
        public void Setup()
        {
            _commandReceiver = Substitute.For<ICommandReceiver>();

            _clientChannel = Substitute.For<IClientChannel>();
            var presenceCommand = new Command();
            _clientChannel.WhenForAnyArgs(c => c.SendCommandAsync(null)).Do(c =>
                presenceCommand = c.Arg<Command>());
            _clientChannel.ReceiveCommandAsync(Arg.Any<CancellationToken>()).Returns(
                async (c) => {
                    await _semaphore.WaitAsync().ConfigureAwait(false);
                    return 
                        new Command {
                            Id = presenceCommand.Id, Status = 
                            CommandStatus.Success, Resource = 
                            new PlainDocument(MediaTypes.PlainText)
                        };
                });

            var clientChannelFactory = Substitute.For<IClientChannelFactory>();
            clientChannelFactory.CreateClientChannelAsync(null).ReturnsForAnyArgs(_clientChannel);

            var session = new Session { State = SessionState.Established };
            _sessionFactory = Substitute.For<ISessionFactory>();
            _sessionFactory.CreateSessionAsync(null, null, null).ReturnsForAnyArgs(session);

            _messagingHubClient = new MessagingHubClient(clientChannelFactory, _sessionFactory, "msging.net");
        }

        [Test]
        [Ignore]
        public async Task WhenClientAddACommandReceiverAndReceiveACommandShouldBeHandledByReceiver()
        {
            //Arrange
            _messagingHubClient.UsingAccount("login", "pass");
            //_messagingHubClient.AddCommandReceiver(_commandReceiver);

            _semaphore = new SemaphoreSlim(2);

            //Act
            await _messagingHubClient.StartAsync();
            
            await Task.Delay(3000);

            //Assert
            await _commandReceiver.ReceivedWithAnyArgs().ReceiveAsync(null);

            _semaphore.DisposeIfDisposable();
        }

        [Test]
        [Ignore]
        public void WhenClientAddACommandReceiverBaseAndReceiveACommandTheReceiverShouldHandleAndBeSet()
        {
            //Arrange

            var commandReceiver = Substitute.For<CommandReceiverBase>();

            _messagingHubClient.UsingAccount("login", "pass");
            //_messagingHubClient.AddCommandReceiver(commandReceiver);

            _semaphore = new SemaphoreSlim(2);

            //Act
            _messagingHubClient.StartAsync().ConfigureAwait(false);

            Task.Delay(3000).Wait();

            //Assert
            commandReceiver.ReceivedWithAnyArgs().ReceiveAsync(null);
            commandReceiver.MessageSender.ShouldNotBeNull();
            commandReceiver.NotificationSender.ShouldNotBeNull();

            _semaphore.DisposeIfDisposable();
        }
    }
}
