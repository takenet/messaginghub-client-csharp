using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Security;
using Lime.Protocol.Serialization;
using Lime.Protocol.Serialization.Newtonsoft;
using Lime.Protocol.Server;
using Lime.Protocol.Util;
using Lime.Transport.Tcp;
using NSubstitute.Routing.Handlers;
using NUnit.Core;
using NUnit.Framework;
using Shouldly;
using Takenet.MessagingHub.Client.Host;
using Takenet.MessagingHub.Client.Receivers;
using Event = Lime.Protocol.Event;

namespace Takenet.MessagingHub.Client.Test.Host
{
    [TestFixture]
    public class BootstrapperTests_StartAsync
    {
        public DummyServer Server;

        [SetUp]
        public async Task SetUpAsync()
        {
            Server = new DummyServer();
            await Server.StartAsync();
        }

        [TearDown]
        public async Task TearDownAsync()
        {
            await Server.StopAsync();
            Server.Dispose();
            TestMessageReceiver.InstanceCount = 0;
            TestNotificationReceiver.InstanceCount = 0;
        }        
        
        [Test]        
        public async Task Create_With_No_Credential_And_No_Receiver_Should_Return_Instance()
        {
            // Arrange
            var application = new Application()
            {
                HostName = Server.ListenerUri.Host
            };

            // Act
            var actual = await Bootstrapper.StartAsync(application);

            // Assert
            actual.ShouldNotBeNull();

        }

        [Test]
        public async Task Create_With_Passowrd_And_No_Receiver_Should_Return_Instance()
        {
            // Arrange
            var application = new Application()
            {
                Login = "testlogin",
                Password = "12345".ToBase64(),
                HostName = Server.ListenerUri.Host
            };

            // Act
            var actual = await Bootstrapper.StartAsync(application);

            // Assert
            actual.ShouldNotBeNull();

        }

        [Test]
        public async Task Create_With_AccessKey_And_No_Receiver_Should_Return_Instance()
        {
            // Arrange
            var application = new Application()
            {
                Login = "testlogin",
                AccessKey = "12345".ToBase64(),
                HostName = Server.ListenerUri.Host
            };

            // Act
            var actual = await Bootstrapper.StartAsync(application);
            
            // Assert
            actual.ShouldNotBeNull();
            
        }

        [Test]        
        public async Task Create_With_StartupType_And_No_Receiver_Should_Return_Instance()
        {
            // Arrange
            var application = new Application()
            {
                Login = "testlogin",
                AccessKey = "12345".ToBase64(),
                StartupType = typeof(TestStartable).Name,
                HostName = Server.ListenerUri.Host
            };

            // Act
            var actual = await Bootstrapper.StartAsync(application);

            // Assert
            actual.ShouldNotBeNull();
            TestStartable._Started.ShouldBeTrue();            
        }

        [Test]        
        public async Task Create_With_StartupType_And_Settings_And_No_Receiver_Should_Return_Instance()
        {
            // Arrange
            var application = new Application()
            {
                Login = "testlogin",
                AccessKey = "12345".ToBase64(),
                StartupType = typeof(SettingsTestStartable).Name,
                Settings = new Dictionary<string, object>()
                {
                    { "setting1", "value1" },
                    { "setting2", 2 }
                },
                HostName = Server.ListenerUri.Host
            };

            // Act
            var actual = await Bootstrapper.StartAsync(application);

            // Assert
            actual.ShouldNotBeNull();
            SettingsTestStartable._Started.ShouldBeTrue();
            SettingsTestStartable.Settings.ShouldNotBeNull();
            SettingsTestStartable.Settings.ShouldBe(application.Settings);
            SettingsTestStartable.Sender.ShouldNotBeNull();
        }

        [Test]        
        public async Task Create_With_StartupFactoryType_And_No_Receiver_Should_Return_Instance()
        {
            // Arrange
            var application = new Application()
            {
                Login = "testlogin",
                AccessKey = "12345".ToBase64(),
                StartupType = typeof(TestStartableFactory).AssemblyQualifiedName,
                HostName = Server.ListenerUri.Host
            };

            // Act
            var actual = await Bootstrapper.StartAsync(application);

            // Assert
            actual.ShouldNotBeNull();
            TestStartable._Started.ShouldBeTrue();
            TestStartableFactory.ServiceProvider.ShouldNotBeNull();
            TestStartableFactory.Settings.ShouldBeNull();
        }

        [Test]
        public async Task Create_With_StartupFactoryType_And_Setings_And_No_Receiver_Should_Return_Instance()
        {
            // Arrange
            var application = new Application()
            {
                Login = "testlogin",
                AccessKey = "12345".ToBase64(),
                StartupType = typeof(TestStartableFactory).AssemblyQualifiedName,
                Settings = new Dictionary<string, object>()
                {
                    { "setting1", "value1" },
                    { "setting2", 2 }
                },
                HostName = Server.ListenerUri.Host
            };

            // Act
            var actual = await Bootstrapper.StartAsync(application);

            // Assert
            actual.ShouldNotBeNull();
            TestStartable._Started.ShouldBeTrue();
            TestStartableFactory.ServiceProvider.ShouldNotBeNull();
            TestStartableFactory.Settings.ShouldBe(application.Settings);
        }

