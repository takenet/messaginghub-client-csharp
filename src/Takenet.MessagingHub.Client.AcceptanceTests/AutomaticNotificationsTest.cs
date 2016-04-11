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
using Takenet.MessagingHub.Client.Connection;
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
            var connection1 = GetConnectionForApplication(ref appShortName1);
            var connection2 = GetConnectionForApplication(ref appShortName2);
            var sender = GetSenderForApplication(connection1);
            var listener1 = GetListenerForApplication(connection1, (m, c) => { }, (n, c) => notifications.Enqueue(n));
            var listener2 = GetListenerForApplication(connection2, (m, c) => { }, (n, c) => { });
            try
            {
                await listener1.StartAsync();
                await listener2.StartAsync();

                await sender.SendMessageAsync(Beat, appShortName2, CancellationToken.None);

                await Task.Delay(Timeout, CancellationToken.None);

                var notification = notifications.Dequeue(); //Accepted

                notification.ShouldNotBeNull();
                notification.Event.ShouldBe(Event.Accepted);
            }
            finally
            {
                await listener2.StopAsync();
                await listener1.StopAsync();
                await connection2.DisconnectAsync();
                await connection1.DisconnectAsync();
            }
        }

        [Test]
        public async Task TestDispatchedNotificationIsSentAfterMessageIsReceived()
        {
            var notifications = new Queue<Notification>();
            string appShortName1 = null, appShortName2 = null;

            var connection1 = GetConnectionForApplication(ref appShortName1);
            var connection2 = GetConnectionForApplication(ref appShortName2);
            var sender = GetSenderForApplication(connection1);
            var listener1 = GetListenerForApplication(connection1, (m, c) => { }, (n, c) => notifications.Enqueue(n));
            var listener2 = GetListenerForApplication(connection2, (m, c) => { }, (n, c) => { });
            try
            {
                await listener1.StartAsync();
                await listener2.StartAsync();

                await sender.SendMessageAsync(Beat, appShortName2, CancellationToken.None);

                await Task.Delay(Timeout, CancellationToken.None);

                notifications.Dequeue(); //Accepted
                var notification = notifications.Dequeue(); //Dispatched

                notification.ShouldNotBeNull();
                notification.Event.ShouldBe(Event.Dispatched);
            }
            finally
            {
                await listener2.StopAsync();
                await listener1.StopAsync();
                await connection2.DisconnectAsync();
                await connection1.DisconnectAsync();
            }
        }

        [Test]
        public async Task TestReceivedNotificationIsSentAfterMessageIsReceived()
        {
            var notifications = new Queue<Notification>();
            string appShortName1 = null, appShortName2 = null;
            var connection1 = GetConnectionForApplication(ref appShortName1);
            var connection2 = GetConnectionForApplication(ref appShortName2);
            var sender = GetSenderForApplication(connection1);
            var listener1 = GetListenerForApplication(connection1, (m, c) => { }, (n, c) => notifications.Enqueue(n));
            var listener2 = GetListenerForApplication(connection2, (m, c) => { }, (n, c) => { });
            try
            {
                await listener1.StartAsync();
                await listener2.StartAsync();

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
                await listener2.StopAsync();
                await listener1.StopAsync();
                await connection2.DisconnectAsync();
                await connection1.DisconnectAsync();
            }
        }

        [Test]
        public async Task TestFailedNotificationIsSentAfterMessageIsReceived()
        {
            var notifications = new Queue<Notification>();
            string appShortName1 = null, appShortName2 = null;
            var connection1 = GetConnectionForApplication(ref appShortName1);
            var connection2 = GetConnectionForApplication(ref appShortName2);
            var sender = GetSenderForApplication(connection1);
            var listener1 = GetListenerForApplication(connection1, (m, c) => { }, (n, c) => notifications.Enqueue(n));
            var listener2 = GetListenerForApplication(connection2, (m, c) => { throw new Exception(); }, (n, c) => { });
            try
            {
                await listener1.StartAsync();
                await listener2.StartAsync();

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
                await listener2.StopAsync();
                await listener1.StopAsync();
                await connection2.DisconnectAsync();
                await connection1.DisconnectAsync();
            }
        }

        [Test]
        public async Task TestConsumedNotificationIsSentAfterMessageIsReceived()
        {
            var notifications = new Queue<Notification>();
            string appShortName1 = null, appShortName2 = null;
            var connection1 = GetConnectionForApplication(ref appShortName1);
            var connection2 = GetConnectionForApplication(ref appShortName2);
            var sender = GetSenderForApplication(connection1);
            var listener1 = GetListenerForApplication(connection1, (m, c) => { }, (n, c) => notifications.Enqueue(n));
            var listener2 = GetListenerForApplication(connection2, (m, c) => { }, (n, c) => { });
            try
            {
                await listener1.StartAsync();
                await listener2.StartAsync();

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
                await listener2.StopAsync();
                await listener1.StopAsync();
                await connection2.DisconnectAsync();
                await connection1.DisconnectAsync();
            }
        }

        private const string Beat = "Beat";

        private static MessagingHubConnection GetConnectionForApplication(ref string appShortName)
        {
            appShortName = appShortName ?? CreateAndRegisterApplicationAsync().Result;
            var appAccessKey = GetApplicationAccessKeyAsync(appShortName).Result;
            var connection = GetConnectionForApplicationAsync(appShortName, appAccessKey).Result;
            return connection;
        }

        private static async Task<MessagingHubConnection> GetConnectionForApplicationAsync(string appShortName, string appAccessKey)
        {
            var connection = new MessagingHubConnectionBuilder()
                .UsingHostName("hmg.msging.net")
                .UsingAccessKey(appShortName, appAccessKey)
                //.WithSendTimeout(Timeout)
                .Build();
            await connection.ConnectAsync();
            return connection;
        }

        private static MessagingHubListener GetListenerForApplication(MessagingHubConnection connection, Action<Message, CancellationToken> onMessageReceived, Action<Notification, CancellationToken> onNotificationReceived)
        {
            var listener = new MessagingHubListener(connection);

            listener.AddMessageReceiver(onMessageReceived);
            listener.AddNotificationReceiver(onNotificationReceived);

            return listener;
        }

        private static IMessagingHubSender GetSenderForApplication(MessagingHubConnection connection)
        {
            var sender = new MessagingHubSender(connection);
            return sender;
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
