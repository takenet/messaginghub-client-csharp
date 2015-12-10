using Lime.Protocol;
using NSubstitute;
using NUnit.Framework;
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
        public void WhenClientStartShouldBeConnected()
        {
            _SUT.UsingAccount("login", "pass");
            var x = _SUT.StartAsync().Result;
        }
    }
}
