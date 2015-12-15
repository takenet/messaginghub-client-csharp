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
using Takenet.MessagingHub.Client.Lime;

namespace Takenet.MessagingHub.Client.Test
{
    internal class MessagingHubClientTests_Close : MessagingHubClientTestBase
    {
        [SetUp]
        protected override void Setup()
        {
            base.Setup();
        }

        [Test]
        public void WhenClientIsConnectedAndCloseConnectionShouldDisconnectFromServer()
        {
            //Arrange
            _clientChannel.WhenForAnyArgs(c => c.SendFinishingSessionAsync()).Do(c => _clientChannel.State.Returns(SessionState.Finished));

            _clientChannel.State.Returns(SessionState.Established);
            _messagingHubClient.UsingAccessKey("login", "key");
            _messagingHubClient.StartAsync().Wait(); 

            // Act
            _messagingHubClient.StopAsync().Wait();

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
        public void WhenClientHasntEstablishedSessionAndCloseConnectionShouldDisconnectFromServer()
        {
            //Arrange
            _clientChannel.State.Returns(SessionState.Failed);

            var transport = Substitute.For<ITransport>();
            _clientChannel.Transport.Returns(transport);

            _messagingHubClient.UsingAccessKey("login", "key");
            _messagingHubClient.StartAsync().Wait();

            // Act
            _messagingHubClient.StopAsync().Wait();

            // Assert
            _clientChannel.State.ShouldBe(SessionState.Failed);
            transport.CloseAsync(CancellationToken.None).Wait();
        }
    }
}
