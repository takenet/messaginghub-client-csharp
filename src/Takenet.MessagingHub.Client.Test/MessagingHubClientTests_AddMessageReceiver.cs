using Lime.Protocol;
using Lime.Protocol.Client;
using NSubstitute;
using NUnit.Framework;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client.Lime;
using Takenet.MessagingHub.Client.Receivers;

namespace Takenet.MessagingHub.Client.Test
{
    [TestFixture]
    internal class MessagingHubClientTests_AddMessageReceiver : MessagingHubClientTestBase
    {
        public Message SomeMessage => new Message { Content = new PlainDocument(MediaTypes.PlainText) };
        
        IMessageReceiver _messageReceiver;
        SemaphoreSlim _semaphore;
        //
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
            _messagingHubClient.UsingAccount("login", "pass");
            _messagingHubClient.AddMessageReceiver(_messageReceiver);

            _semaphore = new SemaphoreSlim(1);

            _clientChannel.ReceiveMessageAsync(new CancellationTokenSource().Token).ReturnsForAnyArgs(async (callInfo) =>
            {
                await _semaphore.WaitAsync();
                return SomeMessage;
            });

            //Act
            _messagingHubClient.StartAsync().Wait();

            Task.Delay(3000).Wait();

            //Assert
            _messageReceiver.ReceivedWithAnyArgs().ReceiveAsync(null);

            _semaphore.DisposeIfDisposable();
        }

        [Test]
        public void WhenClientAddAMessageReceiverAndReceiveAMessageShouldBeHandledByReceiverWhenStopped()
        {
            //Arrange
            _messagingHubClient.UsingAccount("login", "pass");
            _messagingHubClient.AddMessageReceiver(_messageReceiver);

            _semaphore = new SemaphoreSlim(1);

            _clientChannel.ReceiveMessageAsync(Arg.Any<CancellationToken>()).ReturnsForAnyArgs(async (callInfo) =>
            {
                await _semaphore.WaitAsync(callInfo.Arg<CancellationToken>());
                return new Message { Content = new PlainDocument(MediaTypes.PlainText) };
            });

            //Act
            _messagingHubClient.StartAsync().Wait();
            
            _messagingHubClient.StopAsync().Wait();

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

            _messagingHubClient.UsingAccount("login", "pass");
            _messagingHubClient.AddMessageReceiver(messageReceiver);

            _semaphore = new SemaphoreSlim(1);

            _clientChannel.ReceiveMessageAsync(new CancellationTokenSource().Token).ReturnsForAnyArgs(async (_) =>
            {
                await _semaphore.WaitAsync().ConfigureAwait(false);
                return SomeMessage;
            });

            //Act
            _messagingHubClient.StartAsync().Wait();

            Task.Delay(3000).Wait();

            //Assert
            messageReceiver.ReceivedWithAnyArgs().ReceiveAsync(null);
            messageReceiver.MessageSender.ShouldNotBeNull();
            messageReceiver.NotificationSender.ShouldNotBeNull();

            _semaphore.DisposeIfDisposable();
        }
    }
}
