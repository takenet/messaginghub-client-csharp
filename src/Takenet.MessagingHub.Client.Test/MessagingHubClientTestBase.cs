using Lime.Protocol;
using Lime.Protocol.Client;
using NSubstitute;
using System.Threading;
using Takenet.MessagingHub.Client.Lime;

namespace Takenet.MessagingHub.Client.Test
{
    internal class MessagingHubClientTestBase
    {
        protected IMessagingHubClient MessagingHubClient;
        protected IClientChannel ClientChannel;
        protected ISessionFactory SessionFactory;
        protected IClientChannelFactory ClientChannelFactory;
        protected IEnvelopeProcessorFactory<Command> EnvelopeProcessorFactory;
        protected IEnvelopeProcessor<Command> CommandProcessor;

        protected virtual void Setup()
        {
            SubstituteClientChannel();

            SubstituteSetPresence();

            SubstituteSessionFabrication();

            SubstituteCommandProcessor();

            SubstituteEnvelopeProcessorFabrication();

            SubstituteClientChannelFabrication();

            InstanciateActualMessageHubClient();
        }

        private void InstanciateActualMessageHubClient()
        {
            MessagingHubClient = new MessagingHubClient(ClientChannelFactory, SessionFactory, EnvelopeProcessorFactory);
        }

        private void SubstituteClientChannelFabrication()
        {
            ClientChannelFactory = Substitute.For<IClientChannelFactory>();
            ClientChannelFactory.CreateClientChannelAsync(null).ReturnsForAnyArgs(ClientChannel);
        }

        private void SubstituteEnvelopeProcessorFabrication()
        {
            EnvelopeProcessorFactory = Substitute.For<IEnvelopeProcessorFactory<Command>>();
            EnvelopeProcessorFactory.Create(null).ReturnsForAnyArgs(CommandProcessor);
        }

        private void SubstituteCommandProcessor()
        {
            CommandProcessor = Substitute.For<IEnvelopeProcessor<Command>>();
        }

        private void SubstituteSessionFabrication()
        {
            var session = new Session {State = SessionState.Established};

            SessionFactory = Substitute.For<ISessionFactory>();
            SessionFactory.CreateSessionAsync(null, null, null).ReturnsForAnyArgs(session);
        }

        private void SubstituteClientChannel()
        {
            ClientChannel = Substitute.For<IClientChannel>();
        }

        private void SubstituteSetPresence()
        {
            var presenceCommand = new Command();
            ClientChannel.WhenForAnyArgs(c => c.SendCommandAsync(null)).Do(c =>
                presenceCommand = c.Arg<Command>());
            ClientChannel.ReceiveCommandAsync(Arg.Any<CancellationToken>())
                .Returns(c => new Command {Id = presenceCommand.Id, Status = CommandStatus.Success});
        }
    }
}
