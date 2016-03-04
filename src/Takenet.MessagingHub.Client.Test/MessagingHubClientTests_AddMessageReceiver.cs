using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using NSubstitute;
using NUnit.Framework;
using Shouldly;
using Takenet.MessagingHub.Client.Receivers;
using System.Collections.Concurrent;
using System;

namespace Takenet.MessagingHub.Client.Test
{
    [TestFixture]
    internal class MessagingHubClientTests_AddMessageReceiver : MessagingHubClientTestBase
    {
        private Message SomeMessage => new Message { Content = new PlainDocument(MediaTypes.PlainText) };

        private IMessageReceiver _messageReceiver;

        [SetUp]
        protected override void Setup()
        {
            base.Setup();
            _messageReceiver = Substitute.For<IMessageReceiver>();
        }

        [TearDown]
        protected override void TearDown()
        {
            base.TearDown();
        }

        [Test]
        public async Task Add_MessageReceiver_And_Process_Message_With_Success()
        {
            //Arrange
            EnvelopeListenerRegistrar.AddMessageReceiver(_messageReceiver);
            await MessagingHubClient.StartAsync();

            //Act
            DispatchMessage();
            await Task.Delay(TIME_OUT);

            //Assert
            _messageReceiver.ReceivedWithAnyArgs().ReceiveAsync(null);
        }

        [Test]
        public async Task Add_MessageReceiver_Process_Message_And_Stop_With_Success()
        {
            //Arrange
            EnvelopeListenerRegistrar.AddMessageReceiver(_messageReceiver);

            await MessagingHubClient.StartAsync();
            DispatchMessage();
            await Task.Delay(TIME_OUT);

            //Act
            await MessagingHubClient.StopAsync();

            //Assert
            _messageReceiver.ReceivedWithAnyArgs().ReceiveAsync(null);
        }

        [Test]
        public async Task Add_Base_MessageReceiver_And_Process_Message_With_Success()
        {
            //Arrange
            var messageReceiver = Substitute.For<MessageReceiverBase>();
            EnvelopeListenerRegistrar.AddMessageReceiver(messageReceiver);
            await MessagingHubClient.StartAsync();

            //Act
            DispatchMessage();
            await Task.Delay(TIME_OUT);

            //Assert
            messageReceiver.ReceivedWithAnyArgs().ReceiveAsync(null);
            messageReceiver.EnvelopeSender.ShouldNotBeNull();
        }

        [Test]
        public async Task Add_Multiple_MessageReceivers_And_Process_Message_With_Success()
        {
            //Arrange
            var otherMessageReceiver = Substitute.For<IMessageReceiver>();
            EnvelopeListenerRegistrar.AddMessageReceiver(_messageReceiver);
            EnvelopeListenerRegistrar.AddMessageReceiver(otherMessageReceiver);

            await MessagingHubClient.StartAsync();

            //Act
            DispatchMessage();
            await Task.Delay(TIME_OUT);

            //Assert
            _messageReceiver.ReceivedWithAnyArgs().ReceiveAsync(null);
            otherMessageReceiver.ReceivedWithAnyArgs().ReceiveAsync(null);
        }

        private void DispatchMessage()
        {
            MessageProducer.Add(SomeMessage);
        }

    }
}
