using System;
using System.Collections.Generic;

namespace Takenet.MessagingHub.Client.Host
{
    internal class LocalServiceProvider : IServiceContainer
    {
        private readonly Dictionary<Type, object> _typeDictionary;

        public LocalServiceProvider()
        {
            _typeDictionary = new Dictionary<Type, object>();
            RegisterService(typeof(IServiceProvider), this);
        }


        internal IServiceProvider SecondaryServiceProvider { get; set; }

        public object GetService(Type serviceType)
        {
            if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));
            object result;
            return _typeDictionary.TryGetValue(serviceType, out result) ?
                result : SecondaryServiceProvider?.GetService(serviceType);
        }

        public void RegisterService(Type serviceType, object instance)
        {
            if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));
            if (instance == null) throw new ArgumentNullException(nameof(instance));            
            _typeDictionary.Add(serviceType, instance);
            (SecondaryServiceProvider as IServiceContainer)?.RegisterService(serviceType, instance);
        }
    } 
}