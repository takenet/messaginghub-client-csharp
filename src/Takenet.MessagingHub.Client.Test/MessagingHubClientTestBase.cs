using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Client;
using Lime.Protocol.Security;
using NSubstitute;
using Takenet.MessagingHub.Client.LimeProtocol;

namespace Takenet.MessagingHub.Client.Test
{
    internal class MessagingHubClientTestBase
    {
        protected MessagingHubClient MessagingHubClient;
        protected IClientChannel ClientChannel;
        protected IPersistentLimeSession PersistentClientChannel;
        protected IPersistentLimeSessionFactory PersistentClientChannelFactory;
        protected IClientChannelFactory ClientChannelFactory;
        protected ILimeSessionProvider LimeSessionProvider;
        protected EnvelopeListenerRegistrar EnvelopeListenerRegistrar;

        protected string AccessKey = "1234";
        private Uri _endPoint = new Uri("net.tcp://msg.net:12345");
        private Identity _identity => new Identity("developerTakenet","msging.net");
        private TimeSpan _sendTimeout = TimeSpan.FromSeconds(20);


        protected virtual void Setup()
        {
            SubstitutePersistentLimeSession();
            SubstituteSetPresence();            
            SubstituteClientChannel();
            SubstituteClientChannelFabrication();
            SubstituteLimeSessionProvider();
            SubstituteForPersistentClientChannelFactory();

            InstantiateActualMessageHubClient();
        }

        private void SubstituteClientChannel()
        {
            ClientChannel = Substitute.For<IClientChannel>();
        }

        private void SubstituteForPersistentClientChannelFactory()
        {
            PersistentClientChannelFactory = Substitute.For<IPersistentLimeSessionFactory>();
            PersistentClientChannelFactory.CreatePersistentClientChannelAsync(null, TimeSpan.Zero, null, null, null, null)
                .ReturnsForAnyArgs(PersistentClientChannel);
        }

        private void SubstituteLimeSessionProvider()
        {
            LimeSessionProvider = Substitute.For<ILimeSessionProvider>();
        }

        private void InstantiateActualMessageHubClient()
        {
            EnvelopeListenerRegistrar = new EnvelopeListenerRegistrar();
            MessagingHubClient = new MessagingHubClient(_identity, new KeyAuthentication { Key = AccessKey }, _endPoint, _sendTimeout, PersistentClientChannelFactory, ClientChannelFactory, LimeSessionProvider, EnvelopeListenerRegistrar);
        }

        private void SubstituteClientChannelFabrication()
        {
            ClientChannelFactory = Substitute.For<IClientChannelFactory>();
            ClientChannelFactory.CreateClientChannelAsync(TimeSpan.Zero).ReturnsForAnyArgs(ClientChannel);
        }

        
        private void SubstitutePersistentLimeSession()
        {
            PersistentClientChannel = Substitute.For<IPersistentLimeSession>();


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
