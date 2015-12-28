using Lime.Protocol;
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
        protected ITransport Transport;
        protected TimeSpan SendTimeout;
        protected Session Session;
        protected string Domain = "msging.net";
        protected string Instance = "msgingHubClient";

        protected virtual void Setup()
        {
            Transport = Substitute.For<ITransport>();
            SendTimeout = TimeSpan.FromSeconds(5);
            LimeSessionProvider = Substitute.For<ILimeSessionProvider>();

            SetEstablishedSession();

            SetDefaultMessageReceiver();

            PersistentClientChannel = new PersistentClientChannel(Transport, SendTimeout, LimeSessionProvider);
        }

        protected void SetEstablishedSession()
        {
            Transport.IsConnected.Returns(true);

            Session = new Session()
            {
                State = SessionState.Established
            };

            LimeSessionProvider.EstablishSessionAsync(null, CancellationToken.None)
                .ReturnsForAnyArgs(Session);

            LimeSessionProvider.IsSessionEstablished(null).ReturnsForAnyArgs(true);
        }

        protected void SetDefaultMessageReceiver()
        {
            Transport.ReceiveAsync(CancellationToken.None)
                .ReturnsForAnyArgs(DefaultMessage);
        }

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

        protected Command DefaultCommand =>new Command
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
