using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Serialization;
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
        [Test]
        public async Task Create_With_No_Credential_And_No_Receiver_Should_Return_Instance()
        {
            // Arrange
            var application = new Application()
            {
                
            };

            // Act
            var actual = await Bootstrapper.StartAsync(application);

            // Assert
            actual.ShouldNotBeNull();
            var messagingHubClient = actual.ShouldBeOfType<MessagingHubClient>();
            messagingHubClient.Started.ShouldBe(true);

        }

        [Test]
        public async Task Create_With_Passowrd_And_No_Receiver_Should_Return_Instance()
        {
            // Arrange
            var application = new Application()
            {
                Login = "testlogin",
                Password = "12345".ToBase64()
            };

            // Act
            var actual = await Bootstrapper.StartAsync(application);

            // Assert
            actual.ShouldNotBeNull();
            var messagingHubClient = actual.ShouldBeOfType<MessagingHubClient>();
            messagingHubClient.Started.ShouldBe(true);

        }

        [Test]
        public async Task Create_With_AccessKey_And_No_Receiver_Should_Return_Instance()
        {
            // Arrange
            var application = new Application()
            {
                Login = "testlogin",
                AccessKey = "12345".ToBase64()
            };

            // Act
            var actual = await Bootstrapper.StartAsync(application);
            
            // Assert
            actual.ShouldNotBeNull();
            var messagingHubClient = actual.ShouldBeOfType<MessagingHubClient>();
            messagingHubClient.Started.ShouldBe(true);
            
        }

        [Test]
        public async Task Create_With_StartupType_And_No_Receiver_Should_Return_Instance()
        {
            // Arrange
            var application = new Application()
            {
                Login = "testlogin",
                AccessKey = "12345".ToBase64(),
                StartupType = typeof(TestStartable).AssemblyQualifiedName
            };

            // Act
            var actual = await Bootstrapper.StartAsync(application);

            // Assert
            actual.ShouldNotBeNull();
            var messagingHubClient = actual.ShouldBeOfType<MessagingHubClient>();
            messagingHubClient.Started.ShouldBe(true);
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
                StartupType = typeof(SettingsTestStartable).AssemblyQualifiedName,
                Settings = new Dictionary<string, object>()
                {
                    { "setting1", "value1" },
                    { "setting2", 2 }
                }
            };

            // Act
            var actual = await Bootstrapper.StartAsync(application);

            // Assert
            actual.ShouldNotBeNull();
            var messagingHubClient = actual.ShouldBeOfType<MessagingHubClient>();
            messagingHubClient.Started.ShouldBe(true);
            TestStartable._Started.ShouldBeTrue();
            SettingsTestStartable.Settings.ShouldNotBeNull();
            SettingsTestStartable.Settings.ShouldBe(application.Settings);
        }

        [Test]
        public async Task Create_With_StartupFactoryType_And_No_Receiver_Should_Return_Instance()
        {
            // Arrange
            var application = new Application()
            {
                Login = "testlogin",
                AccessKey = "12345".ToBase64(),
                StartupType = typeof(TestStartableFactory).AssemblyQualifiedName
            };

            // Act
            var actual = await Bootstrapper.StartAsync(application);

            // Assert
            actual.ShouldNotBeNull();
            var messagingHubClient = actual.ShouldBeOfType<MessagingHubClient>();
            messagingHubClient.Started.ShouldBe(true);
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
                }
            };

            // Act
            var actual = await Bootstrapper.StartAsync(application);

            // Assert
            actual.ShouldNotBeNull();
            var messagingHubClient = actual.ShouldBeOfType<MessagingHubClient>();
            messagingHubClient.Started.ShouldBe(true);
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
                    new ApplicationMessageReceiver()
                    {
                        Type = typeof(TestMessageReceiver).AssemblyQualifiedName,
                        MediaType = "text/plain"
                    },
                    new ApplicationMessageReceiver()
                    {
                        Type = typeof(TestMessageReceiver).AssemblyQualifiedName,
                        MediaType = "application/json"
                    },
                    new ApplicationMessageReceiver()
                    {
                        Type = typeof(TestMessageReceiver).AssemblyQualifiedName                        
                    }

                }
            };

            // Act
            var actual = await Bootstrapper.StartAsync(application);

            // Assert
            actual.ShouldNotBeNull();
            var messagingHubClient = actual.ShouldBeOfType<MessagingHubClient>();
            messagingHubClient.Started.ShouldBe(true);
            TestMessageReceiver.InstanceCount.ShouldBe(3);
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
                    new ApplicationNotificationReceiver()
                    {
                        Type = typeof(TestNotificationReceiver).AssemblyQualifiedName,
                        EventType = Event.Accepted
                    },
                    new ApplicationNotificationReceiver()
                    {
                        Type = typeof(TestNotificationReceiver).AssemblyQualifiedName,
                        EventType = Event.Dispatched
                    },
                    new ApplicationNotificationReceiver()
                    {
                        Type = typeof(TestNotificationReceiver).AssemblyQualifiedName
                    }
                }
            };

            // Act
            var actual = await Bootstrapper.StartAsync(application);

            // Assert
            actual.ShouldNotBeNull();
            var messagingHubClient = actual.ShouldBeOfType<MessagingHubClient>();
            messagingHubClient.Started.ShouldBe(true);
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

    public class SettingsTestStartable : TestStartable
    {
        public static IDictionary<string, object> Settings;

        public SettingsTestStartable(IDictionary<string, object> settings)
        {
            Settings = settings;
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

        public TestMessageReceiver()
        {
            InstanceCount++;
        }

        public Task ReceiveAsync(Message envelope)
        {
            throw new NotImplementedException();
        }
    }

    public class TestNotificationReceiver : INotificationReceiver
    {
        public static int InstanceCount;

        public TestNotificationReceiver()
        {
            InstanceCount++;
        }

        public Task ReceiveAsync(Notification envelope)
        {
            throw new NotImplementedException();
        }
    }
}
