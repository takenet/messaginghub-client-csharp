using Lime.Protocol;
using Lime.Protocol.Network;
using NSubstitute;
using NUnit.Framework;
using Shouldly;
using System;
using System.Threading;

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
            PersistentClientChannel.Received(1).StartAsync();
        }

        [Test]
        public void Start_UsingAccessKey_Should_Succeed()
        {
            // Arrange
            MessagingHubClient.UsingAccessKey("login", "key");
            
            // Act
            MessagingHubClient.StartAsync().Wait();

            // Assert
            PersistentClientChannel.Received(1).StartAsync();
        }

        [Test]
        public void Start_Without_Credential_Should_Throw_Exception()
        {
            // Act /  Assert
            Should.ThrowAsync<InvalidOperationException>(async () => await MessagingHubClient.StartAsync()).Wait();
        }
    }
}
