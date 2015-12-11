using Lime.Messaging.Resources;
using Lime.Protocol;
using Lime.Protocol.Client;
using Lime.Protocol.Network;
using NSubstitute;
using NUnit.Framework;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client.Test
{
    [TestFixture]
    public class MessagingHubClientTests_Start
    {
        private MessagingHubClient _messagingHubClient;
        private IClientChannel _clientChannel;
        private ISessionFactory _sessionFactory;

        [SetUp]
        public void Setup()
        {
            _clientChannel = Substitute.For<IClientChannel>();

            var presenceCommand = new Command();
            _clientChannel.WhenForAnyArgs(c => c.SendCommandAsync(null)).Do(c =>
                presenceCommand = c.Arg<Command>());
            _clientChannel.ReceiveCommandAsync(Arg.Any<CancellationToken>()).Returns(c => new Command { Id = presenceCommand.Id, Status = CommandStatus.Success });

            var session = new Session { State = SessionState.Established };

            var clientChannelFactory = Substitute.For<IClientChannelFactory>();
            clientChannelFactory.CreateClientChannelAsync(null).ReturnsForAnyArgs(_clientChannel);

            _sessionFactory = Substitute.For<ISessionFactory>();
            _sessionFactory.CreateSessionAsync(null, null, null).ReturnsForAnyArgs(session);

            _messagingHubClient = new MessagingHubClient(clientChannelFactory, _sessionFactory, "msging.net");
        }


        [Test]
        public async Task WhenClientStartUsingAccountShouldConnectToServer()
        {
            // Arrange
            _messagingHubClient.UsingAccount("login", "pass");
            _sessionFactory.WhenForAnyArgs(s => s.CreateSessionAsync(null, null, null)).Do(s => _clientChannel.State.Returns(SessionState.Established));

            // Act
            await _messagingHubClient.StartAsync();

            // Assert
            _clientChannel.State.ShouldBe(SessionState.Established);
        }

        [Test]
        public async Task WhenClientStartUsingAccessKeyShouldConnectToServer()
        {
            // Arrange
            _messagingHubClient.UsingAccessKey("login", "key");
            _sessionFactory.WhenForAnyArgs(s => s.CreateSessionAsync(null, null, null)).Do(s => _clientChannel.State.Returns(SessionState.Established));

            // Act
            await _messagingHubClient.StartAsync();

            // Assert
            _clientChannel.State.ShouldBe(SessionState.Established);
        }

        [Test]
        public void WhenClientStartWithoutCredentialsShouldThrowException()
        {
            // Arrange
            var session = new Session
            {
                State = SessionState.Failed,
                Reason = new Reason { Code = 1, Description = "failure message" }
            };

            // Arrange
            _sessionFactory.CreateSessionAsync(null, null, null).ReturnsForAnyArgs(session);

            // Act /  Assert
            Should.ThrowAsync<InvalidOperationException>(async () => await _messagingHubClient.StartAsync()).Wait();
        }


        [Test]
        public void WhenClientStartAndServerDoNotAcceptTheSessionShouldThrowException()
        {
            var session = new Session
            {
                State = SessionState.Failed,
                Reason = new Reason { Code = 1, Description = "failure message" }
            };

            // Arrange
            _sessionFactory.CreateSessionAsync(null, null, null).ReturnsForAnyArgs(session);


            _messagingHubClient.UsingAccount("login", "pass");

            // Act
            var exception = Should.ThrowAsync<LimeException>(async () => await _messagingHubClient.StartAsync()).Result;

            // Assert
            exception.Reason.Description.ShouldBe(session.Reason.Description);
            exception.Reason.Code.ShouldBe(session.Reason.Code);
        }

    }
}
