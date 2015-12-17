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
        public void WhenClientSendACommandShouldReceiveACommandResponse()
        {
            //Arrange
            MessagingHubClient.UsingAccount("login", "pass");
            var commandId = Guid.NewGuid();

            var commandResponse = new Command()
            {
                Id = commandId,
                Status = CommandStatus.Success,
            };
            
            CommandProcessor.SendAsync(null, TimeSpan.Zero).ReturnsForAnyArgs(commandResponse);

            //Act
            MessagingHubClient.StartAsync().Wait();
            
            var result = MessagingHubClient.SendCommandAsync(new Command() { Id = commandId }).Result;

            //Assert
            CommandProcessor.ReceivedWithAnyArgs().SendAsync(null,TimeSpan.Zero).Wait();
            result.ShouldNotBeNull();
            result.Status.ShouldBe(CommandStatus.Success);
            result.Id.ShouldBe(commandId);
        }

        [Test]
        public void WhenClientTrySendACommandBeforeStartClientShowThrowAException()
        {
            //Arrange
            MessagingHubClient.UsingAccount("login", "pass");

            //Act / Assert
            Should.ThrowAsync<InvalidOperationException>(async () => await MessagingHubClient.SendCommandAsync(Arg.Any<Command>()).ConfigureAwait(false)).Wait();
        }
    }
}
