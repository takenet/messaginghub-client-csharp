using NUnit.Framework;
using System;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Client;
using Lime.Protocol.Listeners;
using NSubstitute;
using Shouldly;

namespace Takenet.MessagingHub.Client.Test
{
    [TestFixture]
    public class MessagingHubClientTests : TestsBase
    {
        public IOnDemandClientChannel OnDemandClientChannel { get; set; }

        public IChannelListener ChannelListener { get; set; }

        public MessagingHubClient GetTarget() => new MessagingHubClient(OnDemandClientChannel);

        public async Task<MessagingHubClient> GetAndStartTarget()
        {
            var target = GetTarget();
            await target.StartAsync(ChannelListener, CancellationToken);
            return target;
        }

        [SetUp]
        public void Setup()
        {
            OnDemandClientChannel = Substitute.For<IOnDemandClientChannel>();
            ChannelListener = Substitute.For<IChannelListener>();
        }

        [Test]
        public async Task StartShouldStartListenerAndEstablishOnDemandClientChannel()
        {
            // Arrange
            var target = GetTarget();

            // Act
            await target.StartAsync(ChannelListener, CancellationToken);

            // Assert
            ChannelListener.Received(1).Start(OnDemandClientChannel);
            OnDemandClientChannel.Received(1).EstablishAsync(CancellationToken);
        }

        [Test]
        public async Task StartTwiceShouldThrowInvalidOperationException()
        {
            // Arrange
            var target = GetTarget();

            // Act                        
            await target.StartAsync(ChannelListener, CancellationToken);
            await target.StartAsync(ChannelListener, CancellationToken).ShouldThrowAsync<InvalidOperationException>();
        }

        [Test]
        public async Task StopShouldStopListenerAndFinishOnDemandClientChannel()
        {
            // Arrange
            var target = await GetAndStartTarget();

            // Act
            await target.StopAsync(CancellationToken);

            // Assert
            ChannelListener.Received(1).Stop();
            OnDemandClientChannel.Received(1).FinishAsync(CancellationToken);
        }

        [Test]
        public async Task StopWithoutStartShouldThrowInvalidOperationException()
        {
            // Arrange
            var target = GetTarget();

            // Act
            await target.StopAsync(CancellationToken).ShouldThrowAsync<InvalidOperationException>();
        }

        [Test]
        public async Task StopTwiceShouldThrowInvalidOperationException()
        {
            // Arrange
            var target = await GetAndStartTarget();

            // Act
            await target.StopAsync(CancellationToken);
            await target.StopAsync(CancellationToken).ShouldThrowAsync<InvalidOperationException>();
        }

        [Test]
        public async Task ProcessComandShouldCallOnDemandClientChannel()
        {
            // Arrange
            var command = Dummy.CreateCommand();
            var target = await GetAndStartTarget();

            // Act
            await target.SendCommandResponseAsync(command, CancellationToken);

            // Assert
            OnDemandClientChannel.Received(1).ProcessCommandAsync(command, CancellationToken);
        }

        [Test]
        public async Task SendCommandShouldCallOnDemandClientChannel()
        {
            // Arrange
            var command = Dummy.CreateCommand();
            var target = await GetAndStartTarget();

            // Act
            await target.SendCommandAsync(command, CancellationToken);

            // Assert
            OnDemandClientChannel.Received(1).SendCommandAsync(command, CancellationToken);
        }

        [Test]
        public async Task SendMessageShouldCallOnDemandClientChannel()
        {
            // Arrange
            var message = Dummy.CreateMessage(Dummy.CreateTextContent());
            var target = await GetAndStartTarget();

            // Act
            await target.SendMessageAsync(message, CancellationToken);

            // Assert
            OnDemandClientChannel.Received(1).SendMessageAsync(message, CancellationToken);
        }

        [Test]
        public async Task SendNotificationShouldCallOnDemandClientChannel()
        {
            // Arrange
            var notification = Dummy.CreateNotification(Event.Received);
            var target = await GetAndStartTarget();

            // Act
            await target.SendNotificationAsync(notification, CancellationToken);

            // Assert
            OnDemandClientChannel.Received(1).SendNotificationAsync(notification, CancellationToken);
        }

        public void Dispose()
        {
            CancellationTokenSource.Dispose();
        }
    }
}
