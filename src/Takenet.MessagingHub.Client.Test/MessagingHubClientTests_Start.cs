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
        public void TestSetUp()
        {
            base.Setup();
            if (DateTime.Today.DayOfWeek == DayOfWeek.Saturday ||
                DateTime.Today.DayOfWeek == DayOfWeek.Sunday ||
                DateTime.Now.Hour < 6 ||
                DateTime.Now.Hour > 19)
            {
                Assert.Ignore("As this test uses hmg server, it cannot be run out of worktime!");
            }
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
                .WithMaxConnectionRetries(1)
                .UsingHostName("invalid.iris.io")
                .UsingGuest()
                .WithSendTimeout(TimeSpan.FromSeconds(2))
                .Build();

            Should.ThrowAsync<TimeoutException>(async () => await connection.ConnectAsync().ConfigureAwait(false)).Wait();
        }
    }
}
