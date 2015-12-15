using Lime.Protocol;
using Lime.Protocol.Client;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client.Lime;

namespace Takenet.MessagingHub.Client.Test
{
    internal class MessagingHubClientTestBase
    {
        protected MessagingHubClient _messagingHubClient;
        protected IClientChannel _clientChannel;
        protected ISessionFactory _sessionFactory;
        protected IEnvelopeProcessorFactory<Command> _envelopeProcessorFactory;
        protected IEnvelopeProcessor<Command> _commandProcessor;

        public virtual void Setup()
        {
            _clientChannel = Substitute.For<IClientChannel>();

            var presenceCommand = new Command();
            _clientChannel.WhenForAnyArgs(c => c.SendCommandAsync(null)).Do(c =>
                presenceCommand = c.Arg<Command>());
            _clientChannel.ReceiveCommandAsync(Arg.Any<CancellationToken>()).Returns(c => new Command { Id = presenceCommand.Id, Status = CommandStatus.Success });

            var session = new Session { State = SessionState.Established };

            var clientChannelFactory = Substitute.For<IClientChannelFactory>();
            clientChannelFactory.CreateClientChannelAsync(null).ReturnsForAnyArgs(_clientChannel);

            _sessionFactory = Substitute.For<ISessionFactory>();
            _sessionFactory.CreateSessionAsync(null, null, null).ReturnsForAnyArgs(session);

            _commandProcessor = Substitute.For<IEnvelopeProcessor<Command>>();
            _envelopeProcessorFactory = Substitute.For<IEnvelopeProcessorFactory<Command>>();
            _envelopeProcessorFactory.Create(null).ReturnsForAnyArgs(_commandProcessor);

            _messagingHubClient = new MessagingHubClient(clientChannelFactory, _sessionFactory, _envelopeProcessorFactory, "msging.net");
        }
    }
}
