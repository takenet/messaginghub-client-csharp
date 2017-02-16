using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Serialization;
using Newtonsoft.Json;
using Takenet.MessagingHub.Client.Extensions;
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

            serviceContainer.RegisterService(typeof(IServiceProvider), serviceContainer);
            serviceContainer.RegisterService(typeof(IServiceContainer), serviceContainer);
            serviceContainer.RegisterService(typeof(Application), application);
            serviceContainer.RegisterService(typeof(IStateManager), StateManager.Instance);
            serviceContainer.RegisterExtensions();

            var client = builder();
            serviceContainer.RegisterService(typeof(IMessagingHubSender), client);
            serviceOverrides?.Invoke(serviceContainer);

            // Now creates the receivers instances
            await AddMessageReceivers(application, serviceContainer, client, typeResolver);
            await AddNotificationReceivers(application, serviceContainer, client, typeResolver);
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

        private static async Task AddNotificationReceivers(Application application, IServiceContainer serviceContainer, IMessagingHubClient client, ITypeResolver typeResolver)
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
                    else
                    {
                        receiver = await CreateAsync<INotificationReceiver>(
                            applicationReceiver.Type, serviceContainer, applicationReceiver.Settings, typeResolver)
                            .ConfigureAwait(false);
                    }

                    Predicate<Notification> notificationPredicate = n => n != null;

                    if (applicationReceiver.EventType != null)
                    {
                        var currentNotificationPredicate = notificationPredicate;
                        notificationPredicate = n => currentNotificationPredicate(n) && n.Event.Equals(applicationReceiver.EventType);
                    }

                    if (applicationReceiver.Sender != null)
                    {
                        var currentNotificationPredicate = notificationPredicate;
                        var senderRegex = new Regex(applicationReceiver.Sender, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                        notificationPredicate = n => currentNotificationPredicate(n) && senderRegex.IsMatch(n.From.ToString());
                    }

                    if (applicationReceiver.Destination != null)
                    {
                        var currentNotificationPredicate = notificationPredicate;
                        var destinationRegex = new Regex(applicationReceiver.Destination, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                        notificationPredicate = n => currentNotificationPredicate(n) && destinationRegex.IsMatch(n.To.ToString());
                    }

                    if (applicationReceiver.State != null)
                    {
                        var currentNotificationPredicate = notificationPredicate;
                        notificationPredicate = n => currentNotificationPredicate(n) && StateManager.Instance.GetState(n.From.ToIdentity()).Equals(applicationReceiver.State, StringComparison.OrdinalIgnoreCase);
                    }

                    if (applicationReceiver.OutState != null)
                    {
                        receiver = new SetStateNotificationReceiver(receiver, applicationReceiver.OutState);
                    }

                    client.AddNotificationReceiver(receiver, notificationPredicate, applicationReceiver.Priority);
                }
            }
        }

        public static async Task AddMessageReceivers(Application application, IServiceContainer serviceContainer, IMessagingHubClient client, ITypeResolver typeResolver)
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
                    else
                    {
                        receiver = await CreateAsync<IMessageReceiver>(
                            applicationReceiver.Type, serviceContainer, applicationReceiver.Settings, typeResolver)
                            .ConfigureAwait(false);
                    }

                    Predicate<Message> messagePredicate = m => m != null;

                    if (applicationReceiver.MediaType != null)
                    {
                        var currentMessagePredicate = messagePredicate;
                        var mediaTypeRegex = new Regex(applicationReceiver.MediaType, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                        messagePredicate = m => currentMessagePredicate(m) && mediaTypeRegex.IsMatch(m.Type.ToString());
                    }

                    if (applicationReceiver.Content != null)
                    {
                        var currentMessagePredicate = messagePredicate;
                        var contentRegex = new Regex(applicationReceiver.Content, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                        messagePredicate = m => currentMessagePredicate(m) && contentRegex.IsMatch(m.Content.ToString());
                    }

                    if (applicationReceiver.Sender != null)
                    {
                        var currentMessagePredicate = messagePredicate;
                        var senderRegex = new Regex(applicationReceiver.Sender, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                        messagePredicate = m => currentMessagePredicate(m) && senderRegex.IsMatch(m.From.ToString());
                    }

                    if (applicationReceiver.Destination != null)
                    {
                        var currentMessagePredicate = messagePredicate;
                        var destinationRegex = new Regex(applicationReceiver.Destination, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                        messagePredicate = m => currentMessagePredicate(m) && destinationRegex.IsMatch(m.To.ToString());
                    }

                    if (applicationReceiver.State != null)
                    {
                        var currentMessagePredicate = messagePredicate;
                        messagePredicate = m => currentMessagePredicate(m) && StateManager.Instance.GetState(m.From.ToIdentity()).Equals(applicationReceiver.State, StringComparison.OrdinalIgnoreCase);
                    }

                    if (applicationReceiver.OutState != null)
                    {
                        receiver = new SetStateMessageReceiver(receiver, applicationReceiver.OutState);
                    }

                    client.AddMessageReceiver(receiver, messagePredicate, applicationReceiver.Priority);
                }
            }
        }

        private static async Task AddCommandReceivers(Application application, IServiceContainer serviceContainer, IMessagingHubClient client, ITypeResolver typeResolver)
        {
            if(application.CommandReceivers == null || application.CommandReceivers.Length == 0)
            {
                return;
            }

            foreach (var commandReceiver in application.CommandReceivers)
            {
                var receiver = await CreateAsync<ICommandReceiver>(
                           commandReceiver.Type, serviceContainer, commandReceiver.Settings, typeResolver)
                           .ConfigureAwait(false);

                Predicate<Command> predicate = c => true;

                if (commandReceiver.Method.HasValue)
                {
                    var currentPredicate = predicate;
                    predicate = c => currentPredicate(c) && c.Method == commandReceiver.Method.Value;
                }

                if (commandReceiver.Uri != null)
                {
                    var limeUriRegex = new Regex(commandReceiver.Uri, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    var currentPredicate = predicate;
                    predicate = c => currentPredicate(c) && limeUriRegex.IsMatch(c.Uri.ToString());
                }

                if (commandReceiver.ResourceUri != null)
                {
                    var resourceUriRegex = new Regex(commandReceiver.ResourceUri, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    var currentPredicate = predicate;
                    predicate = c => currentPredicate(c) && resourceUriRegex.IsMatch(c.GetResourceUri().ToString());
                }

                if (commandReceiver.MediaType != null)
                {
                    var currentPredicate = predicate;
                    var mediaTypeRegex = new Regex(commandReceiver.MediaType, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    predicate = c => currentPredicate(c) && mediaTypeRegex.IsMatch(c.Type.ToString());
                }

                client.AddCommandReceiver(receiver, predicate, commandReceiver.Priority);
            }
        }

        public static Task<T> CreateAsync<T>(string typeName, IServiceProvider serviceProvider, IDictionary<string, object> settings, ITypeResolver typeResolver) where T : class
        {
            if (typeName == null) throw new ArgumentNullException(nameof(typeName));
            var type = typeResolver.Resolve(typeName);
            return CreateAsync<T>(type, serviceProvider, settings);
        }

        public static Task<T> CreateAsync<T>(Type type, IServiceProvider serviceProvider, IDictionary<string, object> settings) where T : class
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

            return factory.CreateAsync(serviceProvider, settings);
        }

        private class StoppableWrapper : IStoppable
        {
            private readonly IStoppable[] _stoppables;

            public StoppableWrapper(params IStoppable[] stoppables)
            {
                _stoppables = stoppables;
            }

            public async Task StopAsync(CancellationToken cancellationTokenn)
            {
                foreach (var stoppable in _stoppables)
                {
                    if (stoppable != null)
                        await stoppable.StopAsync(cancellationTokenn).ConfigureAwait(false);
                }
            }
        }

        private class SetStateMessageReceiver : SetStateEnvelopeReceiver<Message>, IMessageReceiver
        {
            public SetStateMessageReceiver(IEnvelopeReceiver<Message> receiver, string state) : base(receiver, state)
            {
            }
        }

        private class SetStateNotificationReceiver : SetStateEnvelopeReceiver<Notification>, INotificationReceiver
        {
            public SetStateNotificationReceiver(IEnvelopeReceiver<Notification> receiver, string state) : base(receiver, state)
            {
            }
        }

        private class SetStateEnvelopeReceiver<T> : IEnvelopeReceiver<T> where T : Envelope
        {
            private readonly IEnvelopeReceiver<T> _receiver;
            private readonly string _state;

            protected SetStateEnvelopeReceiver(IEnvelopeReceiver<T> receiver, string state)
            {
                if (state == null) throw new ArgumentNullException(nameof(state));
                _receiver = receiver;
                _state = state;
            }

            public async Task ReceiveAsync(T envelope, CancellationToken cancellationToken = new CancellationToken())
            {
                await _receiver.ReceiveAsync(envelope, cancellationToken).ConfigureAwait(false);
                StateManager.Instance.SetState(envelope.From.ToIdentity(), _state);
            }
        }
    }
}
