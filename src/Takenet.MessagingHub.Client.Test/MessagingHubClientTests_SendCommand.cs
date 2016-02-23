using Lime.Protocol;
using NSubstitute;
using NUnit.Framework;
using Shouldly;
using System;

namespace Takenet.MessagingHub.Client.Test
{
    [TestFixture]
    internal class MessagingHubClientTests_SendCommand : MessagingHubClientTestBase
    {
        [SetUp]
        protected override void Setup()
        {
            base.Setup();
        }

        [Test]
        public void Send_Command_And_Receive_Response_With_Success()
        {
            //Arrange
            var commandId = Guid.NewGuid();

            var commandResponse = new Command
            {
                Id = commandId,
                Status = CommandStatus.Success,
            };
            

            //Act
            MessagingHubClient.StartAsync().Wait();
            
            var result = MessagingHubClient.SendCommandAsync(new Command { Id = commandId }).Result;

            //Assert
            result.ShouldNotBeNull();
            result.Status.ShouldBe(CommandStatus.Success);
            result.Id.ShouldBe(commandId);
        }

        [Test]
        public void Send_Command_Without_Start_Should_Throw_Exception()
        {
            //Act / Assert
            Should.ThrowAsync<InvalidOperationException>(async () => await MessagingHubClient.SendCommandAsync(Arg.Any<Command>()).ConfigureAwait(false)).Wait();
        }
    }
}
