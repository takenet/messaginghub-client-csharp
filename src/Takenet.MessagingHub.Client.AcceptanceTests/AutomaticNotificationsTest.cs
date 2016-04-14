using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Newtonsoft.Json;
using NUnit.Framework;
using Shouldly;
using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client.AcceptanceTests
{
    [TestFixture]
    internal class AutomaticNotificationsTest
    {
        [Test]
        public async Task TestAcceptedNotificationIsSentAfterMessageIsReceived()
        {
            var notifications = new Queue<Notification>();
            string appShortName1 = null, appShortName2 = null;
            var sender = GetClientForApplication(ref appShortName1, (m, s, c) => Task.CompletedTask, (n, s, c) =>
            {
                notifications.Enqueue(n);
                return Task.CompletedTask;
            });
            var receiver = GetClientForApplication(ref appShortName2, (m, s, c) => Task.CompletedTask, (n, s, c) => Task.CompletedTask);

            try
            {
                await sender.StartAsync(CancellationToken.None);
                await receiver.StartAsync(CancellationToken.None);

                await sender.SendMessageAsync(Beat, appShortName2, CancellationToken.None);

                await Task.Delay(Timeout, CancellationToken.None);

                var notification = notifications.Dequeue(); //Accepted

                notification.ShouldNotBeNull();
                notification.Event.ShouldBe(Event.Accepted);
            }
            finally
            {
                await sender.StopAsync(CancellationToken.None);
                await receiver.StopAsync(CancellationToken.None);
            }
        }

        [Test]
        public async Task TestDispatchedNotificationIsSentAfterMessageIsReceived()
        {
            var notifications = new Queue<Notification>();
            string appShortName1 = null, appShortName2 = null;
            var sender = GetClientForApplication(ref appShortName1, (m, s, c) => Task.CompletedTask, (n, s, c) =>
            {
                notifications.Enqueue(n);
                return Task.CompletedTask;
            });
            var receiver = GetClientForApplication(ref appShortName2, (m, s, c) => Task.CompletedTask, (n, s, c) => Task.CompletedTask);

            try
            {
                await sender.StartAsync(CancellationToken.None);
                await receiver.StartAsync(CancellationToken.None);

                await sender.SendMessageAsync(Beat, appShortName2, CancellationToken.None);

                await Task.Delay(Timeout, CancellationToken.None);

                notifications.Dequeue(); //Accepted
                var notification = notifications.Dequeue(); //Dispatched

                notification.ShouldNotBeNull();
                notification.Event.ShouldBe(Event.Dispatched);
            }
            finally
            {
                await sender.StopAsync(CancellationToken.None);
                await receiver.StopAsync(CancellationToken.None);
            }
        }

        [Test]
        public async Task TestReceivedNotificationIsSentAfterMessageIsReceived()
        {
            var notifications = new Queue<Notification>();
            string appShortName1 = null, appShortName2 = null;
            var sender = GetClientForApplication(ref appShortName1, (m, s, c) => Task.CompletedTask, (n, s, c) =>
            {
                notifications.Enqueue(n);
                return Task.CompletedTask;
            });
            var receiver = GetClientForApplication(ref appShortName2, (m, s, c) => Task.CompletedTask, (n, s, c) => Task.CompletedTask);

            try
            {
                await sender.StartAsync(CancellationToken.None);
                await receiver.StartAsync(CancellationToken.None);

                await sender.SendMessageAsync(Beat, appShortName2, CancellationToken.None);

                await Task.Delay(Timeout, CancellationToken.None);

                notifications.Dequeue(); //Accepted
                notifications.Dequeue(); //Dispatched
                var notification = notifications.Dequeue(); //Received

                notification.ShouldNotBeNull();
                notification.Event.ShouldBe(Event.Received);
            }
            finally
            {
                await sender.StopAsync(CancellationToken.None);
                await receiver.StopAsync(CancellationToken.None);
            }
        }

        [Test]
        public async Task TestFailedNotificationIsSentAfterMessageIsReceived()
        {
            var notifications = new Queue<Notification>();
            string appShortName1 = null, appShortName2 = null;
            var sender = GetClientForApplication(ref appShortName1, (m, s, c) => Task.CompletedTask, (n, s, c) =>
            {
                notifications.Enqueue(n);
                return Task.CompletedTask;
            });
            var receiver = GetClientForApplication(ref appShortName2, (m, s, c) =>
            {
                throw new Exception();
            }, (n, s, c) => Task.CompletedTask);

            try
            {
                await sender.StartAsync(CancellationToken.None);
                await receiver.StartAsync(CancellationToken.None);

                await sender.SendMessageAsync(Beat, appShortName2, CancellationToken.None);

                await Task.Delay(Timeout, CancellationToken.None);

                notifications.Dequeue(); //Accepted
                notifications.Dequeue(); //Dispatched
                notifications.Dequeue(); //Received
                var notification = notifications.Dequeue(); //Failed

                notification.ShouldNotBeNull();
                notification.Event.ShouldBe(Event.Failed);
            }
            finally
            {
                await sender.StopAsync(CancellationToken.None);
                await receiver.StopAsync(CancellationToken.None);
            }
        }

        [Test]
        public async Task TestConsumedNotificationIsSentAfterMessageIsReceived()
        {
            var notifications = new Queue<Notification>();
            string appShortName1 = null, appShortName2 = null;
            var sender = GetClientForApplication(ref appShortName1, (m, s, c) => Task.CompletedTask, (n, s, c) =>
            {
                notifications.Enqueue(n);
                return Task.CompletedTask;
            });
            var receiver = GetClientForApplication(ref appShortName2, (m, s, c) => Task.CompletedTask, (n, s, c) => Task.CompletedTask);

            try
            {
                await sender.StartAsync(CancellationToken.None);
                await receiver.StartAsync(CancellationToken.None);

                await sender.SendMessageAsync(Beat, appShortName2, CancellationToken.None);

                await Task.Delay(Timeout, CancellationToken.None);

                notifications.Dequeue(); //Accepted
                notifications.Dequeue(); //Dispatched
                notifications.Dequeue(); //Received
                var notification = notifications.Dequeue(); //Consumed

                notification.ShouldNotBeNull();
                notification.Event.ShouldBe(Event.Consumed);
            }
            finally
            {
                await sender.StopAsync(CancellationToken.None);
                await receiver.StopAsync(CancellationToken.None);
            }
        }

        private const string Beat = "Beat";

        private static IMessagingHubClient GetClientForApplication(ref string appShortName, Func<Message, IMessagingHubSender, CancellationToken, Task> onMessageReceived, Func<Notification, IMessagingHubSender, CancellationToken, Task> onNotificationReceived)
        {
            appShortName = appShortName ?? CreateAndRegisterApplicationAsync().Result;
            var appAccessKey = GetApplicationAccessKeyAsync(appShortName).Result;

            var client = new MessagingHubClientBuilder()
                .UsingHostName("hmg.msging.net")
                .UsingAccessKey(appShortName, appAccessKey)
                .WithMaxConnectionRetries(1)
                .WithSendTimeout(Timeout)
                .Build();

            client.AddMessageReceiver(onMessageReceived);
            client.AddNotificationReceiver(onNotificationReceived);

            return client;
        }

        private static TimeSpan Timeout => TimeSpan.FromSeconds(5);

        private static HttpClient _httpClient;
        private static HttpClient HttpClient
        {
            get
            {
                if (_httpClient == null)
                {
                    _httpClient = new HttpClient();
                    _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "cCZkQHRha2VuZXQuY29tLmJyOlRAazNuM3Q=");
                 //   _httpClient.Timeout = Timeout;
                }
                return _httpClient;
            }
        }

        private static async Task<string> GetApplicationAccessKeyAsync(string appShortName)
        {
            var uri = $"http://hmg.api.messaginghub.io/applications/{appShortName}";
            var response = await HttpClient.GetAsync(uri);
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            dynamic application = JsonConvert.DeserializeObject(content);
            return application.accessKey;
        }

        private static async Task<string> CreateAndRegisterApplicationAsync()
        {
            var uri = "http://hmg.api.messaginghub.io/applications/";
            dynamic application = CreateApplication();
            var json = JsonConvert.SerializeObject(application);
            using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
            {
                var response = await HttpClient.PostAsync(uri, content);
                response.StatusCode.ShouldBe(HttpStatusCode.OK);
            }
            return application.shortName;
        }

        private static object CreateApplication()
        {
            var id = "takeQAApp" + DateTime.UtcNow.Ticks;
            return new
            {
                shortName = id,
                name = id
            };
        }
    }
}
