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
        public void Start_UsingAccount_Should_Succeed()
        {
            // Arrange
            MessagingHubClient.UsingAccount("login", "pass");
            
            // Act
            MessagingHubClient.StartAsync().Wait();

            // Assert
            ClientChannel.Received(1).StartAsync();
        }

        [Test]
        public void Start_UsingAccessKey_Should_Succeed()
        {
            // Arrange
            MessagingHubClient.UsingAccessKey("login", "key");
            
            // Act
            MessagingHubClient.StartAsync().Wait();

            // Assert
            ClientChannel.Received(1).StartAsync();
        }

        [Test]
        public void Start_Without_Credential_Should_Throw_Exception()
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
    }
}
