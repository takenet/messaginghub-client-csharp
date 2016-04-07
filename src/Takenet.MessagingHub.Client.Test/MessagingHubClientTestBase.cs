using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Client;
using Lime.Protocol.Security;
using Lime.Protocol.Serialization.Newtonsoft;
using Lime.Protocol.Server;
using Lime.Protocol.Util;
using Lime.Transport.Tcp;
using NSubstitute;
using Takenet.MessagingHub.Client.Deprecated;
using Takenet.MessagingHub.Client.LimeProtocol;

namespace Takenet.MessagingHub.Client.Test
{
    internal class MessagingHubClientTestBase
    {
        protected readonly TimeSpan TIME_OUT = TimeSpan.FromSeconds(2.5);

        protected MessagingHubClient MessagingHubClient;
        protected IOnDemandClientChannel OnDemandClientChannel;
        protected IOnDemandClientChannelFactory OnDemandClientChannelFactory;
        protected IEstablishedClientChannelBuilder EstablishedClientChannelBuilder;
        protected EnvelopeListenerRegistrar EnvelopeListenerRegistrar;
        protected BlockingCollection<Message> MessageProducer;
        protected BlockingCollection<Notification> NotificationProducer;
        protected BlockingCollection<Command> CommandProducer;
        private TimeSpan _sendTimeout = TimeSpan.FromSeconds(20);
        
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

            if (MessagingHubClient.Started) MessagingHubClient.StopAsync().Wait();            
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
            OnDemandClientChannelFactory.Create(EstablishedClientChannelBuilder).Returns(OnDemandClientChannel);
        }

        private void CreateProducers()
        {
            MessageProducer = new BlockingCollection<Message>();
            NotificationProducer = new BlockingCollection<Notification>();
            CommandProducer = new BlockingCollection<Command>();
        }
        
        private void CreateActualMessageHubClient()
        {
            EnvelopeListenerRegistrar = new EnvelopeListenerRegistrar();
            MessagingHubClient = new MessagingHubClient(EstablishedClientChannelBuilder, OnDemandClientChannelFactory, _sendTimeout, EnvelopeListenerRegistrar);            
        }


    }
}
