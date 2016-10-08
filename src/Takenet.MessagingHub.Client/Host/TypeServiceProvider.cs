using System;
using System.Collections.Generic;

namespace Takenet.MessagingHub.Client.Host
{
    /// <summary>
    /// Defines a simple type service provider.
    /// </summary>
    /// <seealso cref="Takenet.MessagingHub.Client.Host.IServiceContainer" />
    public class TypeServiceProvider : IServiceContainer
    {
        protected readonly Dictionary<Type, object> TypeDictionary;

        public TypeServiceProvider()
        {
            TypeDictionary = new Dictionary<Type, object>();
        }

        public IServiceProvider SecondaryServiceProvider { get; set; }

        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public object GetService(Type serviceType)
        {
            if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));
            object result;
            var service = TypeDictionary.TryGetValue(serviceType, out result) ?
                result : SecondaryServiceProvider?.GetService(serviceType);

            var factory = service as Func<object>;
            if (factory != null)
            {
                return factory();
            }

            return service;
        }

        /// <summary>
        /// Registers the service instance.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="instance">The instance.</param>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public void RegisterService(Type serviceType, object instance)
        {
            if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));
            if (instance == null) throw new ArgumentNullException(nameof(instance));            
            TypeDictionary.Add(serviceType, instance);
            (SecondaryServiceProvider as IServiceContainer)?.RegisterService(serviceType, instance);
        }

        public void RegisterService(Type serviceType, Func<object> instanceFactory)
        {
            if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));
            if (instanceFactory == null) throw new ArgumentNullException(nameof(instanceFactory));
            TypeDictionary.Add(serviceType, instanceFactory);
            (SecondaryServiceProvider as IServiceContainer)?.RegisterService(serviceType, instanceFactory);
        }

        public void RegisterExtensions()
        {
            throw new NotImplementedException();
        }

    } 
}