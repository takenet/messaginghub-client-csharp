using Lime.Protocol;
using NSubstitute;
using NUnit.Framework;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client.Receivers;

namespace Takenet.MessagingHub.Client.Test
{
    [TestFixture]
    internal class MessagingHubClientTests_SendCommand : MessagingHubClientTestBase
    {
        private Command SomeCommand => new Command { Resource = new PlainDocument(MediaTypes.PlainText) };

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
            MessagingHubClient.UsingAccount("login", "pass");
            //MessagingHubClient.AddCommandReceiver(_commandReceiver);

            _semaphore = new SemaphoreSlim(2);

            //Act
            MessagingHubClient.StartAsync().Wait();
            
            Task.Delay(3000).Wait();

            //Assert
            _commandReceiver.ReceivedWithAnyArgs().ReceiveAsync(null).Wait();

            _semaphore.DisposeIfDisposable();
        }

        [Test]
        public void WhenClientTrySendACommandBeforeStartClientShowThrowAException()
        {
            //Arrange
            MessagingHubClient.UsingAccount("login", "pass");

            //Act / Assert
            Should.ThrowAsync<InvalidOperationException>(async () => await MessagingHubClient.SendCommandAsync(Arg.Any<Command>()).ConfigureAwait(false)).Wait();
        }
    }
}
