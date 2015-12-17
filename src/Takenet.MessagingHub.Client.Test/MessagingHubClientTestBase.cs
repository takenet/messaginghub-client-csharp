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
        protected IEnvelopeProcessorFactory<Command> CommandProcessorFactory;
        protected IEnvelopeProcessorFactory<Message> MessageProcessorFactory;
        protected IEnvelopeProcessorFactory<Notification> NotificationProcessorFactory;
        protected IEnvelopeProcessor<Command> CommandProcessor;
        protected IEnvelopeProcessor<Message> MessageProcessor;
        protected IEnvelopeProcessor<Notification> NotificationProcessor;

        protected virtual void Setup()
        {
            SubstituteClientChannel();

            SubstituteSetPresence();

            SubstituteSessionFabrication();

            SubstituteEnvelopeProcessor();

            SubstituteEnvelopeProcessorFabrication();

            SubstituteClientChannelFabrication();

            InstanciateActualMessageHubClient();
        }

        private void InstanciateActualMessageHubClient()
        {
            MessagingHubClient = new MessagingHubClient(ClientChannelFactory, SessionFactory, CommandProcessorFactory, MessageProcessorFactory, NotificationProcessorFactory);
        }

        private void SubstituteClientChannelFabrication()
        {
            ClientChannelFactory = Substitute.For<IClientChannelFactory>();
            ClientChannelFactory.CreateClientChannelAsync(null).ReturnsForAnyArgs(ClientChannel);
        }

        private void SubstituteEnvelopeProcessorFabrication()
        {
            CommandProcessorFactory = Substitute.For<IEnvelopeProcessorFactory<Command>>();
            CommandProcessorFactory.Create(null).ReturnsForAnyArgs(CommandProcessor);

            MessageProcessorFactory = Substitute.For<IEnvelopeProcessorFactory<Message>>();
            MessageProcessorFactory.Create(null).ReturnsForAnyArgs(MessageProcessor);

            NotificationProcessorFactory = Substitute.For<IEnvelopeProcessorFactory<Notification>>();
            NotificationProcessorFactory.Create(null).ReturnsForAnyArgs(NotificationProcessor);
        }

        private void SubstituteEnvelopeProcessor()
        {
            CommandProcessor = Substitute.For<IEnvelopeProcessor<Command>>();
            MessageProcessor = Substitute.For<IEnvelopeProcessor<Message>>();
            NotificationProcessor = Substitute.For<IEnvelopeProcessor<Notification>>();
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
