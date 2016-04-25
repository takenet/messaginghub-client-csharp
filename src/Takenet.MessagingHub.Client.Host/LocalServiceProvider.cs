using System;
using System.Collections.Generic;

namespace Takenet.MessagingHub.Client.Host
{
    internal class LocalServiceProvider : IServiceProvider
    {
        public LocalServiceProvider()
        {
            TypeDictionary = new Dictionary<Type, object>
            {
                { typeof(IServiceProvider), this }
            };
        }

        public Dictionary<Type, object> TypeDictionary { get; }

        internal IServiceProvider SecondaryServiceProvider { get; set; }

        public object GetService(Type serviceType)
        {
            object result;
            return TypeDictionary.TryGetValue(serviceType, out result) ?
                result : SecondaryServiceProvider?.GetService(serviceType);
        }
    } 
}