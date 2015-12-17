using Lime.Protocol;
using Lime.Protocol.Network;
using NSubstitute;
using NUnit.Framework;
using Shouldly;
using System;

namespace Takenet.MessagingHub.Client.Test
{
    [TestFixture]
    internal class MessagingHubClientTests_Start : MessagingHubClientTestBase
    {
        [SetUp]
        protected override void Setup()
        {
            base.Setup();
        }

        [Test]
        public void WhenClientStartUsingAccountShouldConnectToServer()
        {
            // Arrange
            MessagingHubClient.UsingAccount("login", "pass");
            SessionFactory.WhenForAnyArgs(s => s.CreateSessionAsync(null, null, null)).Do(s => ClientChannel.State.Returns(SessionState.Established));

            // Act
            MessagingHubClient.StartAsync().Wait();

            // Assert
            ClientChannel.State.ShouldBe(SessionState.Established);
        }

        [Test]
        public void WhenClientStartUsingAccessKeyShouldConnectToServer()
        {
            // Arrange
            MessagingHubClient.UsingAccessKey("login", "key");
            SessionFactory.WhenForAnyArgs(s => s.CreateSessionAsync(null, null, null)).Do(s => ClientChannel.State.Returns(SessionState.Established));

            // Act
            MessagingHubClient.StartAsync().Wait();

            // Assert
            ClientChannel.State.ShouldBe(SessionState.Established);
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
            SessionFactory.CreateSessionAsync(null, null, null).ReturnsForAnyArgs(session);

            // Act /  Assert
            Should.ThrowAsync<InvalidOperationException>(async () => await MessagingHubClient.StartAsync()).Wait();
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
            SessionFactory.CreateSessionAsync(null, null, null).ReturnsForAnyArgs(session);


            MessagingHubClient.UsingAccount("login", "pass");

            // Act
            var exception = Should.ThrowAsync<LimeException>(async () => await MessagingHubClient.StartAsync()).Result;

            // Assert
            exception.Reason.Description.ShouldBe(session.Reason.Description);
            exception.Reason.Code.ShouldBe(session.Reason.Code);
        }

    }
}
