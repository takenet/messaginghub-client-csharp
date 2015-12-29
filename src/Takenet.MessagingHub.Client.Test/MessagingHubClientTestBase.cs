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
        protected IClientChannel ClientChannel;
        protected IPersistentClientChannel PersistentClientChannel;
        protected IPersistentClientChannelFactory PersistentClientChannelFactory;
        protected IClientChannelFactory ClientChannelFactory;
        protected ICommandProcessorFactory CommandProcessorFactory;
        protected ICommandProcessor CommandProcessor;
        protected ILimeSessionProvider LimeSessionProvider;
        private const string hostName = "msging.net";
        private const string domainName = "msging.net";


        protected virtual void Setup()
        {
            SubstituteClientChannel();

            SubstituteSetPresence();
            
            SubstituteEnvelopeProcessor();

            SubstituteEnvelopeProcessorFabrication();

            SubstituteClientChannelFabrication();

            SubstituteLimeSessionProvider();

            SubstituteForPersistentClientChannelFactory();

            InstantiateActualMessageHubClient();
        }

        private void SubstituteForPersistentClientChannelFactory()
        {
            PersistentClientChannelFactory = Substitute.For<IPersistentClientChannelFactory>();
            PersistentClientChannelFactory.CreatePersistentClientChannelAsync(null, TimeSpan.Zero, null, null, null, null)
                .ReturnsForAnyArgs(PersistentClientChannel);
        }

        private void SubstituteLimeSessionProvider()
        {
            LimeSessionProvider = Substitute.For<ILimeSessionProvider>();
        }

        private void InstantiateActualMessageHubClient()
        {
            MessagingHubClient = new MessagingHubClient(PersistentClientChannelFactory, ClientChannelFactory, CommandProcessorFactory, LimeSessionProvider, hostName, domainName);
        }

        private void SubstituteClientChannelFabrication()
        {
            ClientChannelFactory = Substitute.For<IClientChannelFactory>();
            ClientChannelFactory.CreateClientChannelAsync(TimeSpan.Zero).ReturnsForAnyArgs(ClientChannel);
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
        
        private void SubstituteClientChannel()
        {
            PersistentClientChannel = Substitute.For<IPersistentClientChannel>();


            PersistentClientChannel.ReceiveNotificationAsync(CancellationToken.None)
                .ReturnsForAnyArgs(p =>
                {
                    var taskCompletionSource = new TaskCompletionSource<Notification>();
                    var token = p.Arg<CancellationToken>();
                    token.Register(() => taskCompletionSource.TrySetCanceled());
                    return taskCompletionSource.Task;
                });

            PersistentClientChannel.ReceiveMessageAsync(CancellationToken.None)
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
            PersistentClientChannel.WhenForAnyArgs(c => c.SendCommandAsync(null)).Do(c =>
                presenceCommand = c.Arg<Command>());
            PersistentClientChannel.ReceiveCommandAsync(Arg.Any<CancellationToken>())
                .Returns(c => new Command { Id = presenceCommand.Id, Status = CommandStatus.Success });
        }
    }
}
