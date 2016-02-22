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
        }

        [TearDown]
        public void TearDown()
        {
            _semaphore.Dispose();
        }

        [Test]
        public void Add_MessageReceiver_And_Process_Message_With_Success()
        {
            //Arrange
            MessagingHubClient.AddMessageReceiver(_messageReceiver);

            _semaphore = new SemaphoreSlim(1);

            PersistentClientChannel.ReceiveMessageAsync(new CancellationTokenSource().Token).ReturnsForAnyArgs(async (callInfo) =>
            {
                await _semaphore.WaitAsync();
                return SomeMessage;
            });

            //Act
            MessagingHubClient.StartAsync().Wait();

            Task.Delay(3000).Wait();

            //Assert
            _messageReceiver.ReceivedWithAnyArgs().ReceiveAsync(null);
        }

        [Test]
        public void Add_MessageReceiver_Process_Message_And_Stop_With_Success()
        {
            //Arrange
            MessagingHubClient.AddMessageReceiver(_messageReceiver);

            _semaphore = new SemaphoreSlim(1);

            PersistentClientChannel.ReceiveMessageAsync(Arg.Any<CancellationToken>()).ReturnsForAnyArgs(async (callInfo) =>
            {
                await _semaphore.WaitAsync(callInfo.Arg<CancellationToken>());
                return new Message { Content = new PlainDocument(MediaTypes.PlainText) };
            });
            
            //Act
            MessagingHubClient.StartAsync().Wait();

            Task.Delay(3000).Wait();

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

            MessagingHubClient.AddMessageReceiver(messageReceiver);

            _semaphore = new SemaphoreSlim(1);

            PersistentClientChannel.ReceiveMessageAsync(new CancellationTokenSource().Token).ReturnsForAnyArgs(async (_) =>
            {
                await _semaphore.WaitAsync().ConfigureAwait(false);
                return SomeMessage;
            });

            //Act
            MessagingHubClient.StartAsync().Wait();

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

            MessagingHubClient.AddMessageReceiver(_messageReceiver);
            MessagingHubClient.AddMessageReceiver(otherMessageReceiver);

            _semaphore = new SemaphoreSlim(1);

            PersistentClientChannel.ReceiveMessageAsync(new CancellationTokenSource().Token).ReturnsForAnyArgs(async (_) =>
            {
                await _semaphore.WaitAsync().ConfigureAwait(false);
                return SomeMessage;
            });

            //Act
            MessagingHubClient.StartAsync().Wait();

            Task.Delay(3000).Wait();

            //Assert
            _messageReceiver.ReceivedWithAnyArgs().ReceiveAsync(null);
            otherMessageReceiver.ReceivedWithAnyArgs().ReceiveAsync(null);
        }

    }
}
