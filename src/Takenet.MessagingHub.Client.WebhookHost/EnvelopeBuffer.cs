﻿using System.Threading.Tasks.Dataflow;
using Lime.Protocol;

namespace Takenet.MessagingHub.Client.WebhookHost
{
    public class EnvelopeBuffer : IEnvelopeBuffer
    {
        public EnvelopeBuffer()
        {
            var options = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded
            };
            Commands = new BufferBlock<Command>(options);
            Messages = new BufferBlock<Message>(options);
            Notifications = new BufferBlock<Notification>(options);
        }

        public BufferBlock<Command> Commands { get; }

        public BufferBlock<Message> Messages { get; }

        public BufferBlock<Notification> Notifications { get; }
    }
}