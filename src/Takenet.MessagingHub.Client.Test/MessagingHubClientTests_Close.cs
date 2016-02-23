using System;
using System.Threading;
using Lime.Protocol;
using Lime.Protocol.Network;
using NSubstitute;
using NUnit.Framework;
using Shouldly;

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
        [Ignore("Timing issues?")]
        public void Start_Then_Stop_Should_Stop_PersistentClientChannel()
        {
            //Arrange
            MessagingHubClient.StartAsync().Wait(); 

            // Act
            MessagingHubClient.StopAsync().Wait();

            // Assert
            PersistentClientChannel.Received(1).StopAsync().Wait();
        }

        [Test]
        public void Stop_Without_Start_Should_Throw_Exception()
        {
            // Act // Assert
            Should.ThrowAsync<InvalidOperationException>(async () => await MessagingHubClient.StopAsync()).Wait();
        }

        [Test]
        public void Start_With_Error_On_PersistentClientChannel_Should_Throw_Exception()
        {
            //Arrange
            PersistentClientChannel.StartAsync().Returns(System.Threading.Tasks.Task.Run(() => { throw new LimeException(1,"Error"); }));
            
            // Act / Assert
            Should.ThrowAsync<LimeException>(async () => await MessagingHubClient.StartAsync());
        }
    }
}
