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
    class MessagingHubClientTests_SendCommand : MessagingHubClientTestBase
    {
        public Command SomeCommand => new Command { Resource = new PlainDocument(MediaTypes.PlainText) };
        
        private ICommandReceiver _commandReceiver;
        private SemaphoreSlim _semaphore;

        [SetUp]
        protected override void Setup()
        {
            base.Setup();
            _commandReceiver = Substitute.For<ICommandReceiver>();
        }

        [Test]
        [Ignore]
        public void WhenClientSendACommandShouldReceiveACommandResponse()
        {
            //Arrange
            _messagingHubClient.UsingAccount("login", "pass");
            //_messagingHubClient.AddCommandReceiver(_commandReceiver);

            _semaphore = new SemaphoreSlim(2);

            //Act
            _messagingHubClient.StartAsync().Wait();
            
            Task.Delay(3000).Wait();

            //Assert
            _commandReceiver.ReceivedWithAnyArgs().ReceiveAsync(null).Wait();

            _semaphore.DisposeIfDisposable();
        }

        [Test]
        public void WhenClientTrySendACommandBeforeStartClientShowThrowAException()
        {
            //Arrange
            _messagingHubClient.UsingAccount("login", "pass");

            //Act / Assert
            Should.ThrowAsync<InvalidOperationException>(async () => await _messagingHubClient.SendCommandAsync(Arg.Any<Command>()).ConfigureAwait(false)).Wait();
        }
    }
}
