using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Shouldly;
using Lime.Protocol;
using NSubstitute;
using Lime.Protocol.Network;
using NSubstitute.Core;

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

            Message message;

            using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(2)))
            {
                message = PersistentClientChannel.ReceiveMessageAsync(cancellationTokenSource.Token).Result;

                cancellationTokenSource.IsCancellationRequested.ShouldNotBe(true);
            }

            message.ShouldNotBeNull();
            message.From.ShouldBe(DefaultMessage.From);
            message.To.ShouldBe(DefaultMessage.To);
        }

        [Test]
        public void PersistentClientChannel_Receive_Message_Disconnected_Should_timeout()
        {
            PersistentClientChannel.StartAsync().Wait();

            LimeSessionProvider.IsSessionEstablished(null).ReturnsForAnyArgs(false);
            Transport.ReceiveAsync(CancellationToken.None).ReturnsForAnyArgs((Func<CallInfo,Envelope>)(c => { throw new LimeException(1,"Session is not estabilished"); }));

            using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(2)))
            {
                PersistentClientChannel.ReceiveMessageAsync(cancellationTokenSource.Token).ShouldThrow<OperationCanceledException>();

                cancellationTokenSource.IsCancellationRequested.ShouldBe(true);
            }
        }
    }
}
