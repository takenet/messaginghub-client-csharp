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
    [TestFixture]
    internal class MessagingHubClientTests_Start : MessagingHubClientTestBase
    {
        [SetUp]
        void Setup()
        {
            base.Setup();
        }

        [Test]
        public void WhenClientStartUsingAccountShouldConnectToServer()
        {
            // Arrange
            _messagingHubClient.UsingAccount("login", "pass");
            _sessionFactory.WhenForAnyArgs(s => s.CreateSessionAsync(null, null, null)).Do(s => _clientChannel.State.Returns(SessionState.Established));

            // Act
            _messagingHubClient.StartAsync().Wait();

            // Assert
            _clientChannel.State.ShouldBe(SessionState.Established);
        }

        [Test]
        public void WhenClientStartUsingAccessKeyShouldConnectToServer()
        {
            // Arrange
            _messagingHubClient.UsingAccessKey("login", "key");
            _sessionFactory.WhenForAnyArgs(s => s.CreateSessionAsync(null, null, null)).Do(s => _clientChannel.State.Returns(SessionState.Established));

            // Act
            _messagingHubClient.StartAsync().Wait();

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
