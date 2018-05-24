using Lime.Messaging.Resources;
using Lime.Protocol;
using Lime.Protocol.Listeners;
using Lime.Protocol.Security;
using Lime.Protocol.Util;
using NUnit.Framework;
using Shouldly;
using System.Linq;
using System.Threading.Tasks;


namespace Takenet.MessagingHub.Client.Test
{
    [TestFixture]
    public class MessagingHubClientBuilderTests : TestsBase
    {
        public IChannelListener ChannelListener { get; set; }

        public DummyServer Server { get; set; }

        public MessagingHubClientBuilder GetTarget()
            => new MessagingHubClientBuilder()
                .UsingHostName(Server.ListenerUri.Host)
                .UsingPort(Server.ListenerUri.Port);

        [SetUp]
        public void Setup()
        {
            ChannelListener = new ChannelListener(
                m => TaskUtil.TrueCompletedTask,
                n => TaskUtil.TrueCompletedTask,
                c => TaskUtil.TrueCompletedTask);
            Server = new DummyServer();
            Lime.Messaging.Registrator.RegisterDocuments();
        }

        [TearDown]
        public async Task TearDown()
        {
            await Server.StopAsync();
        }

        [Test]
        public async Task BuildUsingAccessKeyAndDefaultSettingsShouldSucceed()
        {
            // Arrange
            var identifier = Dummy.CreateRandomString(10);
            var accessKey = Dummy.CreateRandomString(10);
            await Server.StartAsync(CancellationToken);
            var target = GetTarget();

            // Act
            var actual = target
                .UsingAccessKey(identifier, accessKey)
                .Build();

            await actual.StartAsync(ChannelListener, CancellationToken);

            // Assert
            Server.Channels.Count.ShouldBe(1);
            var serverChannel = Server.Channels.Dequeue();
            serverChannel.RemoteNode.Name.ShouldBe(identifier);
            var authentication = Server.Authentications.First();
            var keyAuthentication = authentication.ShouldBeOfType<KeyAuthentication>();
            keyAuthentication.Key.ShouldBe(accessKey);
            Server.Commands.Count.ShouldBe(2);
            var presenceCommand = Server.Commands.Dequeue();
            presenceCommand.Method.ShouldBe(CommandMethod.Set);
            presenceCommand.Uri.ToString().ShouldBe("/presence");
            var presence = presenceCommand.Resource.ShouldBeOfType<Presence>();
            presence.Status.ShouldBe(PresenceStatus.Available);
            presence.RoutingRule.ShouldBe(target.RoutingRule);
            presence.RoundRobin.ShouldBe(true);
            var receiptCommand = Server.Commands.Dequeue();
            receiptCommand.Method.ShouldBe(CommandMethod.Set);
            receiptCommand.Uri.ToString().ShouldBe("/receipt");
            var receipt = receiptCommand.Resource.ShouldBeOfType<Receipt>();
            receipt.Events.ShouldBe(target.ReceiptEvents);
        }

        [Test]
        public async Task BuildUsingPasswordShouldSucceed()
        {
            // Arrange
            var identifier = Dummy.CreateRandomString(10);
            var password = Dummy.CreateRandomString(10);
            await Server.StartAsync(CancellationToken);
            var target = GetTarget();

            // Act
            var actual = target
                .UsingPassword(identifier, password)
                .Build();

            await actual.StartAsync(ChannelListener, CancellationToken);

            // Assert
            Server.Channels.Count.ShouldBe(1);
            var serverChannel = Server.Channels.Dequeue();
            serverChannel.RemoteNode.Name.ShouldBe(identifier);
            var authentication = Server.Authentications.First();
            var plainAuthentication = authentication.ShouldBeOfType<PlainAuthentication>();
            plainAuthentication.Password.ShouldBe(password);
        }

        [Test]
        public async Task BuildUsingDomainAndInstanceShouldSucceed()
        {
            // Arrange
            var identifier = Dummy.CreateRandomString(10);
            var accessKey = Dummy.CreateRandomString(10);
            var domain = Dummy.CreateRandomString(10);
            var instance = Dummy.CreateRandomString(10);
            await Server.StartAsync(CancellationToken);
            var target = GetTarget();

            // Act
            var actual = target
                .UsingAccessKey(identifier, accessKey)
                .UsingDomain(domain)
                .UsingInstance(instance)
                .Build();

            await actual.StartAsync(ChannelListener, CancellationToken);

            // Assert
            Server.Channels.Count.ShouldBe(1);
            var serverChannel = Server.Channels.Dequeue();
            serverChannel.RemoteNode.Name.ShouldBe(identifier);
            serverChannel.RemoteNode.Domain.ShouldBe(domain);
            serverChannel.RemoteNode.Instance.ShouldBe(instance);
        }

        [Test]
        public async Task BuildWithMultipleChannelCountShouldSucceed()
        {
            // Arrange
            var identifier = Dummy.CreateRandomString(10);
            var accessKey = Dummy.CreateRandomString(10);
            await Server.StartAsync(CancellationToken);
            var channelCount = 5;
            var target = GetTarget();

            // Act
            var actual = target
                .UsingAccessKey(identifier, accessKey)
                .WithChannelCount(channelCount)
                .Build();

            await actual.StartAsync(ChannelListener, CancellationToken);

            // Assert
            Server.Channels.Count.ShouldBe(channelCount);
            while (Server.Channels.Count > 0)
            {
                var serverChannel = Server.Channels.Dequeue();
                serverChannel.RemoteNode.Name.ShouldBe(identifier);
            }
        }

        public override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Server.StopAsync(CancellationToken).Wait();
                Server.Dispose();
            }

            base.Dispose(disposing);
        }

    }
}

