using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
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
                                "value1:Word value2:Integer",
                                new JObject()
                                {
                                    { "processor", typeof(TestCommandProcessor).Name },
                                    { "method", nameof(TestCommandProcessor.ProcessAsync) }

                                }
                            },
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

            };

            // Act
            var actual = await Bootstrapper.StartAsync(application);

        }
    }

    public class TestCommandProcessor
    {
        public static bool Instantiated;
        public static int InstanceCount;

        public TestCommandProcessor()
        {
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
    }
}
