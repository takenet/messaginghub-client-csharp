using Lime.Protocol;
using Lime.Protocol.Client;
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
using Takenet.MessagingHub.Client.Receivers;

namespace Takenet.MessagingHub.Client.Test
{
    [TestFixture]
    class MessagingHubClientTests_SendCommand : MessagingHubClientTestBase
    {
        public Command SomeCommand => new Command { Resource = new PlainDocument(MediaTypes.PlainText) };
        
        private ICommandReceiver _commandReceiver;
        

        [SetUp]
        protected override void Setup()
        {
            base.Setup();
            _commandReceiver = Substitute.For<ICommandReceiver>();
        }

        [Test]
        public void WhenClientSendACommandShouldReceiveACommandResponse()
        {
            //Arrange
            _messagingHubClient.UsingAccount("login", "pass");
            var commandId = Guid.NewGuid();

            var commandResponse = new Command()
            {
                Id = commandId,
                Status = CommandStatus.Success,
            };
            
            _commandProcessor.SendReceiveAsync(null, TimeSpan.Zero).ReturnsForAnyArgs(commandResponse);

            //Act
            _messagingHubClient.StartAsync().Wait();

            var result = _messagingHubClient.CommandSender.SendCommandAsync(new Command() { Id = commandId }).Result;

            //Assert
            _commandProcessor.ReceivedWithAnyArgs().SendReceiveAsync(null,TimeSpan.Zero).Wait();
            result.ShouldNotBeNull();
            result.Status.ShouldBe(CommandStatus.Success);
            result.Id.ShouldBe(commandId);
        }

        [Test]
        public void WhenClientTrySendACommandBeforeStartClientShowThrowAException()
        {
            //Arrange
            _messagingHubClient.UsingAccount("login", "pass");

            //Act / Assert
            Should.ThrowAsync<InvalidOperationException>(async () => await _messagingHubClient.SendCommandAsync(Arg.Any<Command>()).ConfigureAwait(false)).Wait();
        }
    }
}
