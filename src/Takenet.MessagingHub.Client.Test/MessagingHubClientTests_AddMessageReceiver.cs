using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using NSubstitute;
using NUnit.Framework;
using Shouldly;
using Takenet.MessagingHub.Client.Receivers;

namespace Takenet.MessagingHub.Client.Test
{
    [TestFixture]
    internal class MessagingHubClientTests_AddMessageReceiver : MessagingHubClientTestBase
    {
        private Message SomeMessage => new Message { Content = new PlainDocument(MediaTypes.PlainText) };
        private IMessageReceiver _messageReceiver;
        private SemaphoreSlim _semaphore;

        [SetUp]
        protected override void Setup()
        {
            base.Setup();
            _messageReceiver = Substitute.For<IMessageReceiver>();
            _semaphore = new SemaphoreSlim(1);
            _semaphore.Wait();

            PersistentClientChannel.ReceiveMessageAsync(Arg.Any<CancellationToken>()).ReturnsForAnyArgs(async (_) =>
            {
                await _semaphore.WaitAsync().ConfigureAwait(false);
                return SomeMessage;
            });
        }

        [TearDown]
        public void TearDown()
        {
            _semaphore.Dispose();
            MessagingHubClient.StopAsync().Wait();
        }

        [Test]
        public async Task Add_MessageReceiver_And_Process_Message_With_Success()
        {
            //Arrange
            EnvelopeListenerRegistrar.AddMessageReceiver(_messageReceiver);
            await MessagingHubClient.StartAsync();

            //Act
            DispatchMessage();
            await Task.Delay(3000);

            //Assert
            _messageReceiver.ReceivedWithAnyArgs().ReceiveAsync(null);
        }

        [Test]
        public void Add_MessageReceiver_Process_Message_And_Stop_With_Success()
        {
            //Arrange
            EnvelopeListenerRegistrar.AddMessageReceiver(_messageReceiver);

            MessagingHubClient.StartAsync().Wait();
            Task.Delay(3000).Wait();
            DispatchMessage();

            //Act
            MessagingHubClient.StopAsync().Wait();
            Task.Delay(3000).Wait();

            //Assert
            _messageReceiver.ReceivedWithAnyArgs().ReceiveAsync(null);
        }

        [Test]
        public void Add_Base_MessageReceiver_And_Process_Message_With_Success()
        {
            //Arrange
            var messageReceiver = Substitute.For<MessageReceiverBase>();
            EnvelopeListenerRegistrar.AddMessageReceiver(messageReceiver);
            MessagingHubClient.StartAsync().Wait();

            //Act
            DispatchMessage();
            Task.Delay(3000).Wait();

            //Assert
            messageReceiver.ReceivedWithAnyArgs().ReceiveAsync(null);
            messageReceiver.EnvelopeSender.ShouldNotBeNull();
        }

        [Test]
        public void Add_Multiple_MessageReceivers_And_Process_Message_With_Success()
        {
            //Arrange
            var otherMessageReceiver = Substitute.For<IMessageReceiver>();
            EnvelopeListenerRegistrar.AddMessageReceiver(_messageReceiver);
            EnvelopeListenerRegistrar.AddMessageReceiver(otherMessageReceiver);

            MessagingHubClient.StartAsync().Wait();

            //Act
            DispatchMessage();
            Task.Delay(3000).Wait();

            //Assert
            _messageReceiver.ReceivedWithAnyArgs().ReceiveAsync(null);
            otherMessageReceiver.ReceivedWithAnyArgs().ReceiveAsync(null);
        }

        private void DispatchMessage()
        {
            _semaphore.Release();
        }

    }
}
