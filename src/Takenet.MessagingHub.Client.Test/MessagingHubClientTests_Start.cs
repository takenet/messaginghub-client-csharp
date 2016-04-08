using System;
using NUnit.Framework;
using System.Threading.Tasks;
using Shouldly;
using Takenet.MessagingHub.Client.Connection;

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
            var connection = new MessagingHubConnectionBuilder()
                .UsingHostName("hmg.msging.net")
                .UsingGuest()
                .Build();

            await connection.ConnectAsync().ConfigureAwait(false);
        }

        [Test]
        public void TryToStartConnectionWithInvalidServer()
        {
            var connection = new MessagingHubConnectionBuilder()
                .WithMaxConnectionRetries(0)
                .UsingHostName("invalid.iris.io")
                .UsingGuest()
                .Build();

            Should.ThrowAsync<TimeoutException>(async () => await connection.ConnectAsync().ConfigureAwait(false)).Wait();
        }
    }
}
