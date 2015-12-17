using Lime.Protocol;
using NSubstitute;
using NUnit.Framework;
using Shouldly;
using System.Threading;
using System.Threading.Tasks;
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

        [Test]
        public void WhenClientAddAMessageReceiverAndReceiveAMessageShouldBeHandledByReceiver()
        {
            //Arrange
            MessagingHubClient.UsingAccount("login", "pass");
            MessagingHubClient.AddMessageReceiver(_messageReceiver);

            _semaphore = new SemaphoreSlim(1);

            ClientChannel.ReceiveMessageAsync(new CancellationTokenSource().Token).ReturnsForAnyArgs(async (callInfo) =>
            {
                await _semaphore.WaitAsync();
                return SomeMessage;
            });

            //Act
            MessagingHubClient.StartAsync().Wait();

            Task.Delay(3000).Wait();

            //Assert
            _messageReceiver.ReceivedWithAnyArgs().ReceiveAsync(null);

            _semaphore.DisposeIfDisposable();
        }

        [Test]
        public void WhenClientAddAMessageReceiverAndReceiveAMessageShouldBeHandledByReceiverWhenStopped()
        {
            //Arrange
            MessagingHubClient.UsingAccount("login", "pass");
            MessagingHubClient.AddMessageReceiver(_messageReceiver);

            _semaphore = new SemaphoreSlim(1);

            ClientChannel.ReceiveMessageAsync(Arg.Any<CancellationToken>()).ReturnsForAnyArgs(async (callInfo) =>
            {
                await _semaphore.WaitAsync(callInfo.Arg<CancellationToken>());
                return new Message { Content = new PlainDocument(MediaTypes.PlainText) };
            });

            //Act
            MessagingHubClient.StartAsync().Wait();
            
            MessagingHubClient.StopAsync().Wait();

            Task.Delay(3000).Wait();

            //Assert
            _messageReceiver.ReceivedWithAnyArgs().ReceiveAsync(null);

            _semaphore.DisposeIfDisposable();
        }

        [Test]
        public void WhenClientAddAMessageReceiverBaseAndReceiveAMessageTheReceiverShouldHandleAndBeSet()
        {
            //Arrange

            var messageReceiver = Substitute.For<MessageReceiverBase>();

            MessagingHubClient.UsingAccount("login", "pass");
            MessagingHubClient.AddMessageReceiver(messageReceiver);

            _semaphore = new SemaphoreSlim(1);

            ClientChannel.ReceiveMessageAsync(new CancellationTokenSource().Token).ReturnsForAnyArgs(async (_) =>
            {
                await _semaphore.WaitAsync().ConfigureAwait(false);
                return SomeMessage;
            });

            //Act
            MessagingHubClient.StartAsync().Wait();

            Task.Delay(3000).Wait();

            //Assert
            messageReceiver.ReceivedWithAnyArgs().ReceiveAsync(null);
            messageReceiver.MessageSender.ShouldNotBeNull();
            messageReceiver.NotificationSender.ShouldNotBeNull();
            messageReceiver.CommandSender.ShouldNotBeNull();

            _semaphore.DisposeIfDisposable();
        }
    }
}
