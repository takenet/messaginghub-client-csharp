using Lime.Protocol;
using Lime.Protocol.Network;
using NSubstitute;
using NUnit.Framework;
using Shouldly;
using System;
using System.Threading;

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
            ClientChannel.WhenForAnyArgs(c => c.SendFinishingSessionAsync()).Do(c => ClientChannel.State.Returns(SessionState.Finished));

            ClientChannel.State.Returns(SessionState.Established);
            MessagingHubClient.UsingAccessKey("login", "key");
            MessagingHubClient.StartAsync().Wait(); 

            // Act
            MessagingHubClient.StopAsync().Wait();

            // Assert
            ClientChannel.State.ShouldBe(SessionState.Finished);
        }

        [Test]
        public void WhenClientIsNotConnectedAndCloseConnectionShouldThrowException()
        {
            //Arrange
            MessagingHubClient.UsingAccessKey("login", "key");
            
            // Act // Assert
            Should.ThrowAsync<InvalidOperationException>(async () => await MessagingHubClient.StopAsync()).Wait();
        }

        [Test]
        public void WhenClientHasntEstablishedSessionAndCloseConnectionShouldDisconnectFromServer()
        {
            //Arrange
            ClientChannel.State.Returns(SessionState.Failed);

            var transport = Substitute.For<ITransport>();
            ClientChannel.Transport.Returns(transport);

            MessagingHubClient.UsingAccessKey("login", "key");
            MessagingHubClient.StartAsync().Wait();

            // Act
            MessagingHubClient.StopAsync().Wait();

            // Assert
            ClientChannel.State.ShouldBe(SessionState.Failed);
            transport.CloseAsync(CancellationToken.None).Wait();
        }
    }
}