        [Test]
        public async Task Create_With_MessageReceiverType_Should_Return_Instance()
        {
            // Arrange
            var application = new Application()
            {
                Login = "testlogin",
                AccessKey = "12345".ToBase64(),
                MessageReceivers = new []
                {
                    new MessageApplicationReceiver()
                    {
                        Type = typeof(TestMessageReceiver).Name,
                        MediaType = "text/plain"
                    },
                    new MessageApplicationReceiver()
                    {
                        Type = typeof(TestMessageReceiver).Name,
                        MediaType = "application/json"
                    },
                    new MessageApplicationReceiver()
                    {
                        Type = typeof(TestMessageReceiver).AssemblyQualifiedName                        
                    }
                },
                HostName = Server.ListenerUri.Host
            };

            // Act
            var actual = await Bootstrapper.StartAsync(application);

            // Assert
            actual.ShouldNotBeNull();
            TestMessageReceiver.InstanceCount.ShouldBe(3);
        }

        [Test]
        public async Task Create_With_MessageReceiverType_With_Settings_Should_Return_Instance()
        {
            // Arrange
            var application = new Application()
            {
                Login = "testlogin",
                AccessKey = "12345".ToBase64(),
                MessageReceivers = new[]
                {
                    new MessageApplicationReceiver()
                    {
                        Type = typeof(TestMessageReceiver).Name,
                        MediaType = "text/plain",
                        Settings = new Dictionary<string, object>()
                        {
                            { "setting3", "value3" },
                            { "setting4", 4 },
                            { "setting5", 55 }
                        }
                    }

                },
                Settings = new Dictionary<string, object>()
                {
                    { "setting1", "value1" },
                    { "setting2", 2 },
                    { "setting5", 5 }
                },
                HostName = Server.ListenerUri.Host
            };

            // Act
            var actual = await Bootstrapper.StartAsync(application);

            // Assert
            actual.ShouldNotBeNull();
            TestMessageReceiver.InstanceCount.ShouldBe(1);
            TestMessageReceiver.Settings.Keys.Count.ShouldBe(5);
            TestMessageReceiver.Settings["setting1"].ShouldBe("value1");
            TestMessageReceiver.Settings["setting2"].ShouldBe(2);
            TestMessageReceiver.Settings["setting3"].ShouldBe("value3");
            TestMessageReceiver.Settings["setting4"].ShouldBe(4);
            TestMessageReceiver.Settings["setting5"].ShouldBe(55);
        }

        [Test]
        public async Task Create_With_NotificationReceiverType_Should_Return_Instance()
        {
            // Arrange
            var application = new Application()
            {
                Login = "testlogin",
                AccessKey = "12345".ToBase64(),
                NotificationReceivers = new[]
                {
                    new NotificationApplicationReceiver()
                    {
                        Type = typeof(TestNotificationReceiver).AssemblyQualifiedName,
                        EventType = Event.Accepted
                    },
                    new NotificationApplicationReceiver()
                    {
                        Type = typeof(TestNotificationReceiver).AssemblyQualifiedName,
                        EventType = Event.Dispatched
                    },
                    new NotificationApplicationReceiver()
                    {
                        Type = typeof(TestNotificationReceiver).AssemblyQualifiedName
                    }
                },
                HostName = Server.ListenerUri.Host
            };

            // Act
            var actual = await Bootstrapper.StartAsync(application);

            // Assert
            actual.ShouldNotBeNull();
            TestNotificationReceiver.InstanceCount.ShouldBe(3);
        }
    }

    public class TestStartable : IStartable
    {
        public static bool _Started;

        public bool Started => _Started;

        public Task StartAsync()
        {
            _Started = true;
            return Task.CompletedTask;
        }
    }

    public class SettingsTestStartable : IStartable
    {        
        public SettingsTestStartable(IMessagingHubSender sender, IDictionary<string, object> settings)
        {
            Sender = sender;
            Settings = settings;
        }

        public bool Started => _Started;

        public static bool _Started;

        public static IMessagingHubSender Sender;

        public static IDictionary<string, object> Settings;

        public Task StartAsync()
        {
            _Started = true;
            return Task.CompletedTask;
        }
    }


    public class TestStartableFactory : IFactory<IStartable>
    {
        public static IServiceProvider ServiceProvider;

        public static IDictionary<string, object> Settings;

        public Task<IStartable> CreateAsync(IServiceProvider serviceProvider, IDictionary<string, object> settings)
        {
            ServiceProvider = serviceProvider;
            Settings = settings;
            return Task.FromResult<IStartable>(new TestStartable());
        }
    }


    public class TestMessageReceiver : IMessageReceiver
    {
        public static int InstanceCount;
        public static IDictionary<string, object> Settings;

        public TestMessageReceiver(IDictionary<string, object> settings)
        {
            InstanceCount++;
            Settings = settings;
        }

        public Task ReceiveAsync(Message envelope)
        {
            throw new NotImplementedException();
        }
    }

    public class TestNotificationReceiver : INotificationReceiver
    {
        public static int InstanceCount;
        public static IDictionary<string, object> Settings;

        public TestNotificationReceiver(IDictionary<string, object> settings)
        {
            InstanceCount++;
            Settings = settings;
        }

        public Task ReceiveAsync(Notification envelope)
        {
            throw new NotImplementedException();
        }
    }
}
