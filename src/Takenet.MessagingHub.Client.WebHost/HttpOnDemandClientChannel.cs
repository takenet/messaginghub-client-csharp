using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Client;
using Lime.Protocol.Network;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks.Dataflow;

namespace Takenet.MessagingHub.Client.WebHost
{
    internal class HttpOnDemandClientChannel : IOnDemandClientChannel
    {
        private readonly IEnvelopeBuffer _envelopeBuffer;
        private readonly string _baseUrl;
        private readonly HttpClient _client;

        public HttpOnDemandClientChannel(IEnvelopeBuffer envelopeBuffer)
        {
            _baseUrl = ConfigurationManager.AppSettings["MessagingHub.BaseUrl"];
            _client = new HttpClient();
           _envelopeBuffer = envelopeBuffer;
        }

        public ICollection<Func<ChannelInformation, Task>> ChannelCreatedHandlers
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ICollection<Func<FailedChannelInformation, Task<bool>>> ChannelCreationFailedHandlers
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ICollection<Func<ChannelInformation, Task>> ChannelDiscardedHandlers
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ICollection<Func<FailedChannelInformation, Task<bool>>> ChannelOperationFailedHandlers
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsEstablished { get; private set; }

        public Task EstablishAsync(CancellationToken cancellationToken)
        {
            IsEstablished = true;
            return Task.CompletedTask;
        }

        public Task FinishAsync(CancellationToken cancellationToken)
        {
            IsEstablished = false;
            return Task.CompletedTask;
        }

        public async Task<Command> ProcessCommandAsync(Command requestCommand, CancellationToken cancellationToken)
        {
            var response = await _client.PostAsJsonAsync($"{_baseUrl}/commands", requestCommand, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsAsync<Command>(cancellationToken);
        }

        public Task<Command> ReceiveCommandAsync(CancellationToken cancellationToken)
        {
            return _envelopeBuffer.Commands.ReceiveAsync(cancellationToken);
        }

        public Task<Message> ReceiveMessageAsync(CancellationToken cancellationToken)
        {
            return _envelopeBuffer.Messages.ReceiveAsync(cancellationToken);
        }

        public Task<Notification> ReceiveNotificationAsync(CancellationToken cancellationToken)
        {
            return _envelopeBuffer.Notifications.ReceiveAsync(cancellationToken);
        }

        public Task SendCommandAsync(Command command, CancellationToken cancellationToken)
        {
            return _client.PostAsJsonAsync($"{_baseUrl}/commands", command, cancellationToken);
        }

        public Task SendMessageAsync(Message message, CancellationToken cancellationToken)
        {
            return _client.PostAsJsonAsync($"{_baseUrl}/messages", message, cancellationToken);
        }

        public Task SendNotificationAsync(Notification notification, CancellationToken cancellationToken)
        {
            return _client.PostAsJsonAsync($"{_baseUrl}/notifications", notification, cancellationToken);
        }
    }
}