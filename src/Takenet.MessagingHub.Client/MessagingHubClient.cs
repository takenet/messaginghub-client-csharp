using Lime.Messaging.Resources;
using Lime.Protocol;
using Lime.Protocol.Client;
using Lime.Protocol.Network;
using Lime.Protocol.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client.Lime;
using Takenet.MessagingHub.Client.Receivers;

namespace Takenet.MessagingHub.Client
{
    /// <summary>
    /// Default implementation for <see cref="IMessagingHubClient"/> class.
    /// </summary>
    public class MessagingHubClient : IMessagingHubClient
    {
        public bool Started { get; private set; }

        private readonly Uri _endpoint;
        private string _login;
        private readonly string _domainName;

        private readonly IPersistentLimeSessionFactory _persistentClientFactory;
        private IPersistentLimeSession _persistentLimeSession;

        private ICommandProcessor _commandProcessor;

        private readonly IClientChannelFactory _clientChannelFactory;
        private readonly ICommandProcessorFactory _commandProcessorFactory;

        private readonly TimeSpan _timeout;
        private readonly ILimeSessionProvider _limeSessionProvider;
        private Authentication _authentication;

        internal MessagingHubClient(string login, Authentication authentication, Uri endPoint, string domainName, IPersistentLimeSessionFactory persistentChannelFactory, IClientChannelFactory clientChannelFactory,
            ICommandProcessorFactory commandProcessorFactory, ILimeSessionProvider limeSessionProvider)
        {
            _persistentClientFactory = persistentChannelFactory;
            _clientChannelFactory = clientChannelFactory;
            _commandProcessorFactory = commandProcessorFactory;
            _limeSessionProvider = limeSessionProvider;
            _domainName = domainName;
            _endpoint = endPoint;
            _timeout = TimeSpan.FromSeconds(60);
            _login = login;
            _authentication = authentication;
        }


        public MessagingHubClient(string login, Authentication authentication, Uri endPoint, string domainName) :
            this(login, authentication, endPoint, domainName, new PersistentLimeSessionFactory(), new ClientChannelFactory(), new CommandProcessorFactory(), new LimeSessionProvider())
        { }

        public virtual async Task<Command> SendCommandAsync(Command command)
        {
            if (!Started) throw new InvalidOperationException("Client must be started before to proceed with this operation");

            return await _commandProcessor.SendAsync(command, _timeout).ConfigureAwait(false);
        }

        public virtual async Task SendMessageAsync(Message message)
        {
            if (!Started) throw new InvalidOperationException("Client must be started before to proceed with this operation");

            await _persistentLimeSession.SendMessageAsync(message).ConfigureAwait(false);
        }

        public virtual async Task SendNotificationAsync(Notification notification)
        {
            if (!Started) throw new InvalidOperationException("Client must be started before to proceed with this operation!");

            await _persistentLimeSession.SendNotificationAsync(notification).ConfigureAwait(false);
        }

        public virtual Task<Message> ReceiveMessageAsync(CancellationToken cancellationToken)
        {
            return _persistentLimeSession.ReceiveMessageAsync(cancellationToken);
        }

        public virtual Task<Notification> ReceiveNotificationAsync(CancellationToken cancellationToken)
        {
            return _persistentLimeSession.ReceiveNotificationAsync(cancellationToken);
        }

        public virtual async Task StartAsync()
        {
            await InstantiateClientChannelAsync().ConfigureAwait(false);
            await _persistentLimeSession.StartAsync().ConfigureAwait(false);
            await SetPresenceAsync().ConfigureAwait(false);

            StartEnvelopeProcessors();

            Started = true;
        }

        public virtual async Task StopAsync()
        {
            if (!Started)
                throw new InvalidOperationException(
                    "Client must be started before to proceed with this operation!");

            await _commandProcessor.StopReceivingAsync().ConfigureAwait(false);
            await _persistentLimeSession.StopAsync().ConfigureAwait(false);
            Started = false;
        }

        private void StartEnvelopeProcessors()
        {
            _commandProcessor = _commandProcessorFactory.Create(_persistentLimeSession);
            _commandProcessor.StartReceiving();
        }

        private async Task InstantiateClientChannelAsync()
        {
            var identity = Identity.Parse($"{_login}@{_domainName}");

            _persistentLimeSession = await _persistentClientFactory.CreatePersistentClientChannelAsync(_endpoint, _timeout, identity, _authentication, _clientChannelFactory, _limeSessionProvider);
        }

        //TODO: Presence should not be here. It should set presence on reconnection
        private async Task SetPresenceAsync()
        {
            await _persistentLimeSession.SetResourceAsync(
                LimeUri.Parse(UriTemplates.PRESENCE),
                new Presence { Status = PresenceStatus.Available, RoutingRule = RoutingRule.Identity },
                CancellationToken.None);
        }
    }
}