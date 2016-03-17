using System;
using System.Text;
using NUnit.Framework;
using System.Threading.Tasks;
using Lime.Messaging.Resources;
using Lime.Protocol;
using Shouldly;

namespace Takenet.MessagingHub.Client.Test
{
    [TestFixture]
    internal class MessagingHubClientTests_Start : MessagingHubClientTestBase
    {
        [SetUp]
        protected override void Setup()
        {
            base.Setup();
        }

        [TearDown]
        protected override void TearDown()
        {
            base.TearDown();
        }

        [Test]
        [Ignore("Needs real server connection")]
        public async Task StartSuccessfully()
        {
            var client = new MessagingHubClientBuilder()
                .UsingGuest()
                .Build();

            await client.StartAsync().ConfigureAwait(false);
        }

        [Test]
        [Ignore("Taking too long")]
        public void TryToStartConnectionWithInvalidServer()
        {
            var client = new MessagingHubClientBuilder()
                .UsingHostName("invalid.iris.io")
                .UsingGuest()
                .Build();

            Should.ThrowAsync<TimeoutException>(async () => await client.StartAsync().ConfigureAwait(false)).Wait();
        }
    }
}
