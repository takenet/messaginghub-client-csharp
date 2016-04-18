using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Resources;
using Lime.Protocol;
using Newtonsoft.Json.Linq;
using Takenet.MessagingHub.Client;

namespace Switcher
{
    public class Startup : IStartable, IStoppable
    {
        private readonly IMessagingHubSender _sender;
        private readonly CancellationTokenSource _cts;
        private readonly TimeSpan _messageInterval;
        private readonly string[] _messages;
        private int _nextMessageIndex;
        private Task _sendScheduledMessagesTask;

        public static HashSet<Identity> Destinations = new HashSet<Identity>();

        public Startup(IMessagingHubSender sender, IDictionary<string, object> settings)
        {
            _sender = sender;
            _messageInterval = TimeSpan.Parse((string)settings["messageInterval"]);
            _messages = ((IEnumerable)settings["messages"]).Cast<JValue>().Select(j => j.Value.ToString()).ToArray();
            _cts = new CancellationTokenSource();
        }

        public async Task StartAsync()
        {
            var delegationResult = await _sender.SendCommandAsync(
                new Command()
                {
                    Uri = new LimeUri("/delegations"),
                    Method = CommandMethod.Set,
                    Resource = new Delegation()
                    {
                        Target = Node.Parse("postmaster@cs.msging.net")
                    }
                });

            _sendScheduledMessagesTask = SendScheduledMessagesAsync();
        }

        private async Task SendScheduledMessagesAsync()
        {
            try
            {
                while (!_cts.IsCancellationRequested)
                {
                    await Task.Delay(_messageInterval, _cts.Token);
                    var messageText = _messages[_nextMessageIndex];

                    foreach (var destination in Destinations)
                    {
                        var switcherDestination = new Node()
                        {
                            Name = Uri.EscapeDataString(destination.ToString()),
                            Domain = "cs.msging.net"
                        };

                        await _sender.SendMessageAsync(messageText, switcherDestination);
                    }

                    _nextMessageIndex = (_nextMessageIndex + 1) % _messages.Length;
                }
            }
            catch (OperationCanceledException) when (_cts.IsCancellationRequested) { }
        }

        public async Task StopAsync()
        {
            _cts.Cancel();
            await _sendScheduledMessagesTask;
        }
    }
}
