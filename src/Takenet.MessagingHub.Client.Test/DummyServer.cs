using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Lime.Protocol;
using Lime.Protocol.Listeners;
using Lime.Protocol.Security;
using Lime.Protocol.Serialization.Newtonsoft;
using Lime.Protocol.Server;
using Lime.Protocol.Util;
using Lime.Transport.Tcp;
using Takenet.MessagingHub.Client.Deprecated;

namespace Takenet.MessagingHub.Client.Test
{
    public sealed class DummyServer : IDisposable, IStartable, IStoppable
    {
        private readonly CancellationTokenSource _cts;
        private readonly TcpTransportListener _transportListener;

        private static readonly SemaphoreSlim ListenerSemaphore = new SemaphoreSlim(1, 1);

        public DummyServer()
            : this(new Uri("net.tcp://localhost:55321"))
        {
            
        }

        public DummyServer(Uri listenerUri)
        {
            ListenerUri = listenerUri;
            _cts = new CancellationTokenSource();
            _transportListener = new TcpTransportListener(ListenerUri, null, new JsonNetSerializer());

        }

        public Uri ListenerUri { get; }

        public async Task StartAsync()
        {
            await ListenerSemaphore.WaitAsync();

            await _transportListener.StartAsync();
#pragma warning disable 4014
            ProducerConsumer.CreateAsync(

                c => _transportListener.AcceptTransportAsync(c),
                async t =>
                {
                    await t.OpenAsync(null, _cts.Token);

                    var serverChannel = new ServerChannel(
                        Guid.NewGuid(),
                        new Node("postmaster", "msging.net", "instance"),
                        t,
                        TimeSpan.FromSeconds(60),
                        autoReplyPings: true);

                    var clientNode = new Node("client", "msging.net", "instance");

                    await serverChannel.EstablishSessionAsync(
                        new[] { SessionCompression.None },
                        new[] { SessionEncryption.None },
                        new[]
                        {
                            AuthenticationScheme.Guest,
                            AuthenticationScheme.Key,
                            AuthenticationScheme.Plain,
                            AuthenticationScheme.Transport,
                        },
                        (n, a) => new AuthenticationResult(null, clientNode), _cts.Token);

                    var channelListener = new ChannelListener(
                        m => TaskUtil.TrueCompletedTask,
                        n => TaskUtil.TrueCompletedTask,
                        async c =>
                        {
                            if (c.Status == CommandStatus.Pending)
                            {
                                await serverChannel.SendCommandAsync(
                                    new Command(c.Id)
                                    {
                                        Status = CommandStatus.Success,
                                        Method = c.Method
                                    },
                                    _cts.Token);
                            }
                            return true;
                        });

                    channelListener.Start(serverChannel);                    

                    return true;
                },
                _cts.Token);
        }

        public async Task StopAsync()
        {
            _cts?.Cancel();
            await (_transportListener?.StopAsync() ?? Task.CompletedTask);

            ListenerSemaphore.Release();
        }

        public void Dispose()
        {
            _cts.Dispose();
            _transportListener?.DisposeIfDisposable();
        }
    }
}
