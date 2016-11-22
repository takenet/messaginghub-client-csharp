using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Client;
using NSubstitute;
using Takenet.MessagingHub.Client.Connection;
using Takenet.MessagingHub.Client.LimeProtocol;
using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client.Test
{
    internal class MessagingHubClientTestBase
    {
        protected readonly TimeSpan TIME_OUT = TimeSpan.FromSeconds(5);

        protected IMessagingHubConnection MessagingHubConnection;
        protected IOnDemandClientChannel OnDemandClientChannel;
        protected IOnDemandClientChannelFactory OnDemandClientChannelFactory;
        protected IEstablishedClientChannelBuilder EstablishedClientChannelBuilder;
        protected BlockingCollection<Message> MessageProducer;
        protected BlockingCollection<Notification> NotificationProducer;
        protected BlockingCollection<Command> CommandProducer;

        private TimeSpan _sendTimeout = TimeSpan.FromSeconds(20);
        private int _maxConnectionRetries = 1;

        protected IMessagingHubListener MessagingHubListener { get; private set; }
        protected IMessagingHubSender MessagingHubSender { get; private set; }

        protected virtual void Setup()
        {
            CreateProducers();

            SubstituteOnDemandClientChannel();
            SubstituteEstablishedClientChannelBuilder();
            SubstituteOnDemandClientChannelFactory();

            CreateActualMessageHubClient();            
        }

        protected virtual void TearDown()
        {
            MessageProducer.Dispose();
            NotificationProducer.Dispose();
            CommandProducer.Dispose();

            if (MessagingHubConnection.IsConnected)
                MessagingHubConnection.DisconnectAsync().Wait();
        }

        private void SubstituteOnDemandClientChannel()
        {            
            OnDemandClientChannel = Substitute.For<IOnDemandClientChannel>();            
            OnDemandClientChannel.ReceiveMessageAsync(CancellationToken.None).ReturnsForAnyArgs(callInfo => MessageProducer.Take());
            OnDemandClientChannel.ReceiveNotificationAsync(CancellationToken.None).ReturnsForAnyArgs(callInfo => NotificationProducer.Take());
            OnDemandClientChannel.ReceiveCommandAsync(CancellationToken.None).ReturnsForAnyArgs(callInfo => CommandProducer.Take());
            OnDemandClientChannel.ProcessCommandAsync(null, CancellationToken.None).ReturnsForAnyArgs(Task.FromResult(new Command
            {
                Status = CommandStatus.Success
            }));
        }

        private void SubstituteEstablishedClientChannelBuilder()
        {
            EstablishedClientChannelBuilder = Substitute.For<IEstablishedClientChannelBuilder>();
        }

        private void SubstituteOnDemandClientChannelFactory()
        {
            OnDemandClientChannelFactory = Substitute.For<IOnDemandClientChannelFactory>();
            OnDemandClientChannelFactory.ChannelBuilder.Returns(EstablishedClientChannelBuilder);
            OnDemandClientChannelFactory.Create(1).Returns(OnDemandClientChannel);
        }

        private void CreateProducers()
        {
            MessageProducer = new BlockingCollection<Message>();
            NotificationProducer = new BlockingCollection<Notification>();
            CommandProducer = new BlockingCollection<Command>();
        }
        
        private void CreateActualMessageHubClient()
        {
            MessagingHubConnection = new MessagingHubConnection(_sendTimeout, _maxConnectionRetries, OnDemandClientChannelFactory, 1);
            MessagingHubListener = new MessagingHubListener(MessagingHubConnection);
            MessagingHubSender = new MessagingHubSender(MessagingHubConnection);
        }
    }
}
