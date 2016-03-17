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
        public async Task StartSuccessfully()
        {
            var client = new MessagingHubClientBuilder()
                .UsingAccount("probe", Encoding.UTF8.GetString(Convert.FromBase64String("S21WMzRDUGxqR1ByemN4")))
                .Build();

            await client.StartAsync().ConfigureAwait(false);
        }

        [Test]
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
