using Lime.Protocol;
using NSubstitute;
using NUnit.Framework;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;

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

        [TearDown]
        protected override void TearDown()
        {
            base.TearDown();
        }

        [Test]
        public async Task Send_Command_And_Receive_Response_With_Success()
        {
            //Arrange
            var commandId = Guid.NewGuid();
            var commandResponse = new Command
            {
                Id = commandId,
                Status = CommandStatus.Success,
            };

            OnDemandClientChannel.ProcessCommandAsync(null, CancellationToken.None).ReturnsForAnyArgs(commandResponse);
            await MessagingHubClient.StartAsync();

            //Act
            var result = await MessagingHubClient.SendCommandAsync(new Command { Id = commandId });
            await Task.Delay(TIME_OUT);

            //Assert
            OnDemandClientChannel.ReceivedWithAnyArgs().ReceiveCommandAsync(CancellationToken.None).Wait();
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
