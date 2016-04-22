using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Serialization;
using NUnit.Framework;
using Shouldly;
using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.Sender;
using Takenet.MessagingHub.Client.Test;

namespace Takenet.MessagingHub.Client.Host.Test
{
    [TestFixture]
    public class BootstrapperTests
    {
        public DummyServer Server;

        [SetUp]
        public async Task SetUpAsync()
        {
            Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
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
        public void Ensure_Default_Application_Json_Values_Are_Correct()
        {
            // Arrange
            var json = "{}";

            // Act
            var application = Application.ParseFromJson(json);

            // Assert
            application.SessionCompression.ShouldBe(null);
            application.SessionEncryption.ShouldBe(null);
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
                Identifier = "testlogin",
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
                Identifier = "testlogin",
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
                Identifier = "testlogin",
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
                Identifier = "testlogin",
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
            SettingsTestStartable.Settings["setting1"].ShouldBe("value1");
            SettingsTestStartable.Settings["setting2"].ShouldBe(2);
            SettingsTestStartable.Sender.ShouldNotBeNull();
        }

        [Test]        
        public async Task Create_With_StartupFactoryType_And_No_Receiver_Should_Return_Instance()
        {
            // Arrange
            var application = new Application()
            {
                Identifier = "testlogin",
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
            TestStartableFactory.Settings.ShouldNotBeNull();
            TestStartableFactory.Settings.Count.ShouldBe(0);
        }

        [Test]
        public async Task Create_With_StartupFactoryType_And_Setings_And_No_Receiver_Should_Return_Instance()
        {
            // Arrange
            var application = new Application()
            {
                Identifier = "testlogin",
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
            TestStartableFactory.Settings.ShouldNotBeNull();
            TestStartableFactory.Settings["setting1"].ShouldBe("value1");
            TestStartableFactory.Settings["setting2"].ShouldBe(2);
        }

        [Test]
        public async Task Create_With_MessageReceiverType_Should_Return_Instance()
        {
            // Arrange
            var application = new Application()
            {
                Identifier = "testlogin",
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
            var application = new Application
            {
                Identifier = "testlogin",
                AccessKey = "12345".ToBase64(),
                MessageReceivers = new[]
                {
                    new MessageApplicationReceiver
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
                Settings = new Dictionary<string, object>
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
            TestMessageReceiver.Settings.Count.ShouldBe(4);
            TestMessageReceiver.Settings["setting1"].ShouldBe("value1");
            TestMessageReceiver.Settings["setting2"].ShouldBe(2);
            TestMessageReceiver.Settings["setting5"].ShouldBe(5);

            var testMessageReceiverSettings = TestMessageReceiver.Settings[nameof(TestMessageReceiver)] as Settings;
            testMessageReceiverSettings.ShouldNotBeNull();
            testMessageReceiverSettings.Count.ShouldBe(3);
            testMessageReceiverSettings["setting3"].ShouldBe("value3");
            testMessageReceiverSettings["setting4"].ShouldBe(4);
            testMessageReceiverSettings["setting5"].ShouldBe(55);
        }

        [Test]
        public async Task Create_With_NotificationReceiverType_Should_Return_Instance()
        {
            // Arrange
            var application = new Application()
            {
                Identifier = "testlogin",
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

        [Test]
        public async Task Create_With_CustomServiceProvider()
        {
            // Arrange
            var application = new Application()
            {
                Identifier = "testlogin",
                AccessKey = "12345".ToBase64(),
                MessageReceivers = new[]
                {
                    new MessageApplicationReceiver()
                    {
                        Type = typeof(TestMessageReceiverWithCustomParameter).Name,
                        MediaType = "text/plain"
                    }

                },
                HostName = Server.ListenerUri.Host,
                ServiceProviderType = typeof(TestServiceProvider).Name
            };

            // Act
            var actual = await Bootstrapper.StartAsync(application);

            // Assert
            actual.ShouldNotBeNull();
            TestMessageReceiverWithCustomParameter.InstanceCount.ShouldBe(1);
            TestMessageReceiverWithCustomParameter.Dependency.ShouldNotBeNull();
        }
    }

    public class TestMessageReceiverWithCustomParameter : IMessageReceiver
    {
        public static TestCustomType Dependency { get; private set; }
        public static int InstanceCount;

        public TestMessageReceiverWithCustomParameter(TestCustomType dependency)
        {
            Dependency = dependency;
            InstanceCount++;
        }

        public Task ReceiveAsync(Message envelope, IMessagingHubSender sender,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.CompletedTask;
        }
    }

    public class TestCustomType
    {
    }

    public class TestServiceProvider : IServiceProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public TestServiceProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Type SingleInjectedType = typeof(TestCustomType);

        public object GetService(Type serviceType)
        {
            return serviceType == SingleInjectedType ? new TestCustomType() : _serviceProvider.GetService(serviceType);
        }
    }

    public class TestStartable : IStartable
    {
        public static bool _Started;

        public bool Started => _Started;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _Started = true;
            return Task.CompletedTask;
        }
    }

    public class SettingsTestStartable : IStartable
    {
        public static IMessagingHubSender Sender { get; private set; }

        public SettingsTestStartable(IMessagingHubSender sender, Settings settings)
        {
            Sender = sender;
            Settings = settings;
        }

        public bool Started => _Started;

        public static bool _Started;

        public static Settings Settings;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _Started = true;
            return Task.CompletedTask;
        }
    }


    public class TestStartableFactory : IFactory<IStartable>
    {
        public static IServiceProvider ServiceProvider;

        public static Settings Settings;

        public Task<IStartable> CreateAsync(IServiceProvider serviceProvider, IDictionary<string, object> settings)
        {
            ServiceProvider = serviceProvider;
            Settings = (Settings)ServiceProvider.GetService(typeof(Settings));
            return Task.FromResult<IStartable>(new TestStartable());
        }
    }


    public class TestMessageReceiver : IMessageReceiver
    {
        public static int InstanceCount;
        public static Settings Settings;

        public TestMessageReceiver(Settings settings)
        {
            InstanceCount++;
            Settings = settings;
        }

        public Task ReceiveAsync(Message message, IMessagingHubSender sender, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    public class TestNotificationReceiver : INotificationReceiver
    {
        public static int InstanceCount;
        public static Settings Settings;

        public TestNotificationReceiver(Settings settings)
        {
            InstanceCount++;
            Settings = settings;
        }

        public Task ReceiveAsync(Notification envelope, IMessagingHubSender sender, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
