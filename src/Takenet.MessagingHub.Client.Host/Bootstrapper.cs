using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Serialization;
using Takenet.MessagingHub.Client.Connection;
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
        /// <param name="serviceProvider">The service provider to be used when building the type instances. It not provided, the only injected types will be the <see cref="Sender.IMessagingHubSender" /> and the settings instances.</param>
        /// <param name="loadAssembliesFromWorkingDirectory">if set to <c>true</c> indicates to the bootstrapper to load all assemblies from the current working directory.</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="ArgumentException">At least an access key or password must be defined</exception>
        /// <exception cref="FileNotFoundException">Could not find the 'application.json' file</exception>
        /// <exception cref="ArgumentException">At least an access key or password must be defined</exception>
        public static async Task<IStoppable> StartAsync(Application application = null, IServiceProvider serviceProvider = null, bool loadAssembliesFromWorkingDirectory = true)
        {            
            if (application == null)
            {
                if (!File.Exists(DefaultApplicationFileName)) throw new FileNotFoundException($"Could not find the '{DefaultApplicationFileName}' file", DefaultApplicationFileName);
                application = Application.ParseFromJsonFile(DefaultApplicationFileName);
            }

            if (loadAssembliesFromWorkingDirectory)
            {
                var assembly = Assembly.GetExecutingAssembly();
                var path = new FileInfo(assembly.Location).DirectoryName;
                TypeUtil.LoadAssembliesAndReferences(path, assemblyFilter: TypeUtil.IgnoreSystemAndMicrosoftAssembliesFilter);
            }

            var connectionBuilder = new MessagingHubConnectionBuilder();
            if (application.Login != null)
            {
                if (application.Password != null)
                {
                    connectionBuilder = connectionBuilder.UsingAccount(application.Login, application.Password);
                }
                else if (application.AccessKey != null)
                {
                    connectionBuilder = connectionBuilder.UsingAccessKey(application.Login, application.AccessKey);
                }
                else
                {
                    throw new ArgumentException("At least an access key or password must be defined", nameof(application));
                }
            }
            else
            {
                connectionBuilder = connectionBuilder.UsingGuest();
            }

            if (application.Domain != null) connectionBuilder = connectionBuilder.UsingDomain(application.Domain);
            if (application.HostName != null) connectionBuilder = connectionBuilder.UsingHostName(application.HostName);
            if (application.SendTimeout != 0) connectionBuilder = connectionBuilder.WithSendTimeout(TimeSpan.FromMilliseconds(application.SendTimeout));
            if (application.SessionEncryption.HasValue) connectionBuilder = connectionBuilder.UsingEncryption(application.SessionEncryption.Value);
            if (application.SessionCompression.HasValue) connectionBuilder = connectionBuilder.UsingCompression(application.SessionCompression.Value);

            var localServiceProvider = new ServiceProvider(serviceProvider);

            var connection = connectionBuilder.Build();
            await connection.ConnectAsync().ConfigureAwait(false);

            localServiceProvider.TypeDictionary.Add(typeof(MessagingHubConnectionBuilder), connectionBuilder);
            localServiceProvider.TypeDictionary.Add(typeof(MessagingHubConnection), connection);

            var listener = await BuildMessagingHubListenerAsync(application, connection, localServiceProvider);
            localServiceProvider.TypeDictionary.Add(typeof(MessagingHubListener), listener);

            var sender = new MessagingHubSender(connection);
            localServiceProvider.TypeDictionary.Add(typeof(MessagingHubSender), sender);

            var stoppables = new IStoppable[2];
            stoppables[0] = listener;
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

        private static async Task<MessagingHubListener> BuildMessagingHubListenerAsync(Application application, MessagingHubConnection connection, ServiceProvider localServiceProvider)
        {
            var listener = new MessagingHubListener(connection);

            if (application.MessageReceivers != null && application.MessageReceivers.Length > 0)
            {
                foreach (var applicationReceiver in application.MessageReceivers)
                {                    
                    var receiver = 
                        await 
                            CreateAsync<IMessageReceiver>(applicationReceiver.Type, localServiceProvider, MergeSettings(application, applicationReceiver))
                                .ConfigureAwait(false);

                    Predicate<Message> messagePredicate = m => m != null;

                    if (applicationReceiver.MediaType != null)
                    {
                        var currentMessagePredicate = messagePredicate;
                        var mediaType = MediaType.Parse(applicationReceiver.MediaType);
                        messagePredicate = m => currentMessagePredicate(m) && m.Type.Equals(mediaType);
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
                        messagePredicate = m => currentMessagePredicate(m) && senderRegex.IsMatch(m.GetSender().ToString());
                    }

                    if (applicationReceiver.Destination != null)
                    {
                        var currentMessagePredicate = messagePredicate;
                        var destinationRegex = new Regex(applicationReceiver.Destination, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                        messagePredicate = m => currentMessagePredicate(m) && destinationRegex.IsMatch(m.To.ToString());
                    }

                    listener.AddMessageReceiver(receiver, messagePredicate);
                }
            }

            if (application.NotificationReceivers != null && application.NotificationReceivers.Length > 0)
            {
                foreach (var applicationReceiver in application.NotificationReceivers)
                {                    
                    var receiver =
                        await
                            CreateAsync<INotificationReceiver>(applicationReceiver.Type, localServiceProvider, MergeSettings(application, applicationReceiver))
                                .ConfigureAwait(false);


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
                        notificationPredicate = n => currentNotificationPredicate(n) && senderRegex.IsMatch(n.GetSender().ToString());
                    }

                    if (applicationReceiver.Destination != null)
                    {
                        var currentNotificationPredicate = notificationPredicate;
                        var destinationRegex = new Regex(applicationReceiver.Destination, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                        notificationPredicate = n => currentNotificationPredicate(n) && destinationRegex.IsMatch(n.To.ToString());
                    }

                    listener.AddNotificationReceiver(receiver, notificationPredicate);
                }
            }

            return listener;
        }

        private static IDictionary<string, object> MergeSettings(Application application, ApplicationReceiver applicationReceiver)
        {
            IDictionary<string, object> settings;
            if (application.Settings == null)
            {
                settings = applicationReceiver.Settings;
            }
            else if (applicationReceiver.Settings == null)
            {
                settings = application.Settings;
            }
            else
            {
                settings = applicationReceiver
                    .Settings
                    .Union(application.Settings)
                    .GroupBy(a => a.Key)
                    .ToDictionary(a => a.Key, a => a.First().Value);
            }
            return settings;
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

        private class ServiceProvider : IServiceProvider
        {
            private readonly IServiceProvider _underlyingServiceProvider;

            public ServiceProvider(IServiceProvider underlyingServiceProvider)
            {
                _underlyingServiceProvider = underlyingServiceProvider;
                TypeDictionary = new Dictionary<Type, object>();
            }

            public Dictionary<Type, object> TypeDictionary { get; }

            public object GetService(Type serviceType)
            {
                if (TypeDictionary.ContainsKey(serviceType)) return TypeDictionary[serviceType];
                if (_underlyingServiceProvider != null) return _underlyingServiceProvider.GetService(serviceType);
                return serviceType.GetDefaultValue();
            }
        }

        private class Factory<T> : IFactory<T> where T : class
        {
            private readonly Type _type;
            
            public Factory(Type type)
            {
                if (type.IsAssignableFrom(typeof (T))) throw new ArgumentException($"The type '{type}' is not assignable from '{typeof(T)}'");                
                _type = type;
            }

            public Task<T> CreateAsync(IServiceProvider serviceProvider, IDictionary<string, object> settings)
            {
                var service = serviceProvider.GetService(_type) as T ?? (T) GetService(_type, serviceProvider, settings);
                return Task.FromResult(service);
            }

            private object GetService(Type serviceType, IServiceProvider serviceProvider, params object[] args)
            {
                // Check the type constructors
                try
                {
                var serviceConstructor = serviceType
                    .GetConstructors()
                    .OrderByDescending(c => c.GetParameters().Length)
                    .FirstOrDefault();

                if (serviceConstructor == null)
                {
                    throw new ArgumentException($"The  type '{serviceType}' doesn't have a public constructor", nameof(serviceType));
                }

                var parameters = serviceConstructor.GetParameters();
                var serviceArgs = new object[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    var parameter = parameters[i];

                    var arg = args.FirstOrDefault(p => parameter.ParameterType.IsInstanceOfType(p));
                    if (arg != null)
                    {
                        serviceArgs[i] = arg;
                    }
                    else
                    {
                        serviceArgs[i] = serviceProvider.GetService(parameter.ParameterType);
                    }
                }

                return Activator.CreateInstance(serviceType, serviceArgs);
            }
                catch (Exception e)
                {
                    throw new ArgumentException($"Could not instantiate type {serviceType.FullName}!", e);
                }
            }
        }

        private class StoppableWrapper : IStoppable
        {
            private readonly IStoppable[] _stoppables;

            public StoppableWrapper(params IStoppable[] stoppables)
            {
                _stoppables = stoppables;
            }

            public async Task StopAsync()
            {
                foreach (var stoppable in _stoppables)
                {
                    if (stoppable != null) await stoppable.StopAsync().ConfigureAwait(false);
                }                
            }
        }
    }
} 