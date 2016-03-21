using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lime.Protocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Shouldly;
using Takenet.MessagingHub.Client.Host;
using Takenet.MessagingHub.Client.Test;
using Takenet.Textc;

namespace Takenet.MessagingHub.Client.Textc.Test
{
    [TestFixture]
    public class TextcMessageReceiverFactoryTest_Create
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
            TestCommandProcessor.Instantiated = false;
            TestCommandProcessor.InstanceCount = 0;
        }

        [Test]
        public async Task Create_With_Single_Syntax_Should_Create_Processor()
        {
            // Arrange
            var application = new Application()
            {
                MessageReceivers = new[]
                {
                    new MessageApplicationReceiver()
                    {
                        Type = typeof(TextcMessageReceiverFactory).Name,
                        Settings = new Dictionary<string, object>
                        {
                            {
                                "syntaxes",
                                new[]
                                {
                                    new Dictionary<string, object>
                                    {
                                        {
                                            "value1:Word value2:Integer",
                                            new JObject()
                                            {
                                                { "processor", typeof(TestCommandProcessor).Name },
                                                { "method", nameof(TestCommandProcessor.ProcessAsync) }

                                            }
                                        },

                                    },
                                    new Dictionary<string, object>
                                    {
                                        {
                                            "value1:Word value2:Integer value3:Word",
                                            new JObject()
                                            {
                                                { "processor", typeof(TestCommandProcessor).AssemblyQualifiedName },
                                                { "method", nameof(TestCommandProcessor.ProcessWithResultAsync) }

                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
                StartupType = nameof(SettingsTestStartable),
                HostName = Server.ListenerUri.Host
            };

            // Act
            var actual = await Bootstrapper.StartAsync(application);

            // Assert
            actual.ShouldNotBeNull();
            SettingsTestStartable.Sender.ShouldNotBeNull();
            TestCommandProcessor.Instantiated.ShouldBeTrue();
            TestCommandProcessor.InstanceCount.ShouldBe(2);            
        }


        [Test]
        public async Task Create_With_Json_Single_Multiple_Syntaxes_Should_Create_Processor()
        {
            // Arrange
            var json =
                "{ \"login\": \"abcd1234\", \"accessKey\": \"xyz1234\", \"messageReceivers\": [ { \"type\": \"TextcMessageReceiverFactory\", \"settings\": { \"syntaxes\": [ { \"[:Word(mais,more,top) top:Integer? query+:Text]\": { \"processor\": \"TestCommandProcessor\", \"method\": \"GetImageDocumentAsync\" } }, { \"[query+:Text]\": { \"processor\": \"TestCommandProcessor\", \"method\": \"GetFirstImageDocumentAsync\" } } ], \"scorer\": \"MatchCountExpressionScorer\" } } ], \"settings\": { \"myApiKey\": \"askjakdaksjasjdalksjd\" }, \"hostName\": \"localhost\",  } ";

            var application = Application.ParseFromJson(json);

            // Act
            var actual = await Bootstrapper.StartAsync(application);

            // Assert
            actual.ShouldNotBeNull();
            TestCommandProcessor.Instantiated.ShouldBeTrue();
            TestCommandProcessor.InstanceCount.ShouldBe(2);
        }

    }

    public class TestCommandProcessor
    {
        public static IMessagingHubSender Sender;
        public static IDictionary<string, object> Settings;
        public static bool Instantiated;
        public static int InstanceCount;

        public TestCommandProcessor(IMessagingHubSender sender, IDictionary<string, object> settings)
        {
            Sender = sender;
            Settings = settings;
            Instantiated = true;
            InstanceCount++;
        }

        public Task ProcessAsync(string value1, int value2, IRequestContext context)
        {
            return Task.CompletedTask;
        }

        public Task<string> ProcessWithResultAsync(string value1, int value2, string value3, IRequestContext context)
        {
            return Task.FromResult("result");
        }

        public async Task<JsonDocument> GetFirstImageDocumentAsync(string query, IRequestContext context)
        {
            return new JsonDocument();
        }

        public async Task<JsonDocument> GetImageDocumentAsync(int? top, string query, IRequestContext context)
        {
            return new JsonDocument();
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

}
