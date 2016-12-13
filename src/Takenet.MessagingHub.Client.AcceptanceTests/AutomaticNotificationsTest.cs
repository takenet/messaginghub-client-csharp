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

namespace Takenet.MessagingHub.Client.AcceptanceTests
{
    [Ignore("Blip endpoint is not working")]
    [TestFixture]
    internal class AutomaticNotificationsTest
    {
        [Test]
        public async Task TestAcceptedNotificationIsSentAfterMessageIsReceived()
        {
            var notifications = new Queue<Notification>();
            var tuple1 = await GetClientForApplicationAsync((m, c) => Task.CompletedTask, (n, c) =>
            {
                notifications.Enqueue(n);
                return Task.CompletedTask;
            });
            var sender = tuple1.Item1;
            var appShortName1 = tuple1.Item2;
            var tuple2 = await GetClientForApplicationAsync((m, c) => Task.CompletedTask, (n, c) => Task.CompletedTask);
            var receiver = tuple2.Item1;
            var appShortName2 = tuple2.Item2;

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
            var tuple1 = await GetClientForApplicationAsync((m, c) => Task.CompletedTask, (n, c) =>
            {
                notifications.Enqueue(n);
                return Task.CompletedTask;
            });
            var sender = tuple1.Item1;
            var appShortName1 = tuple1.Item2;
            var tuple2 = await GetClientForApplicationAsync((m, c) => Task.CompletedTask, (n, c) => Task.CompletedTask);
            var receiver = tuple2.Item1;
            var appShortName2 = tuple2.Item2;

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
            var tuple1 = await GetClientForApplicationAsync((m, c) => Task.CompletedTask, (n, c) =>
            {
                notifications.Enqueue(n);
                return Task.CompletedTask;
            });
            var sender = tuple1.Item1;
            var appShortName1 = tuple1.Item2;
            var tuple2 = await GetClientForApplicationAsync((m, c) => Task.CompletedTask, (n, c) => Task.CompletedTask);
            var receiver = tuple2.Item1;
            var appShortName2 = tuple2.Item2;

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
            var tuple1 = await GetClientForApplicationAsync((m, c) => Task.CompletedTask, (n, c) =>
            {
                notifications.Enqueue(n);
                return Task.CompletedTask;
            });
            var sender = tuple1.Item1;
            var appShortName1 = tuple1.Item2;
            var tuple2 = await GetClientForApplicationAsync((m, c) =>
            {
                throw new Exception();
            }, (n, c) => Task.CompletedTask, (m, c) => Task.CompletedTask);
            var receiver = tuple2.Item1;
            var appShortName2 = tuple2.Item2;

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

                notifications.Count.ShouldBe(0); // No other notification should arrive

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
            var tuple1 = await GetClientForApplicationAsync((m, c) => Task.CompletedTask, (n, c) =>
            {
                notifications.Enqueue(n);
                return Task.CompletedTask;
            });
            var sender = tuple1.Item1;
            var appShortName1 = tuple1.Item2;
            var tuple2 = await GetClientForApplicationAsync((m, c) => Task.CompletedTask, (n, c) => Task.CompletedTask);
            var receiver = tuple2.Item1;
            var appShortName2 = tuple2.Item2;

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


        [Test]
        public async Task TestOnlyFailedNotificationIsSentWhenNoReceiverIsRegistered()
        {
            var notifications = new Queue<Notification>();
            var tuple1 = await GetClientForApplicationAsync((m, c) => Task.CompletedTask, (n, c) =>
            {
                notifications.Enqueue(n);
                return Task.CompletedTask;
            });
            var sender = tuple1.Item1;
            var appShortName1 = tuple1.Item2;
            var tuple2 = await GetClientForApplicationAsync(null, (n, c) => Task.CompletedTask);
            var receiver = tuple2.Item1;
            var appShortName2 = tuple2.Item2;

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

                notifications.Count.ShouldBe(0); // No other notification should arrive

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
        public async Task TestOnlyFailedNotificationIsSentWhenMultipleReceiversAreRegistered()
        {
            var notifications = new Queue<Notification>();
            var tuple1 = await GetClientForApplicationAsync((m, c) => Task.CompletedTask, (n, c) =>
            {
                notifications.Enqueue(n);
                return Task.CompletedTask;
            });
            var sender = tuple1.Item1;
            var appShortName1 = tuple1.Item2;
            var tuple2 = await GetClientForApplicationAsync((m, c) =>
            {
                throw new Exception();
            }, (n, c) => Task.CompletedTask, (m, c) => Task.CompletedTask);
            var receiver = tuple2.Item1;
            var appShortName2 = tuple2.Item2;

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

                notifications.Count.ShouldBe(0); // No other notification should arrive

                notification.ShouldNotBeNull();
                notification.Event.ShouldBe(Event.Failed);
            }
            finally
            {
                await sender.StopAsync(CancellationToken.None);
                await receiver.StopAsync(CancellationToken.None);
            }
        }

        private const string Beat = "Beat";

        private static async Task<Tuple<IMessagingHubClient, string>> GetClientForApplicationAsync(Func<Message, CancellationToken, Task> onMessageReceived, Func<Notification, CancellationToken, Task> onNotificationReceived, Func<Message, CancellationToken, Task> secondOnMessageReceived = null)
        {
            var appShortName = await CreateAndRegisterApplicationAsync();
            var appAccessKey = await GetApplicationAccessKeyAsync(appShortName);

            var client = new MessagingHubClientBuilder()
                .UsingHostName("hmg.msging.net")
                .UsingAccessKey(appShortName, appAccessKey)
                .WithMaxConnectionRetries(1)
                .WithSendTimeout(Timeout)
                .Build();

            if (onMessageReceived != null)
                client.AddMessageReceiver(onMessageReceived);
            if (secondOnMessageReceived != null)
                client.AddMessageReceiver(secondOnMessageReceived);

            client.AddNotificationReceiver(onNotificationReceived);

            return new Tuple<IMessagingHubClient, string>(client, appShortName);
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
            var uri = $"http://hmg.api.blip.ai/applications/{appShortName}";
            var response = await HttpClient.GetAsync(uri);
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            dynamic application = JsonConvert.DeserializeObject(content);
            return application.accessKey;
        }

        private static async Task<string> CreateAndRegisterApplicationAsync()
        {
            var uri = "http://hmg.api.blip.ai/applications/";
            var application = CreateApplication();
            var json = JsonConvert.SerializeObject(application);
            using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
            {
                var response = await HttpClient.PostAsync(uri, content);
                response.StatusCode.ShouldBe(HttpStatusCode.OK);
            }
            return application.shortName;
        }

        private static Application CreateApplication()
        {
            var id = "takeqaapp" + DateTime.UtcNow.Ticks;
            return new Application
            {
                shortName = id,
                name = id
            };
        }
    }

    class Application
    {
        public string shortName;
        public string name;
    }
}
