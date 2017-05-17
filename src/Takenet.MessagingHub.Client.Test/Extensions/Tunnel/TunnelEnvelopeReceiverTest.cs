using Lime.Protocol;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client.Extensions.Tunnel;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client.Test.Extensions.Tunnel
{
    [TestFixture]
    public class TunnelEnvelopeReceiverTest
    {
        private IMessagingHubSender _sender;

        [SetUp]
        protected void Setup()
        {
            _sender = Substitute.For<IMessagingHubSender>();
        }

        public TunnelEnvelopeReceiver<Message> GetMessageTarget() => new TunnelEnvelopeReceiver<Message>(_sender.SendMessageAsync);

        [Test]
        public async Task Receive_Valid_Tunnel_Envelope_Should_Forward_To_Originator()
        {
            // Arrange
            var message = new Message
            {
                From = "children-bot@tunnel.msging.net/originator%40domain.local%2Finstance",
                To = "master-bot@msging.net",
                Content = "Hello"
            };

            var target = GetMessageTarget();

            // Act
            await target.ReceiveAsync(message, CancellationToken.None);

            // Assert
            _sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.To == "originator@domain.local/instance"
                        && m.From == null
                        && m.Content == message.Content),
                    Arg.Any<CancellationToken>());
        }

        [Test]        
        public async Task Receive_Invalid_Tunnel_Envelope_Should_Throw_Argument_Exception()
        {
            // Arrange
            var message = new Message
            {
                From = "originator@domain.local/instance",
                To = "master-bot@msging.net",
                Content = "Hello"
            };

            var target = GetMessageTarget();

            // Act
            Assert.ThrowsAsync<ArgumentException>(async () => await target.ReceiveAsync(message, CancellationToken.None));
        }
    }
}
