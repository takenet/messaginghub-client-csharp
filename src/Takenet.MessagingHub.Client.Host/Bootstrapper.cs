using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Serialization;
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

            IServiceProvider serviceProvider = null;
            if (application.ServiceProviderType != null)
            {
                var serviceProviderType = ParseTypeName(application.ServiceProviderType);
                if (serviceProviderType != null && typeof(IServiceProvider).IsAssignableFrom(serviceProviderType))
                    serviceProvider = (IServiceProvider)Activator.CreateInstance(serviceProviderType);
            }

            var localServiceProvider = new LocalServiceProvider(serviceProvider);

            localServiceProvider.TypeDictionary.Add(typeof(MessagingHubClientBuilder), builder);

            var client = await BuildMessagingHubClientAsync(application, builder, localServiceProvider);
            localServiceProvider.TypeDictionary.Add(typeof(IMessagingHubClient), client);
            localServiceProvider.TypeDictionary.Add(typeof(IMessagingHubSender), client);

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

        private static async Task<IMessagingHubClient> BuildMessagingHubClientAsync(Application application, MessagingHubClientBuilder builder, LocalServiceProvider localServiceProvider)
        {
            var client = builder.Build();

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

                    client.AddMessageReceiver(receiver, messagePredicate);
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

                    client.AddNotificationReceiver(receiver, notificationPredicate);
                }
            }

            return client;
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

        private static Task<T> CreateAsync<T>(string typeName, IServiceProvider serviceProvider, IDictionary<string, object> settings) where T : class
        {
            if (typeName == null) throw new ArgumentNullException(nameof(typeName));
            var type = ParseTypeName(typeName);
            return CreateAsync<T>(type, serviceProvider, settings);
        }

        private static Task<T> CreateAsync<T>(Type type, IServiceProvider serviceProvider, IDictionary<string, object> settings) where T : class
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

        private static Type ParseTypeName(string typeName)
        {
            return TypeUtil
                .GetAllLoadedTypes()
                .FirstOrDefault(t => t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase)) ??
                       Type.GetType(typeName, true, true);
        }

        private class LocalServiceProvider : IServiceProvider
        {
            private readonly IServiceProvider _underlyingServiceProvider;

            public LocalServiceProvider(IServiceProvider underlyingServiceProvider)
            {
                _underlyingServiceProvider = underlyingServiceProvider;
                TypeDictionary = new Dictionary<Type, object>();
            }

            public Dictionary<Type, object> TypeDictionary { get; }

            public object GetService(Type serviceType)
            {
                object result = null;

                // Try to find the serviceType in the underlying service provider
                if (_underlyingServiceProvider != null)
                {
                    try
                    {
                        result = _underlyingServiceProvider.GetService(serviceType);
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine($"Exception trying to load type {serviceType} from service provider of type {_underlyingServiceProvider.GetType().Name}: {e}");
                    }
                }

                // If could not find the serviceType, try to find it in the TypeDictionary
                if (result == null && TypeDictionary.ContainsKey(serviceType))
                    result = TypeDictionary[serviceType];

                return result;
            }
        }

        private class Factory<T> : IFactory<T> where T : class
        {
            private readonly Type _type;

            public Factory(Type type)
            {
                if (type.IsAssignableFrom(typeof(T))) throw new ArgumentException($"The type '{type}' is not assignable from '{typeof(T)}'");
                _type = type;
            }

            public Task<T> CreateAsync(IServiceProvider serviceProvider, IDictionary<string, object> settings)
            {
                var service = serviceProvider.GetService(_type) as T ?? 
                              GetService(_type, serviceProvider, settings) as T ?? 
                              _type.GetDefaultValue() as T;

                return Task.FromResult(service);
            }

            private static object GetService(Type serviceType, IServiceProvider serviceProvider, params object[] args)
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
                    for (var i = 0; i < parameters.Length; i++)
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