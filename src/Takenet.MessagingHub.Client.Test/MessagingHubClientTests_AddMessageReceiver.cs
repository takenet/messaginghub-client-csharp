using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using NSubstitute;
using NUnit.Framework;
using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.Messages;

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
            MessagingHubListener.AddMessageReceiver(_messageReceiver);
            await MessagingHubConnection.ConnectAsync();
            await MessagingHubListener.StartAsync();

            //Act
            DispatchMessage();
            await Task.Delay(TIME_OUT);

            //Assert
            _messageReceiver.ReceivedWithAnyArgs().ReceiveAsync(null, null, CancellationToken.None);
        }

        [Test]
        public async Task Add_MessageReceiver_Process_Message_And_Stop_With_Success()
        {
            //Arrange
            MessagingHubListener.AddMessageReceiver(_messageReceiver);

            await MessagingHubConnection.ConnectAsync();
            await MessagingHubListener.StartAsync();
            DispatchMessage();
            await Task.Delay(TIME_OUT);

            //Act
            await MessagingHubListener.StopAsync();
            await MessagingHubConnection.DisconnectAsync();

            //Assert
            _messageReceiver.ReceivedWithAnyArgs().ReceiveAsync(null, null, CancellationToken.None);
        }

        [Test]
        public async Task Add_Base_MessageReceiver_And_Process_Message_With_Success()
        {
            //Arrange
            var messageReceiver = Substitute.For<MessageReceiverBase>();
            MessagingHubListener.AddMessageReceiver(messageReceiver);
            await MessagingHubConnection.ConnectAsync();
            await MessagingHubListener.StartAsync();

            //Act
            DispatchMessage();
            await Task.Delay(TIME_OUT);

            //Assert
            messageReceiver.ReceivedWithAnyArgs().ReceiveAsync(null, null, CancellationToken.None);
        }

        [Test]
        public async Task Add_Multiple_MessageReceivers_And_Process_Message_With_Success()
        {
            //Arrange
            var otherMessageReceiver = Substitute.For<IMessageReceiver>();
            MessagingHubListener.AddMessageReceiver(_messageReceiver);
            MessagingHubListener.AddMessageReceiver(otherMessageReceiver);

            await MessagingHubConnection.ConnectAsync();
            await MessagingHubListener.StartAsync();

            //Act
            DispatchMessage();
            await Task.Delay(TIME_OUT);

            //Assert
            _messageReceiver.ReceivedWithAnyArgs().ReceiveAsync(null, null, CancellationToken.None);
            otherMessageReceiver.ReceivedWithAnyArgs().ReceiveAsync(null, null, CancellationToken.None);
        }

        private void DispatchMessage()
        {
            MessageProducer.Add(SomeMessage);
        }

    }
}
