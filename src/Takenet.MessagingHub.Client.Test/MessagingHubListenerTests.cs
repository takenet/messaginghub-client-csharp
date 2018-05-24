using Lime.Messaging.Contents;
using Lime.Protocol;
using NSubstitute;
using NUnit.Framework;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client.Test
{
    [TestFixture]
    public class MessagingHubListenerTests : TestsBase
    {
        public IMessagingHubSender Sender { get; set; }

        public bool AutoNotify { get; set; }

        public FakeEstablishedReceiverChannel EstablishedReceiverChannel { get; set; }


        private MessagingHubListener GetTarget()
        {
            return new MessagingHubListener(Sender, AutoNotify);
        }

        [SetUp]
        public void Setup()
        {
            Sender = Substitute.For<IMessagingHubSender>();
            EstablishedReceiverChannel = new FakeEstablishedReceiverChannel();
            AutoNotify = true;
        }

        [Test]
        public async Task StartingShouldStartConsumingTheReceiver()
        {
            // Arrange
            var target = GetTarget();
            await EstablishedReceiverChannel.MessageBuffer.SendAsync(Dummy.CreateMessage());
            await EstablishedReceiverChannel.NotificationBuffer.SendAsync(Dummy.CreateNotification());
            await EstablishedReceiverChannel.CommandBuffer.SendAsync(Dummy.CreateCommand());

            // Act
            target.Start(EstablishedReceiverChannel);

            // Assert
            await Task.Delay(250);
            EstablishedReceiverChannel.MessageBuffer.Count.ShouldBe(0);
            EstablishedReceiverChannel.NotificationBuffer.Count.ShouldBe(0);
            EstablishedReceiverChannel.CommandBuffer.Count.ShouldBe(0);
        }


        [Test]
        public void StartingTwiceShouldThrowInvalidOperationException()
        {
            // Arrange
            var target = GetTarget();

            // Act
            target.Start(EstablishedReceiverChannel);

            // Assert
            Assert.Throws<InvalidOperationException>(() => target.Start(EstablishedReceiverChannel));
        }

        [Test]
        public async Task RegisteringMessageReceiverWithoutPredicateShouldBeCalledWhenAnyMessageIsReceived()
        {
            // Arrange
            var target = GetTarget();
            var messageReceiver = Substitute.For<IMessageReceiver>();
            var message = Dummy.CreateMessage();
            await EstablishedReceiverChannel.MessageBuffer.SendAsync(message);

            // Act
            target.AddMessageReceiver(messageReceiver);
            target.Start(EstablishedReceiverChannel);

            // Assert
            await Task.Delay(250);
            messageReceiver.Received(1).ReceiveAsync(message, Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task RegisteringMessageReceiverWithTextTypePredicateShouldBeCalledWhenTextMessageIsReceived()
        {
            // Arrange
            var target = GetTarget();
            var messageReceiver = Substitute.For<IMessageReceiver>();
            var chatStateMessage = Dummy.CreateMessage(Dummy.CreateChatState());
            var textMessage = Dummy.CreateMessage(Dummy.CreateTextContent());
            await EstablishedReceiverChannel.MessageBuffer.SendAsync(chatStateMessage);
            await EstablishedReceiverChannel.MessageBuffer.SendAsync(textMessage);

            // Act
            target.AddMessageReceiver(messageReceiver, m => m.Type.Equals(PlainText.MediaType).AsCompletedTask());
            target.Start(EstablishedReceiverChannel);

            // Assert
            await Task.Delay(250);
            messageReceiver.Received(1).ReceiveAsync(textMessage, Arg.Any<CancellationToken>());
            messageReceiver.DidNotReceive().ReceiveAsync(chatStateMessage, Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task RegisteringMultipleMessageReceiverWithSamePredicateShouldBeCalledWhenSpecificMessageTypeIsReceived()
        {
            // Arrange
            var target = GetTarget();
            var textMessageReceiver1 = Substitute.For<IMessageReceiver>();
            var textMessageReceiver2 = Substitute.For<IMessageReceiver>();
            var textMessage = Dummy.CreateMessage(Dummy.CreateTextContent());
            await EstablishedReceiverChannel.MessageBuffer.SendAsync(textMessage);

            // Act
            target.AddMessageReceiver(textMessageReceiver1, m => m.Type.Equals(PlainText.MediaType).AsCompletedTask());
            target.AddMessageReceiver(textMessageReceiver2, m => m.Type.Equals(PlainText.MediaType).AsCompletedTask());
            target.Start(EstablishedReceiverChannel);

            // Assert
            await Task.Delay(250);
            textMessageReceiver1.Received(1).ReceiveAsync(textMessage, Arg.Any<CancellationToken>());
            textMessageReceiver2.Received(1).ReceiveAsync(textMessage, Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task RegisteringMultipleMessageReceiverWithDifferentTypePredicateShouldBeCalledWhenSpecificMessageTypeIsReceived()
        {
            // Arrange
            var target = GetTarget();
            var textMessageReceiver = Substitute.For<IMessageReceiver>();
            var chatStateMessageReceiver = Substitute.For<IMessageReceiver>();
            var textMessage = Dummy.CreateMessage(Dummy.CreateTextContent());
            var chatStateMessage = Dummy.CreateMessage(Dummy.CreateChatState());
            await EstablishedReceiverChannel.MessageBuffer.SendAsync(textMessage);
            await EstablishedReceiverChannel.MessageBuffer.SendAsync(chatStateMessage);

            // Act
            target.AddMessageReceiver(textMessageReceiver, m => m.Type.Equals(PlainText.MediaType).AsCompletedTask());
            target.AddMessageReceiver(chatStateMessageReceiver, m => m.Type.Equals(ChatState.MediaType).AsCompletedTask());
            target.Start(EstablishedReceiverChannel);

            // Assert
            await Task.Delay(250);
            textMessageReceiver.Received(1).ReceiveAsync(textMessage, Arg.Any<CancellationToken>());
            textMessageReceiver.DidNotReceive().ReceiveAsync(chatStateMessage, Arg.Any<CancellationToken>());
            chatStateMessageReceiver.Received(1).ReceiveAsync(chatStateMessage, Arg.Any<CancellationToken>());
            chatStateMessageReceiver.DidNotReceive().ReceiveAsync(textMessage, Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task RegisteringMultipleMessageReceiverWithDifferentTypePrioritiesShouldBeCalledWhenSpecificMessageTypeIsReceived()
        {
            // Arrange
            var target = GetTarget();
            var textMessageReceiver1 = Substitute.For<IMessageReceiver>();
            var textMessageReceiver2 = Substitute.For<IMessageReceiver>();
            var textContent1 = "first expected content";
            var textContent2 = "not expected content";
            var textMessage1 = Dummy.CreateMessage(textContent1);
            var textMessage2 = Dummy.CreateMessage(textContent2);
            await EstablishedReceiverChannel.MessageBuffer.SendAsync(textMessage1);
            await EstablishedReceiverChannel.MessageBuffer.SendAsync(textMessage2);

            // Act
            target.AddMessageReceiver(textMessageReceiver1, m => (m.Type.Equals(PlainText.MediaType) && m.Content.ToString().Equals(textContent1)).AsCompletedTask(), 1);
            target.AddMessageReceiver(textMessageReceiver2, null, 2);
            target.Start(EstablishedReceiverChannel);

            // Assert
            await Task.Delay(250);
            textMessageReceiver1.Received(1).ReceiveAsync(textMessage1, Arg.Any<CancellationToken>());
            textMessageReceiver1.DidNotReceive().ReceiveAsync(textMessage2, Arg.Any<CancellationToken>());
            textMessageReceiver2.Received(1).ReceiveAsync(textMessage2, Arg.Any<CancellationToken>());
            textMessageReceiver2.DidNotReceive().ReceiveAsync(textMessage1, Arg.Any<CancellationToken>());
        }
    }
}
