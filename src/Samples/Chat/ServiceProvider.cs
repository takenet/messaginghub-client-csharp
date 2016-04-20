using System;
using System.Collections.Generic;
using Lime.Protocol.Serialization;

namespace Chat
{
    class ServiceProvider : IServiceProvider
    {
        public ServiceProvider()
        {
            TypeDictionary = new Dictionary<Type, object>();
        }

        public Dictionary<Type, object> TypeDictionary { get; }

        public object GetService(Type serviceType)
        {
            return TypeDictionary.ContainsKey(serviceType) ? TypeDictionary[serviceType] : serviceType.GetDefaultValue();
        }
    }
}