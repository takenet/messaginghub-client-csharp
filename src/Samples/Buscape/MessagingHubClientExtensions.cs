using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client;

namespace Buscape
{
    static class IMessagingHubClientExtentions
    {
        public static void StartReceivingMessages(this IMessagingHubClient receiver, CancellationTokenSource cts, Action<Message> callback)
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () =>
            {
                while (!cts.IsCancellationRequested)
                {
                    var message = await receiver.ReceiveMessageAsync(cts.Token);
                    callback?.Invoke(message);
                }
            }, cts.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }
    }
}
