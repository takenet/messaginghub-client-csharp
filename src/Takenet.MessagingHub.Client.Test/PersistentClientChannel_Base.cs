using Lime.Protocol;
using Lime.Protocol.Client;
using Lime.Protocol.Network;
using Lime.Protocol.Security;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client.Test
{
    internal class PersistentClientChannel_Base
    {
        protected IPersistentClientChannel PersistentClientChannel;
        protected ILimeSessionProvider LimeSessionProvider;
        protected IClientChannelFactory ClientChannelFactory;
        protected IClientChannel ClientChannel;
        protected TimeSpan SendTimeout;
        protected Session Session;
        protected string Domain = "msging.net";
        protected string Instance = "msgingHubClient";
        protected Uri EndPoint = new Uri("tcp://msging.net.test:55332");


        protected virtual void Setup()
        {
            SendTimeout = TimeSpan.FromSeconds(5);
            LimeSessionProvider = Substitute.For<ILimeSessionProvider>();
            ClientChannelFactory = Substitute.For<IClientChannelFactory>();
            ClientChannel = Substitute.For<IClientChannel>();

            SetEstablishedSession();

            SetDefaultMessageReceiver();

            SetClientChannelFactory();

            PersistentClientChannel = new PersistentClientChannel(EndPoint, DefaultIdentity, DefaultAuthentication, SendTimeout, ClientChannelFactory, LimeSessionProvider);
        }

        private void SetClientChannelFactory()
        {
            ClientChannelFactory.CreateClientChannelAsync(TimeSpan.Zero)
                .ReturnsForAnyArgs(ClientChannel);
        }

        protected void SetEstablishedSession()
        {
            LimeSessionProvider.IsSessionEstablished(null).ReturnsForAnyArgs(true);
        }

        protected void SetDefaultMessageReceiver()
        {
            ClientChannel.ReceiveMessageAsync(CancellationToken.None)
                .ReturnsForAnyArgs(DefaultMessage);
        }

        protected Authentication DefaultAuthentication => new KeyAuthentication();

        protected Identity DefaultIdentity => new Identity
        {
            Domain = Domain,
            Name = "Identity"
        };

        protected Node DefaultFrom => new Node
        {
            Domain = Domain,
            Instance = Instance,
            Name = "Sender"
        };

        protected Node DefaultTo => new Node
        {
            Domain = Domain,
            Instance = Instance,
            Name = "Destination"
        };

        protected Command DefaultCommand => new Command
        {
            From = DefaultFrom,
            To = DefaultTo,
            Method = CommandMethod.Set,
            Uri = new LimeUri("/presence")
        };

        protected Message DefaultMessage => new Message
        {
            From = DefaultFrom,
            To = DefaultTo,
            Content = new PlainDocument(MediaTypes.PlainText)
        };

        protected Notification DefaultNotification => new Notification
        {
            From = DefaultFrom,
            To = DefaultTo,
            Reason = new Reason() { Code = 1, Description = "Received" }
        };

    }
}
