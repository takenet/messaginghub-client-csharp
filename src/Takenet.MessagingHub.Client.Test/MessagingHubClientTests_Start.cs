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
        [Ignore("Requires real server connection")]
        public async Task StartSuccessfully()
        {
            var connection = new MessagingHubConnectionBuilder()
                .UsingGuest()
                .Build();

            await connection.ConnectAsync().ConfigureAwait(false);
        }

        [Test]
        [Ignore("Taking too long")]
        public void TryToStartConnectionWithInvalidServer()
        {
            var connection = new MessagingHubConnectionBuilder()
                .UsingHostName("invalid.iris.io")
                .UsingGuest()
                .Build();

            Should.ThrowAsync<TimeoutException>(async () => await connection.ConnectAsync().ConfigureAwait(false)).Wait();
        }
    }
}
