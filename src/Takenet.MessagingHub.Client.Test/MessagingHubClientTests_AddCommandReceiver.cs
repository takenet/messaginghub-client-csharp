using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Resources;
using Lime.Protocol;
using NSubstitute;
using NUnit.Framework;
using Takenet.MessagingHub.Client.Listener;

namespace Takenet.MessagingHub.Client.Test
{
    [TestFixture]
    internal class MessagingHubClientTests_AddCommandReceiver : MessagingHubClientTestBase
    {
        private Command _someCommand => new Command { Id = EnvelopeId.NewId(), Method = CommandMethod.Get, Resource = new Contact(), Uri = new LimeUri("lime://test/command") };

        private ICommandReceiver _commandReceiver;

        [SetUp]
        protected override void Setup()
        {
            base.Setup();
            _commandReceiver = Substitute.For<ICommandReceiver>();
        }

        [TearDown]
        protected override void TearDown()
        {
            base.TearDown();
        }

        [Test]
        public async Task Add_CommandReceiver_And_Process_Command_With_Success()
        {
            //Arrange
            MessagingHubListener.AddCommandReceiver(_commandReceiver);
            await MessagingHubConnection.ConnectAsync();
            await MessagingHubListener.StartAsync();

            //Act
            DispatchCommand();
            await Task.Delay(TIME_OUT);

            //Assert
            Assert.AreEqual(CommandStatus.Pending, _someCommand.Status);
            _commandReceiver.ReceivedWithAnyArgs().ReceiveAsync(null, CancellationToken.None);
        }

        [Test]
        public async Task Add_CommandReceiver_And_Send_Command_Response_Should_Not_Process()
        {
            //Arrange
            MessagingHubListener.AddCommandReceiver(_commandReceiver);
            await MessagingHubConnection.ConnectAsync();
            await MessagingHubListener.StartAsync();

            //Act
            CommandProducer.Add(new Command
            {
                Id = EnvelopeId.NewId(),
                Method = CommandMethod.Get,
                Resource = new Contact(),
                Uri = new LimeUri("lime://test/command"),
                Status = CommandStatus.Success
            });
            await Task.Delay(TIME_OUT);

            //Assert
            _commandReceiver.DidNotReceiveWithAnyArgs().ReceiveAsync(null, CancellationToken.None);
        }

        [Test]
        public async Task Add_CommandReceiver_And_Send_Command_ResponseFailure_Should_Not_Process()
        {
            //Arrange
            MessagingHubListener.AddCommandReceiver(_commandReceiver);
            await MessagingHubConnection.ConnectAsync();
            await MessagingHubListener.StartAsync();

            //Act
            CommandProducer.Add(new Command
            {
                Id = EnvelopeId.NewId(),
                Method = CommandMethod.Get,
                Resource = new Contact(),
                Uri = new LimeUri("lime://test/command"),
                Status = CommandStatus.Failure
            });
            await Task.Delay(TIME_OUT);

            //Assert
            _commandReceiver.DidNotReceiveWithAnyArgs().ReceiveAsync(null, CancellationToken.None);
        }

        [Test]
        public async Task Add_CommandReceiver_Process_Command_And_Stop_With_Success()
        {
            //Arrange
            MessagingHubListener.AddCommandReceiver(_commandReceiver);

            await MessagingHubConnection.ConnectAsync();
            await MessagingHubListener.StartAsync();
            DispatchCommand();
            await Task.Delay(TIME_OUT);

            //Act
            await MessagingHubListener.StopAsync();
            await MessagingHubConnection.DisconnectAsync();

            //Assert
            _commandReceiver.ReceivedWithAnyArgs().ReceiveAsync(null, CancellationToken.None);
        }

        [Test]
        public async Task Add_Multiple_CommandReceivers_And_Process_Command_With_Success()
        {
            //Arrange
            var otherCommandReceiver = Substitute.For<ICommandReceiver>();
            MessagingHubListener.AddCommandReceiver(_commandReceiver);
            MessagingHubListener.AddCommandReceiver(otherCommandReceiver);

            await MessagingHubConnection.ConnectAsync();
            await MessagingHubListener.StartAsync();

            //Act
            DispatchCommand();
            await Task.Delay(TIME_OUT);

            //Assert
            _commandReceiver.ReceivedWithAnyArgs().ReceiveAsync(null, CancellationToken.None);
            otherCommandReceiver.ReceivedWithAnyArgs().ReceiveAsync(null, CancellationToken.None);
        }

        [Test]
        public async Task Add_Multiple_CommandReceivers_With_Different_Filters_And_Process_Command_With_Success()
        {
            //Arrange
            var otherCommandReceiver = Substitute.For<ICommandReceiver>();
            MessagingHubListener.AddCommandReceiver(_commandReceiver, c => c.Uri.Equals(_someCommand.Uri));
            MessagingHubListener.AddCommandReceiver(otherCommandReceiver, c => c.Uri.Equals(new LimeUri("lime://test/message")));

            await MessagingHubConnection.ConnectAsync();
            await MessagingHubListener.StartAsync();

            //Act
            DispatchCommand();
            await Task.Delay(TIME_OUT);

            //Assert
            _commandReceiver.ReceivedWithAnyArgs().ReceiveAsync(null, CancellationToken.None);
            otherCommandReceiver.DidNotReceiveWithAnyArgs().ReceiveAsync(null, CancellationToken.None);
        }

        private void DispatchCommand()
        {
            CommandProducer.Add(_someCommand);
        }
    }
}
