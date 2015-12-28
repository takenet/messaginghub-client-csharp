using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Shouldly;

namespace Takenet.MessagingHub.Client.Test
{
    [TestFixture]
    class PersistentClientChannel_Receive : PersistentClientChannel_Base
    {
        [SetUp]
        protected override void Setup()
        {
            base.Setup();
        }

        [Test]
        public void PersistentClientChannel_Receive_Message_Should_not_throw()
        {
            PersistentClientChannel.StartAsync().Wait();

            var message = PersistentClientChannel.ReceiveMessageAsync(CancellationToken.None).Result;

            message.From.ShouldBe(DefaultMessage.From);
            message.To.ShouldBe(DefaultMessage.To);
        }
    }
}
