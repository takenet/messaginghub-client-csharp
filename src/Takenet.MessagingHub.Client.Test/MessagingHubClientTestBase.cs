using Lime.Protocol;
using Lime.Protocol.Client;
using NSubstitute;
using System;
using System.Threading;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client.Lime;

namespace Takenet.MessagingHub.Client.Test
{
    internal class MessagingHubClientTestBase
    {
        protected IMessagingHubClient MessagingHubClient;
        protected IPersistentClientChannel ClientChannel;
        protected ISessionFactory SessionFactory;
        protected IClientChannelFactory ClientChannelFactory;
        protected ICommandProcessorFactory CommandProcessorFactory;
        protected ICommandProcessor CommandProcessor;
        private const string hostName = "msging.net";
        private const string domainName = "msging.net";


        protected virtual void Setup()
        {
            SubstituteClientChannel();

            SubstituteSetPresence();

            SubstituteSessionFabrication();

            SubstituteEnvelopeProcessor();

            SubstituteEnvelopeProcessorFabrication();

            SubstituteClientChannelFabrication();

            InstantiateActualMessageHubClient();
        }

        private void InstantiateActualMessageHubClient()
        {
            MessagingHubClient = new MessagingHubClient(ClientChannelFactory, SessionFactory, CommandProcessorFactory, hostName, domainName);
        }

        private void SubstituteClientChannelFabrication()
        {
            ClientChannelFactory = Substitute.For<IClientChannelFactory>();
            ClientChannelFactory.CreatePersistentClientChannelAsync(null,TimeSpan.Zero,null,null, null).ReturnsForAnyArgs(ClientChannel);
        }

        private void SubstituteEnvelopeProcessorFabrication()
        {
            CommandProcessorFactory = Substitute.For<ICommandProcessorFactory>();
            CommandProcessorFactory.Create(null).ReturnsForAnyArgs(CommandProcessor);
        }

        private void SubstituteEnvelopeProcessor()
        {
            CommandProcessor = Substitute.For<ICommandProcessor>();
        }

        private void SubstituteSessionFabrication()
        {
            var session = new Session {State = SessionState.Established};

            SessionFactory = Substitute.For<ISessionFactory>();
            SessionFactory.CreateSessionAsync(null, null, null).ReturnsForAnyArgs(session);
        }

        private void SubstituteClientChannel()
        {
            ClientChannel = Substitute.For<IPersistentClientChannel>();

            ClientChannel.ReceiveNotificationAsync(Arg.Any<CancellationToken>())
                .ReturnsForAnyArgs(p =>
                {
                    var taskCompletionSource = new TaskCompletionSource<Notification>();
                    var token = p.Arg<CancellationToken>();
                    token.Register(() => taskCompletionSource.TrySetCanceled());
                    return taskCompletionSource.Task;
                });

            ClientChannel.ReceiveMessageAsync(Arg.Any<CancellationToken>())
                .ReturnsForAnyArgs(p =>
                {
                    var taskCompletionSource = new TaskCompletionSource<Message>();
                    var token = p.Arg<CancellationToken>();
                    token.Register(() => taskCompletionSource.TrySetCanceled());
                    return taskCompletionSource.Task;
                });
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
