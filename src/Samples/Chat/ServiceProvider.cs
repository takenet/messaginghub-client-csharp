using System;
using System.Collections.Generic;
using Takenet.MessagingHub.Client.Host;

namespace Chat
{
    class ServiceProvider : IServiceContainer
    {
        public Dictionary<Type, object> TypeDictionary { get; }

        public ServiceProvider()
        {
            TypeDictionary = new Dictionary<Type, object>();
        }

        public object GetService(Type serviceType)
        {
            return TypeDictionary[serviceType];
        }

        public void RegisterService(Type serviceType, object instance)
        {
            TypeDictionary[serviceType] = instance;
        }
    }
}