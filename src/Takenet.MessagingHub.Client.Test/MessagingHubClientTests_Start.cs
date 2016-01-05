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
            
            // Act
            MessagingHubClient.StartAsync().Wait();

            // Assert
            PersistentClientChannel.Received(1).StartAsync();
        }

        [Test]
        public void Start_UsingAccessKey_Should_Succeed()
        {
            // Act
            MessagingHubClient.StartAsync().Wait();

            // Assert
            PersistentClientChannel.Received(1).StartAsync();
        }
    }
}
