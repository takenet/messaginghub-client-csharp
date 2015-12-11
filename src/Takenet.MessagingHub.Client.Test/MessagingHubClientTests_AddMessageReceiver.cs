using Lime.Protocol;
using Lime.Protocol.Client;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client.Test
{
    [TestFixture]
    class MessagingHubClientTests_AddMessageReceiver
    {
        private MessagingHubClient _messagingHubClient;
        private IClientChannel _clientChannel;
        private ISessionFactory _sessionFactory;
        private IMessageReceiver _messageReceiver;
        private TaskCompletionSource<Message> _tcsMessage;
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

            _semaphore = new SemaphoreSlim(1);
            
            _messagingHubClient = new MessagingHubClient(clientChannelFactory, _sessionFactory, "msging.net");
        }



        [Test]
        public async Task WhenClientAddAMessageReceiverAndReceiveAMessageShouldBeHandledByReceiver()
        {
            //Arrange
            _messagingHubClient.UsingAccount("login", "pass");
            _messagingHubClient.AddMessageReceiver(_messageReceiver);

            _clientChannel.ReceiveMessageAsync(CancellationToken.None).ReturnsForAnyArgs(async (callInfo) => {
                await _semaphore.WaitAsync();
                return new Message { Content = new PlainDocument(MediaTypes.PlainText) };
            });

            //Act
            await _messagingHubClient.StartAsync();

            await Task.Delay(3000);

            //Assert
            _messageReceiver.ReceivedWithAnyArgs().ReceiveAsync(null);

            _semaphore.DisposeIfDisposable();
        }
    }
}
