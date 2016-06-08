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
using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.Sender;
using static Takenet.MessagingHub.Client.Host.ApplicationActivator;


namespace Takenet.MessagingHub.Client.Host
{
    public static class Bootstrapper
    {
        /// <summary>
        /// Creates ans starts an application with the given settings.
        /// </summary>
        /// <param name="application">The application instance. If not defined, the class will look for an application.json file in the current directory.</param>
        /// <param name="loadAssembliesFromWorkingDirectory">if set to <c>true</c> indicates to the bootstrapper to load all assemblies from the current working directory.</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="ArgumentException">At least an access key or password must be defined</exception>
        /// <exception cref="FileNotFoundException">Could not find the 'application.json' file</exception>
        /// <exception cref="ArgumentException">At least an access key or password must be defined</exception>
        public static async Task<IStoppable> StartAsync(Application application = null, bool loadAssembliesFromWorkingDirectory = true)
        {
            if (application == null)
            {
                if (!File.Exists(DefaultApplicationFileName)) throw new FileNotFoundException($"Could not find the '{DefaultApplicationFileName}' file", DefaultApplicationFileName);
                application = Application.ParseFromJsonFile(DefaultApplicationFileName);
            }

            if (loadAssembliesFromWorkingDirectory)
            {
                TypeUtil.LoadAssembliesAndReferences(".", assemblyFilter: TypeUtil.IgnoreSystemAndMicrosoftAssembliesFilter, ignoreExceptionLoadingReferencedAssembly: true);
            }

            var builder = new MessagingHubClientBuilder();
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

            if (application.Domain != null) builder = builder.UsingDomain(application.Domain);
            if (application.HostName != null) builder = builder.UsingHostName(application.HostName);
            if (application.SendTimeout != 0) builder = builder.WithSendTimeout(TimeSpan.FromMilliseconds(application.SendTimeout));
            if (application.SessionEncryption.HasValue) builder = builder.UsingEncryption(application.SessionEncryption.Value);
            if (application.SessionCompression.HasValue) builder = builder.UsingCompression(application.SessionCompression.Value);

            var localServiceProvider = new TypeServiceProvider();
            if (application.ServiceProviderType != null)
            {
                var serviceProviderType = ParseTypeName(application.ServiceProviderType);
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

            localServiceProvider.RegisterService(typeof(IServiceProvider), localServiceProvider);
            localServiceProvider.RegisterService(typeof(IServiceContainer), localServiceProvider);
            localServiceProvider.RegisterService(typeof(MessagingHubClientBuilder), builder);
            localServiceProvider.RegisterService(typeof(Application), application);
            RegisterSettingsContainer(application, localServiceProvider);

            var client = await BuildMessagingHubClientAsync(application, builder, localServiceProvider);

            await client.StartAsync().ConfigureAwait(false);

            var stoppables = new IStoppable[2];
            stoppables[0] = client;
            if (application.StartupType != null)
            {
                var startable = await CreateAsync<IStartable>(
                    application.StartupType,
                    localServiceProvider,
                    application.Settings)
                    .ConfigureAwait(false);
                await startable.StartAsync().ConfigureAwait(false);
                stoppables[1] = startable as IStoppable;
            }
            return new StoppableWrapper(stoppables);
        }

        private static void RegisterSettingsContainer(SettingsContainer settingsContainer, IServiceContainer serviceContainer)
        {
            if (settingsContainer.SettingsType != null)
            {
                var settingsDictionary = settingsContainer.Settings;
                var settingsType = ParseTypeName(settingsContainer.SettingsType);
                if (settingsType != null)
                {
                    var settingsJson = JsonConvert.SerializeObject(settingsDictionary, Application.SerializerSettings);
                    var settings = JsonConvert.DeserializeObject(settingsJson, settingsType, Application.SerializerSettings);
                    serviceContainer.RegisterService(settingsType, settings);
                }
            }
        }

        private static async Task<IMessagingHubClient> BuildMessagingHubClientAsync(
            Application application, MessagingHubClientBuilder builder, 
            TypeServiceProvider typeServiceProvider)
        {            
            var applicationReceivers =
                (application.MessageReceivers ?? new ApplicationReceiver[0]).Union(
                    application.NotificationReceivers ?? new ApplicationReceiver[0]);
            
            // First, register the receivers settings
            foreach (var applicationReceiver in applicationReceivers.Where(a => a.SettingsType != null))
            {
                RegisterSettingsContainer(applicationReceiver, typeServiceProvider);
            }

            var client = builder.Build();
            typeServiceProvider.RegisterService(typeof(IMessagingHubSender), client);

            // Now creates the receivers instances
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
                            applicationReceiver.Type, typeServiceProvider, applicationReceiver.Settings)
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

                    client.AddMessageReceiver(receiver, messagePredicate);
                }
            }

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
                            applicationReceiver.Type, typeServiceProvider, applicationReceiver.Settings)
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

                    client.AddNotificationReceiver(receiver, notificationPredicate);
                }
            }

            return client;
        }

        public static Task<T> CreateAsync<T>(string typeName, IServiceProvider serviceProvider, IDictionary<string, object> settings) where T : class
        {
            if (typeName == null) throw new ArgumentNullException(nameof(typeName));
            var type = ParseTypeName(typeName);
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

        public static Type ParseTypeName(string typeName)
        {
            return TypeUtil
                .GetAllLoadedTypes()
                .FirstOrDefault(t => t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase)) ??
                       Type.GetType(typeName, true, true);
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
    }
}