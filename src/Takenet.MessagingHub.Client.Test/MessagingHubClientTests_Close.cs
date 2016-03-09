using System;
using System.Threading;
using Lime.Protocol;
using Lime.Protocol.Network;
using NSubstitute;
using NUnit.Framework;
using Shouldly;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client.Test
{
    internal class MessagingHubClientTests_Close : MessagingHubClientTestBase
    {
        [SetUp]
        protected override void Setup()
        {
            base.Setup();
        }

        [TearDown]
        protected override void TearDown()
        {
            base.TearDown();
        }

        [Test]
        public async Task Start_Then_Stop_Should_Finish_OnDemandClientChannel()
        {
            //Arrange
            await MessagingHubClient.StartAsync(); 

            // Act
            await MessagingHubClient.StopAsync();

            // Assert
            OnDemandClientChannel.ReceivedWithAnyArgs(1).FinishAsync(CancellationToken.None).Wait();
        }

        [Test]
        public void Stop_Without_Start_Should_Throw_Exception()
        {
            // Act // Assert
            Should.ThrowAsync<InvalidOperationException>(async () => await MessagingHubClient.StopAsync()).Wait();
        }
        
    }
}
