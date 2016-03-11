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
using Takenet.Textc;

namespace Takenet.MessagingHub.Client.Textc.Test
{
    [TestFixture]
    public class TextcMessageReceiverFactoryTest_Create
    {
        [TearDown]
        public void TearDown()
        {
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
                }
            };

            // Act
            var actual = await Bootstrapper.StartAsync(application);

            // Assert
            actual.ShouldNotBeNull();
            var messagingHubClient = actual.ShouldBeOfType<MessagingHubClient>();
            messagingHubClient.Started.ShouldBe(true);
            TestCommandProcessor.Instantiated.ShouldBeTrue();
            TestCommandProcessor.InstanceCount.ShouldBe(2);
        }


        [Test]
        public async Task Create_With_Json_Single_Multiple_Syntaxes_Should_Create_Processor()
        {
            // Arrange
            var json =
                "{ \"login\": \"abcd1234\", \"accessKey\": \"xyz1234\", \"messageReceivers\": [ { \"type\": \"TextcMessageReceiverFactory\", \"settings\": { \"syntaxes\": [ { \"[:Word(mais,more,top) top:Integer? query+:Text]\": { \"processor\": \"TestCommandProcessor\", \"method\": \"GetImageDocumentAsync\" } }, { \"[query+:Text]\": { \"processor\": \"TestCommandProcessor\", \"method\": \"GetFirstImageDocumentAsync\" } } ], \"scorer\": \"MatchCountExpressionScorer\" } } ], \"settings\": { \"myApiKey\": \"askjakdaksjasjdalksjd\" } } ";

            var application = Application.ParseFromJson(json);

            // Act
            var actual = await Bootstrapper.StartAsync(application);

            // Assert
            actual.ShouldNotBeNull();
            var messagingHubClient = actual.ShouldBeOfType<MessagingHubClient>();
            messagingHubClient.Started.ShouldBe(true);
            TestCommandProcessor.Instantiated.ShouldBeTrue();
            TestCommandProcessor.InstanceCount.ShouldBe(2);
        }

    }

    public class TestCommandProcessor
    {
        public static IDictionary<string, object> Settings;
        public static bool Instantiated;
        public static int InstanceCount;

        public TestCommandProcessor(IDictionary<string, object> settings)
        {
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
}
