using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Newtonsoft.Json;
using Takenet.MessagingHub.Client.Extensions;
using Takenet.MessagingHub.Client.Extensions.Bucket;
using Takenet.MessagingHub.Client.Extensions.Session;
using Takenet.MessagingHub.Client.Extensions.Tunnel;
using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client.Host
{
    public static class Bootstrapper
    {
        public const string DefaultApplicationFileName = "application.json";

        /// <summary>
        /// Creates ans starts an application with the given settings.
        /// </summary>
        /// <param name="application">The application instance. If not defined, the class will look for an application.json file in the current directory.</param>
        /// <param name="loadAssembliesFromWorkingDirectory">if set to <c>true</c> indicates to the bootstrapper to load all assemblies from the current working directory.</param>
        /// <param name="path">Assembly path to load</param>
        /// <param name="builder">The builder instance to be used.</param>
        /// <param name="typeResolver">The type provider.</param>
        /// <returns></returns>
        /// <exception cref="System.IO.FileNotFoundException">Could not find the '{DefaultApplicationFileName}'</exception>
        /// <exception cref="System.ArgumentException">At least an access key or password must be defined</exception>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="ArgumentException">At least an access key or password must be defined</exception>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="ArgumentException">At least an access key or password must be defined</exception>
        public static async Task<IStoppable> StartAsync(
            Application application = null, 
            bool loadAssembliesFromWorkingDirectory = true, 
            string path = ".", 
            MessagingHubClientBuilder builder = null, 
            ITypeResolver typeResolver = null)
        {
            if (application == null)
            {
                if (!File.Exists(DefaultApplicationFileName)) throw new FileNotFoundException($"Could not find the '{DefaultApplicationFileName}' file", DefaultApplicationFileName);
                application = Application.ParseFromJsonFile(DefaultApplicationFileName);
            }

            if (loadAssembliesFromWorkingDirectory)
            {
                ReferencesUtil.LoadAssembliesAndReferences(path, assemblyFilter: ReferencesUtil.IgnoreSystemAndMicrosoftAssembliesFilter, ignoreExceptionLoadingReferencedAssembly: true);
            }

            if (builder == null) builder = new MessagingHubClientBuilder();
            if (application.Identifier != null)
            {
                if (application.Password != null)
                {
                    builder = builder.UsingPassword(application.Identifier, application.Password);
                }
                else if (application.AccessKey != null)
                {
                    builder = builder.UsingAccessKey(application.Identifier, application.AccessKey);
                }
                else
                {
                    throw new ArgumentException("At least an access key or password must be defined", nameof(application));
                }
            }
            else
            {
                builder = builder.UsingGuest();
            }

            if (application.Instance != null) builder = builder.UsingInstance(application.Instance);
            if (application.RoutingRule != null) builder = builder.UsingRoutingRule(application.RoutingRule.Value);
            if (application.Domain != null) builder = builder.UsingDomain(application.Domain);
            if (application.Scheme != null) builder = builder.UsingScheme(application.Scheme);
            if (application.HostName != null) builder = builder.UsingHostName(application.HostName);
            if (application.Port != null) builder = builder.UsingPort(application.Port.Value);
            if (application.SendTimeout != 0) builder = builder.WithSendTimeout(TimeSpan.FromMilliseconds(application.SendTimeout));
            if (application.SessionEncryption.HasValue) builder = builder.UsingEncryption(application.SessionEncryption.Value);
            if (application.SessionCompression.HasValue) builder = builder.UsingCompression(application.SessionCompression.Value);
            if (application.Throughput != 0) builder = builder.WithThroughput(application.Throughput);
            if (application.ChannelBuffer != 0) builder = builder.WithChannelBuffer(application.ChannelBuffer);
            if (application.DisableNotify) builder = builder.WithAutoNotify(false);
            if (application.ChannelCount.HasValue) builder = builder.WithChannelCount(application.ChannelCount.Value);
            if (application.ReceiptEvents != null && application.ReceiptEvents.Any())
                builder = builder.WithReceiptEvents(application.ReceiptEvents);
            else if(application.ReceiptEvents != null)
                builder = builder.WithReceiptEvents(new[] { Event.Failed });

            if (typeResolver == null) typeResolver = TypeResolver.Instance;

            var localServiceProvider = BuildServiceProvider(application, typeResolver);

            localServiceProvider.RegisterService(typeof(MessagingHubClientBuilder), builder);

            var client = await BuildMessagingHubClientAsync(application, builder.Build, localServiceProvider, typeResolver);

            await client.StartAsync().ConfigureAwait(false);

            var stoppables = new IStoppable[2];
            stoppables[0] = client;
            var startable = await BuildStartupAsync(application, localServiceProvider, typeResolver);
            if (startable != null)
            {
                stoppables[1] = startable as IStoppable;
            }

            return new StoppableWrapper(stoppables);
        }

        public static async Task<IStartable> BuildStartupAsync(Application application, IServiceContainer localServiceProvider, ITypeResolver typeResolver)
        {
            if (application.StartupType == null) return null;

            var startable = await CreateAsync<IStartable>(
                application.StartupType,
                localServiceProvider,
                application.Settings,
                typeResolver)
                .ConfigureAwait(false);
            await startable.StartAsync().ConfigureAwait(false);
            return startable;
        }

        public static IServiceContainer BuildServiceProvider(Application application, ITypeResolver typeResolver)
        {
            var localServiceProvider = new TypeServiceProvider();
            if (application.ServiceProviderType != null)
            {
                var serviceProviderType = typeResolver.Resolve(application.ServiceProviderType);
                if (serviceProviderType != null)
                {
                    if (!typeof(IServiceProvider).IsAssignableFrom(serviceProviderType))
                    {
                        throw new InvalidOperationException($"{application.ServiceProviderType} must be an implementation of '{nameof(IServiceProvider)}'");
                    }

                    if (serviceProviderType == typeof(TypeServiceProvider))
                    {
                        throw new InvalidOperationException($"{nameof(Application.ServiceProviderType)} type cannot be '{serviceProviderType.Name}'");
                    }

                    if (serviceProviderType.GetConstructors(BindingFlags.Instance | BindingFlags.Public).All(c => c.GetParameters().Length != 0))
                    {
                        throw new InvalidOperationException($"{nameof(Application.ServiceProviderType)} must have an empty public constructor");
                    }

                    localServiceProvider.SecondaryServiceProvider = (IServiceProvider)Activator.CreateInstance(serviceProviderType);
                }
            }

            if (localServiceProvider.SecondaryServiceProvider is IServiceContainer serviceContainer)
            {
                return serviceContainer;
            }

            return localServiceProvider;
        }

        public static void RegisterSettingsContainer(SettingsContainer settingsContainer, IServiceContainer serviceContainer, ITypeResolver typeResolver)
        {
            if (settingsContainer.SettingsType != null)
            {
                var settingsDictionary = settingsContainer.Settings;
                var settingsType = typeResolver.Resolve(settingsContainer.SettingsType);
                if (settingsType != null)
                {
                    var settingsJson = JsonConvert.SerializeObject(settingsDictionary, Application.SerializerSettings);
                    var settings = JsonConvert.DeserializeObject(settingsJson, settingsType, Application.SerializerSettings);
                    serviceContainer.RegisterService(settingsType, settings);
                }
            }
        }

        public static async Task<IMessagingHubClient> BuildMessagingHubClientAsync(
            Application application,
            Func<IMessagingHubClient> builder,
            IServiceContainer serviceContainer,
            ITypeResolver typeResolver,
            Action<IServiceContainer> serviceOverrides = null)
        {
            RegisterSettingsContainer(application, serviceContainer, typeResolver);
            RegisterSettingsTypes(application, serviceContainer, typeResolver);
            RegisterStateManager(application, serviceContainer, typeResolver);

            serviceContainer.RegisterExtensions();
            serviceContainer.RegisterService(typeof(IServiceProvider), serviceContainer);
            serviceContainer.RegisterService(typeof(IServiceContainer), serviceContainer);
            serviceContainer.RegisterService(typeof(Application), application);

            var client = builder();
            serviceContainer.RegisterService(typeof(IMessagingHubSender), client);
            serviceOverrides?.Invoke(serviceContainer);

            var stateManager = serviceContainer.GetService<IStateManager>();
            var sessionManager = serviceContainer.GetService<ISessionManager>();

            if (application.RegisterTunnelReceivers)
            {
                RegisterTunnelReceivers(application);
            }

            await AddMessageReceivers(application, serviceContainer, client, typeResolver, stateManager, sessionManager);
            await AddNotificationReceivers(application, serviceContainer, client, typeResolver, stateManager, sessionManager);
            await AddCommandReceivers(application, serviceContainer, client, typeResolver);

            return client;
        }

        public static void RegisterSettingsTypes(Application application, IServiceContainer serviceContainer, ITypeResolver typeResolver)
        {
            var applicationReceivers =
                (application.MessageReceivers ?? new ApplicationReceiver[0]).Union(
                    application.NotificationReceivers ?? new ApplicationReceiver[0]);

            // First, register the receivers settings
            foreach (var applicationReceiver in applicationReceivers.Where(a => a.SettingsType != null))
            {
                RegisterSettingsContainer(applicationReceiver, serviceContainer, typeResolver);
            }
        }

        public static void RegisterStateManager(Application application, IServiceContainer serviceContainer, ITypeResolver typeResolver)
        {
            if (string.IsNullOrWhiteSpace(application.StateManagerType))
            {
                serviceContainer.RegisterService(typeof(IStateManager), () => new BucketStateManager(serviceContainer.GetService<IBucketExtension>()));
            }
            else
            {
                var stateManagerType = typeResolver.Resolve(application.StateManagerType);
                serviceContainer.RegisterService(typeof(IStateManager), () => serviceContainer.GetService(stateManagerType));
            }
        }

        private static void RegisterTunnelReceivers(Application application)
        {
            // Message
            var messageReceivers = new List<MessageApplicationReceiver>();
            if (application.MessageReceivers != null
                && application.MessageReceivers.Length > 0)
            {
                messageReceivers.AddRange(application.MessageReceivers);
            }
            messageReceivers.Add(
                new MessageApplicationReceiver
                {
                    Sender = $"(.+)@{TunnelExtension.TunnelAddress.Domain.Replace(".", "\\.")}\\/(.+)",
                    Type = nameof(TunnelMessageReceiver)
                });

            application.MessageReceivers = messageReceivers.ToArray();

            // Notification
            var notificationReceivers = new List<NotificationApplicationReceiver>();
            if (application.NotificationReceivers != null
                && application.NotificationReceivers.Length > 0)
            {
                notificationReceivers.AddRange(application.NotificationReceivers);
            }
            notificationReceivers.Add(
                new NotificationApplicationReceiver
                {
                    Sender = $"(.+)@{TunnelExtension.TunnelAddress.Domain.Replace(".", "\\.")}\\/(.+)",
                    Type = nameof(TunnelNotificationReceiver)
                });

            application.NotificationReceivers = notificationReceivers.ToArray();
        }

        private static async Task AddNotificationReceivers(
            Application application, 
            IServiceContainer serviceContainer, 
            IMessagingHubClient client, 
            ITypeResolver typeResolver,
            IStateManager stateManager,
            ISessionManager sessionManager)
        {
            if (application.NotificationReceivers != null && application.NotificationReceivers.Length > 0)
            {
                foreach (var applicationReceiver in application.NotificationReceivers)
                {
                    INotificationReceiver receiver;
                    if (applicationReceiver.Response?.MediaType != null)
                    {
                        var content = applicationReceiver.Response.ToDocument();
                        receiver =
                            new LambdaNotificationReceiver(
                                (notification, c) => client.SendMessageAsync(content, notification.From, c));
                    }
                    else if (!string.IsNullOrWhiteSpace(applicationReceiver.ForwardTo))
                    {
                        var tunnelExtension = serviceContainer.GetService<ITunnelExtension>();
                        var destination = Identity.Parse(applicationReceiver.ForwardTo);
                        receiver =
                            new LambdaNotificationReceiver(
                                (notification, c) => tunnelExtension.ForwardNotificationAsync(notification, destination, c));
                    }
                    else
                    {
                        receiver = await CreateAsync<INotificationReceiver>(
                            applicationReceiver.Type, serviceContainer, applicationReceiver.Settings, typeResolver)
                            .ConfigureAwait(false);
                    }

                    if (applicationReceiver.OutState != null)
                    {
                        receiver = new SetStateNotificationReceiver(receiver, stateManager, applicationReceiver.OutState);
                    }

                    Func<Notification, Task<bool>> notificationPredicate = BuildPredicate<Notification>(applicationReceiver, stateManager, sessionManager);

                    if (applicationReceiver.EventType != null)
                    {
                        var currentNotificationPredicate = notificationPredicate;
                        notificationPredicate = async (n) => await currentNotificationPredicate(n) && n.Event.Equals(applicationReceiver.EventType);
                    }

                    client.AddNotificationReceiver(receiver, notificationPredicate, applicationReceiver.Priority);
                }
            }
        }

        public static async Task AddMessageReceivers(
            Application application,
            IServiceContainer serviceContainer,
            IMessagingHubClient client,
            ITypeResolver typeResolver,
            IStateManager stateManager,
            ISessionManager sessionManager)
        {
            if (application.MessageReceivers != null && application.MessageReceivers.Length > 0)
            {
                foreach (var applicationReceiver in application.MessageReceivers)
                {
                    IMessageReceiver receiver;
                    if (applicationReceiver.Response?.MediaType != null)
                    {
                        var content = applicationReceiver.Response.ToDocument();
                        receiver =
                            new LambdaMessageReceiver(
                                (message, c) => client.SendMessageAsync(content, message.From, c));
                    }
                    else if (!string.IsNullOrWhiteSpace(applicationReceiver.ForwardTo))
                    {
                        var tunnelExtension = serviceContainer.GetService<ITunnelExtension>();
                        var destination = Identity.Parse(applicationReceiver.ForwardTo);
                        receiver =
                            new LambdaMessageReceiver(
                                (message, c) => tunnelExtension.ForwardMessageAsync(message, destination, c));
                    }
                    else
                    {
                        receiver = await CreateAsync<IMessageReceiver>(
                            applicationReceiver.Type, serviceContainer, applicationReceiver.Settings, typeResolver)
                            .ConfigureAwait(false);
                    }

                    if (applicationReceiver.OutState != null)
                    {
                        receiver = new SetStateMessageReceiver(receiver, stateManager, applicationReceiver.OutState);
                    }

                    Func<Message, Task<bool>> messagePredicate = BuildPredicate<Message>(applicationReceiver, stateManager, sessionManager);

                    if (applicationReceiver.MediaType != null)
                    {
                        var currentMessagePredicate = messagePredicate;
                        var mediaTypeRegex = new Regex(applicationReceiver.MediaType, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                        messagePredicate = async (m) => await currentMessagePredicate(m) && mediaTypeRegex.IsMatch(m.Type.ToString());
                    }

                    if (applicationReceiver.Content != null)
                    {
                        var currentMessagePredicate = messagePredicate;
                        var contentRegex = new Regex(applicationReceiver.Content, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                        messagePredicate = async (m) => await currentMessagePredicate(m) && contentRegex.IsMatch(m.Content.ToString());
                    }

                    client.AddMessageReceiver(receiver, messagePredicate, applicationReceiver.Priority);
                }
            }
        }

        private static async Task AddCommandReceivers(
            Application application, 
            IServiceContainer serviceContainer, 
            IMessagingHubClient client, 
            ITypeResolver typeResolver)
        {
            if (application.CommandReceivers == null || application.CommandReceivers.Length == 0)
            {
                return;
            }

            foreach (var commandReceiver in application.CommandReceivers)
            {
                var receiver = await CreateAsync<ICommandReceiver>(
                           commandReceiver.Type, serviceContainer, commandReceiver.Settings, typeResolver)
                           .ConfigureAwait(false);

                Func<Command, Task<bool>> predicate = c => Task.FromResult(true);

                if (commandReceiver.Method.HasValue)
                {
                    var currentPredicate = predicate;
                    predicate = async (c) => await currentPredicate(c) && c.Method == commandReceiver.Method.Value;
                }

                if (commandReceiver.Uri != null)
                {
                    var limeUriRegex = new Regex(commandReceiver.Uri, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    var currentPredicate = predicate;
                    predicate = async (c) => await currentPredicate(c) && limeUriRegex.IsMatch(c.Uri.ToString());
                }

                if (commandReceiver.ResourceUri != null)
                {
                    var resourceUriRegex = new Regex(commandReceiver.ResourceUri, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    var currentPredicate = predicate;
                    predicate = async (c) => await currentPredicate(c) && resourceUriRegex.IsMatch(c.GetResourceUri().ToString());
                }

                if (commandReceiver.MediaType != null)
                {
                    var currentPredicate = predicate;
                    var mediaTypeRegex = new Regex(commandReceiver.MediaType, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    predicate = async (c) => await currentPredicate(c) && mediaTypeRegex.IsMatch(c.Type.ToString());
                }

                client.AddCommandReceiver(receiver, predicate, commandReceiver.Priority);
            }
        }

        private static Func<T, Task<bool>> BuildPredicate<T>(
            ApplicationReceiver applicationReceiver,
            IStateManager stateManager,
            ISessionManager sessionManager) where T : Envelope, new()
        {
            Func<T, Task<bool>> predicate = m => Task.FromResult(m != null);

            if (applicationReceiver.Sender != null)
            {
                var currentPredicate = predicate;
                var senderRegex = new Regex(applicationReceiver.Sender, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                predicate = async (m) => await currentPredicate(m) && senderRegex.IsMatch(m.From.ToString());
            }

            if (applicationReceiver.Destination != null)
            {
                var currentPredicate = predicate;
                var destinationRegex = new Regex(applicationReceiver.Destination, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                predicate = async (m) => await currentPredicate(m) && destinationRegex.IsMatch(m.To.ToString());
            }

            if (applicationReceiver.State != null)
            {
                var currentPredicate = predicate;
                predicate = async (m) => await currentPredicate(m)
                    && (await stateManager.GetStateAsync(m.From.ToIdentity())).Equals(applicationReceiver.State, StringComparison.OrdinalIgnoreCase);
            }

            if (applicationReceiver.Culture != null)
            {
                var currentPredicate = predicate;
                predicate = async (m) => await currentPredicate(m)
                    && (await sessionManager.GetCultureAsync(m.From, CancellationToken.None) ?? "").Equals(applicationReceiver.Culture, StringComparison.OrdinalIgnoreCase);
            }

            return predicate;
        }

        public static Task<T> CreateAsync<T>(string typeName, IServiceProvider serviceProvider, IDictionary<string, object> settings, ITypeResolver typeResolver) where T : class
        {
            if (typeName == null) throw new ArgumentNullException(nameof(typeName));
            var type = typeResolver.Resolve(typeName);
            return CreateAsync<T>(type, serviceProvider, settings);
        }

        public static async Task<T> CreateAsync<T>(Type type, IServiceProvider serviceProvider, IDictionary<string, object> settings) where T : class
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            IFactory<T> factory;
            if (typeof(IFactory<T>).IsAssignableFrom(type))
            {
                factory = (IFactory<T>)Activator.CreateInstance(type);
            }
            else
            {
                factory = new Factory<T>(type);
            }

            var instance = await factory.CreateAsync(serviceProvider, settings);
            if (instance == null)
            {
                throw new Exception($"{type.Name} does not implement {typeof(T).Name}");
            }
            return instance;
        }

        private class StoppableWrapper : IStoppable
        {
            private readonly IStoppable[] _stoppables;

            public StoppableWrapper(params IStoppable[] stoppables)
            {
                _stoppables = stoppables;
            }

            public async Task StopAsync(CancellationToken cancellationToken)
            {
                foreach (var stoppable in _stoppables)
                {
                    if (stoppable != null)
                        await stoppable.StopAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private class SetStateMessageReceiver : SetStateEnvelopeReceiver<Message>, IMessageReceiver
        {
            public SetStateMessageReceiver(IEnvelopeReceiver<Message> receiver, IStateManager stateManager, string state) 
                : base(receiver, stateManager, state)
            {
            }
        }

        private class SetStateNotificationReceiver : SetStateEnvelopeReceiver<Notification>, INotificationReceiver
        {
            public SetStateNotificationReceiver(IEnvelopeReceiver<Notification> receiver, IStateManager stateManager, string state) 
                : base(receiver, stateManager, state)
            {
            }
        }

        private class SetStateEnvelopeReceiver<T> : IEnvelopeReceiver<T> where T : Envelope
        {
            private readonly IEnvelopeReceiver<T> _receiver;
            private readonly string _state;
            private readonly IStateManager _stateManager;

            protected SetStateEnvelopeReceiver(IEnvelopeReceiver<T> receiver, IStateManager stateManager, string state)
            {
                if (state == null) throw new ArgumentNullException(nameof(state));
                _receiver = receiver;
                _state = state;
                _stateManager = stateManager;
            }

            public async Task ReceiveAsync(T envelope, CancellationToken cancellationToken = new CancellationToken())
            {
                await _receiver.ReceiveAsync(envelope, cancellationToken).ConfigureAwait(false);
                await _stateManager.SetStateAsync(envelope.From.ToIdentity(), _state);
            }
        }
    }
}
