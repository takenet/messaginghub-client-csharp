using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Takenet.MessagingHub.Client.Receivers;

namespace Takenet.MessagingHub.Client.Host
{
    public static class Bootstrapper
    {
        public const string DefaultApplicationFileName = "application.json";

        /// <summary>
        /// Creates ans starts an application with the given settings.
        /// </summary>
        /// <param name="application">The application instance. If not defined, the class will look for an application.json file in the current directory.</param>
        /// <param name="serviceProvider">The service provider to be used when building the type instances. It not provided, the only injected types will be the <see cref="IMessagingHubSender" /> and the settings instances.</param>
        /// <param name="loadAssembliesFromWorkingDirectory">if set to <c>true</c> indicates to the bootstrapper to load all assemblies from the current working directory.</param>
        /// <returns></returns>
        /// <exception cref="System.IO.FileNotFoundException"></exception>
        /// <exception cref="System.ArgumentException">At least an access key or password must be defined</exception>
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
                TypeUtil.LoadAssembliesAndReferences(".", assemblyFilter: TypeUtil.IgnoreSystemAndMicrosoftAssembliesFilter);
            }

            var clientBuilder = new MessagingHubClientBuilder();
            if (application.Login != null)
            {
                if (application.Password != null)
                {
                    clientBuilder = clientBuilder.UsingAccount(application.Login, application.Password);
                }
                else if (application.AccessKey != null)
                {
                    clientBuilder = clientBuilder.UsingAccessKey(application.Login, application.AccessKey);
                }
                else
                {
                    throw new ArgumentException("At least an access key or password must be defined", nameof(application));
                }
            }
            else
            {
                clientBuilder = clientBuilder.UsingGuest();
            }

            if (application.Domain != null) clientBuilder = clientBuilder.UsingDomain(application.Domain);
            if (application.HostName != null) clientBuilder = clientBuilder.UsingHostName(application.HostName);
            if (application.SendTimeout != 0) clientBuilder = clientBuilder.WithSendTimeout(TimeSpan.FromMilliseconds(application.SendTimeout));

            var localServiceProvider = new ServiceProvider(serviceProvider);
            var senderBuilder = await BuildSenderAsync(application, clientBuilder, localServiceProvider);

            var sender = senderBuilder.Build();
            localServiceProvider.TypeDictionary.Add(typeof(IMessagingHubSender), sender);
            await sender.StartAsync().ConfigureAwait(false);

            var stoppables = new IStoppable[2];
            stoppables[0] = sender;
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

        private static async Task<MessagingHubSenderBuilder> BuildSenderAsync(Application application, MessagingHubClientBuilder clientBuilder,
            ServiceProvider localServiceProvider)
        {
            localServiceProvider.TypeDictionary.Add(typeof(MessagingHubClientBuilder), clientBuilder);            
            var senderBuilder = clientBuilder.SenderBuilder;
            localServiceProvider.TypeDictionary.Add(typeof(MessagingHubSenderBuilder), senderBuilder);            

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
                    
                    senderBuilder = senderBuilder.AddMessageReceiver(receiver, messagePredicate);
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
                    senderBuilder = senderBuilder.AddNotificationReceiver(receiver, applicationReceiver.EventType);
                }
            }
            return senderBuilder;
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

            var type = TypeUtil
                .GetAllLoadedTypes()
                .FirstOrDefault(t => t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase)) ?? Type.GetType(typeName, true, true);

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
                var service = serviceProvider.GetService(_type) as T;
                if (service == null) service = (T) GetService(_type, serviceProvider, settings);                
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
                        .First();

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