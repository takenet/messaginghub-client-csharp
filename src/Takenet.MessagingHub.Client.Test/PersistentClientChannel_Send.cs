using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Lime.Protocol;
using Shouldly;
using NSubstitute;

namespace Takenet.MessagingHub.Client.Test
{
    [TestFixture]
    internal class PersistentClientChannel_Send : PersistentClientChannel_Base
    {
        [SetUp]
        protected override void Setup()
        {
            base.Setup();
        }

        [Test]
        public void PersistentClientChannel_Send_Message_Should_not_throw()
        {
            PersistentClientChannel.StartAsync().Wait();

            PersistentClientChannel.SendMessageAsync(DefaultMessage)
                .ShouldNotThrow();
        }

        [Test]
        public void PersistentClientChannel_Send_Command_Should_not_throw()
        {
            PersistentClientChannel.StartAsync().Wait();

            PersistentClientChannel.SendCommandAsync(DefaultCommand)
                .ShouldNotThrow();
        }

        [Test]
        public void PersistentClientChannel_Send_Notification_Should_not_throw()
        {
            PersistentClientChannel.StartAsync().Wait();

            PersistentClientChannel.SendNotificationAsync(DefaultNotification)
                .ShouldNotThrow();
        }

        [Test]
        public void PersistentClientChannel_Send_Message_Disconnected_Should_Timeout()
        {
            PersistentClientChannel.StartAsync().Wait();

            LimeSessionProvider.IsSessionEstablished(null).ReturnsForAnyArgs(false);

            PersistentClientChannel.SendMessageAsync(DefaultMessage)
                .ShouldThrow<OperationCanceledException>();
        }

        [Test]
        public void PersistentClientChannel_Send_Message_Disconnected_Should_Timeout_Then_Should_Connect()
        {
            PersistentClientChannel.StartAsync().Wait();

            LimeSessionProvider.IsSessionEstablished(null).ReturnsForAnyArgs(false);

            PersistentClientChannel.SendMessageAsync(DefaultMessage)
                .ShouldThrow<OperationCanceledException>();
            
            LimeSessionProvider.IsSessionEstablished(null).ReturnsForAnyArgs(true);
            
            PersistentClientChannel.SendMessageAsync(DefaultMessage)
                .ShouldNotThrow();
        }

    }
}
