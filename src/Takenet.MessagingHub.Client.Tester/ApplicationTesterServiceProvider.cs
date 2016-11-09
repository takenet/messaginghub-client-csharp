using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Takenet.MessagingHub.Client.Host;

namespace Takenet.MessagingHub.Client.Tester
{
    public abstract class ApplicationTesterServiceProvider : IServiceContainer
    {
        private readonly IDictionary<Type, object> _testServiceTypes = new ConcurrentDictionary<Type, object>();

        public object GetService(Type serviceType)
        {
            try
            {
                var testService = _testServiceTypes.ContainsKey(serviceType) ? _testServiceTypes[serviceType] : null;
                var service = testService ?? ApplicationTester.ApplicationServiceProvider?.GetService(serviceType);
                if (service is Func<object>)
                {
                    return (service as Func<object>)();
                }
                return service;
            }
            catch (Exception ex)
            {
                throw new TypeInitializationException(serviceType.FullName, ex);
            }
        }

        public void RegisterService(Type serviceType, Func<object> instanceFactory)
        {
            RegisterService(serviceType, instanceFactory as object);
        }

        public void RegisterService(Type serviceType, object instance)
        {
            try
            {
                var applicationServiceContainer = ApplicationTester.ApplicationServiceProvider as IServiceContainer;
                if (applicationServiceContainer != null)
                    applicationServiceContainer.RegisterService(serviceType, instance);
                else
                    _testServiceTypes[serviceType] = instance;
            }
            catch (Exception)
            {
                // Ignore registration errors
            }
        }
    }
}