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
    public class MessagingHubClientTests_Close
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

            _clientChannel.WhenForAnyArgs(c => c.SendFinishingSessionAsync()).Do(c => _clientChannel.State.Returns(SessionState.Finished));

            var session = new Session { State = SessionState.Established };

            var clientChannelFactory = Substitute.For<IClientChannelFactory>();
            clientChannelFactory.CreateClientChannelAsync(null).ReturnsForAnyArgs(_clientChannel);

            _sessionFactory = Substitute.For<ISessionFactory>();
            _sessionFactory.CreateSessionAsync(null, null, null).ReturnsForAnyArgs(session);

            _messagingHubClient = new MessagingHubClient(clientChannelFactory, _sessionFactory, "msging.net");
        }

        [Test]
        public async Task WhenClientIsConnectedAndCloseConnectionShouldDisconnectFromServer()
        {
            //Arrange
            _clientChannel.State.Returns(SessionState.Established);
            _messagingHubClient.UsingAccessKey("login", "key");
            await _messagingHubClient.StartAsync(); 

            // Act
            var x = _messagingHubClient.StopAsync();

            // Assert
            _clientChannel.State.ShouldBe(SessionState.Finished);
        }

        [Test]
        public void WhenClientIsNotConnectedAndCloseConnectionShouldThrowException()
        {
            //Arrange
            _messagingHubClient.UsingAccessKey("login", "key");
            
            // Act // Asert
            Should.ThrowAsync<InvalidOperationException>(async () => await _messagingHubClient.StopAsync()).Wait();
        }

        [Test]
        public async Task WhenClientHasntEstablishedSessionAndCloseConnectionShouldDisconnectFromServer()
        {
            //Arrange
            _clientChannel.State.Returns(SessionState.Failed);

            var transport = Substitute.For<ITransport>();
            _clientChannel.Transport.Returns(transport);

            _messagingHubClient.UsingAccessKey("login", "key");
            await _messagingHubClient.StartAsync();

            // Act
            var x = _messagingHubClient.StopAsync();

            // Assert
            _clientChannel.State.ShouldBe(SessionState.Failed);
            await transport.CloseAsync(CancellationToken.None);
        }
    }
}
