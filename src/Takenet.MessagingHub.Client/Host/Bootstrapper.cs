using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
        private static readonly IDictionary<string, Type> Types;        

        static Bootstrapper()
        {            
            Types = TypeUtil
                .GetAllLoadedTypes()
                .Where(t => 
                    typeof(IFactory<IMessageReceiver>).IsAssignableFrom(t) || 
                    typeof(IFactory<INotificationReceiver>).IsAssignableFrom(t) || 
                    typeof (IStartable).IsAssignableFrom(t) || 
                    typeof(IMessageReceiver).IsAssignableFrom(t) || 
                    typeof(INotificationReceiver).IsAssignableFrom(t))
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .GroupBy(t => t.Name)
                .ToDictionary(t => t.Key, t => t.First());
        }

        /// <summary>
        /// Creates ans starts an application with the given settings.
        /// </summary>
        /// <param name="application">The application instance. If not defined, the class will look for an application.json file in the current directory.</param>
        /// <param name="serviceProvider">The service provider to be used when building the type instances. It not provided, the only injected types will be the <see cref="IMessagingHubSender"/> and the settings instances.</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException">Could not find the 'application.json' file</exception>
        /// <exception cref="ArgumentException">At least an access key or password must be defined</exception>
        public static async Task<IStoppable> StartAsync(Application application = null, IServiceProvider serviceProvider = null)
        {            
            if (application == null)
            {
                var fileName = $"{nameof(application)}.json";
                if (!File.Exists(fileName)) throw new FileNotFoundException($"Could not find the '{fileName}' file", fileName);
                application = JsonConvert.DeserializeObject<Application>(
                    File.ReadAllText(fileName), new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    });
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

            if (application.StartupType != null)
            {
                var startable = await CreateAsync<IStartable>(application.StartupType, localServiceProvider, application.Settings).ConfigureAwait(false);
                await startable.StartAsync().ConfigureAwait(false);
            }

            await sender.StartAsync().ConfigureAwait(false);
            return sender;
        }

        private static async Task<MessagingHubSenderBuilder> BuildSenderAsync(Application application, MessagingHubClientBuilder clientBuilder,
            ServiceProvider localServiceProvider)
        {
            localServiceProvider.TypeDictionary.Add(typeof(MessagingHubClientBuilder), clientBuilder);
            var senderBuilder = new MessagingHubSenderBuilder(clientBuilder);
            localServiceProvider.TypeDictionary.Add(typeof(MessagingHubSenderBuilder), senderBuilder);

            if (application.MessageReceivers != null && application.MessageReceivers.Length > 0)
            {
                foreach (var applicationReceiver in application.MessageReceivers)
                {                    
                    var receiver = 
                        await 
                            CreateAsync<IMessageReceiver>(applicationReceiver.Type, localServiceProvider, MergeSettings(application, applicationReceiver))
                                .ConfigureAwait(false);
                    MediaType mediaType = null;
                    if (applicationReceiver.MediaType != null) mediaType = MediaType.Parse(applicationReceiver.MediaType);
                    senderBuilder = senderBuilder.AddMessageReceiver(receiver, mediaType);
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

        private static Task<T> CreateAsync<T>(string typeName, IServiceProvider serviceProvider, IDictionary<string, object> settings) where T : class
        {
            if (typeName == null) throw new ArgumentNullException(nameof(typeName));

            Type type;
            if (!Types.TryGetValue(typeName, out type))
            {
                type = Type.GetType(typeName, true);
            }

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
        }        
    }
} 