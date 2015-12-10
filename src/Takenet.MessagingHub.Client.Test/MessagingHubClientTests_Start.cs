using Lime.Messaging.Resources;
using Lime.Protocol;
using NSubstitute;
using NUnit.Framework;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client.Test
{
    [TestFixture]
    public class MessagingHubClientTests_Start
    {
        private MessagingHubClientSUT _SUT;

        [SetUp]
        public void Setup()
        {
            _SUT = new MessagingHubClientSUT("hostname");
            var commandSent = new Command();
            _SUT.ClientChannel.WhenForAnyArgs(c => c.SendCommandAsync(null)).Do(c => 
                commandSent = c.Arg<Command>());
            _SUT.ClientChannel.ReceiveCommandAsync(Arg.Any<CancellationToken>()).Returns(c => new Command { Id = commandSent.Id, Status = CommandStatus.Success });
        }


        [Test]
        public void WhenClientStartUsingAccountShouldConnectToServer()
        {
            // Arrange
            _SUT.UsingAccount("login", "pass");

            // Act
            var x = _SUT.StartAsync().Result;

            // Assert
            _SUT.ClientChannelCreated.ShouldBe(true);
            _SUT.ClientChannel.Received().SendCommandAsync(
                Arg.Is<Command>(c => c.Uri.ToString().Equals(UriTemplates.PRESENCE)));
        }

        [Test]
        public void WhenClientStartUsingAccessKeyShouldConnectToServer()
        {
            // Arrange
            _SUT.UsingAccessKey("login", "key");

            // Act
            var x = _SUT.StartAsync().Result;

            // Assert
            _SUT.ClientChannelCreated.ShouldBe(true);
        }

        [Test]
        public void WhenClientStartWithoutCredentialsShouldThrowException()
        {
            // Arrange

            // Act
            Should.ThrowAsync<InvalidOperationException>(async () => await _SUT.StartAsync()).Wait();

            // Assert
            _SUT.ClientChannelCreated.ShouldBe(false);
        }


        [Test]
        public void WhenClientStartAndServerDoNotAcceptTheSessionShouldThrowException()
        {
            // Arrange
            var reason = new Reason { Code = 1, Description = "failure message" };
            _SUT.SetSessionResult(SessionState.Failed, reason);
            _SUT.UsingAccount("login", "pass");

            // Act
            var exception = Should.ThrowAsync<Exception>(async () => await _SUT.StartAsync()).Result;

            // Assert
            exception.Message.ShouldContain(reason.Description);
            exception.Message.ShouldContain(reason.Code.ToString());
        }

    }
}
