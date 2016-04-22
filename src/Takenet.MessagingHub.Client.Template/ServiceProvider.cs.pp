using System;
using System.Collections.Generic;
//using SimpleInjector;

namespace $rootnamespace$
{
    class ServiceProvider : IServiceProvider
    {
        //private Container Container { get; }
		public Dictionary<Type, object> TypeDictionary { get; }
		private readonly IServiceProvider _underlyingServiceProvider;

        public ServiceProvider(IServiceProvider underlyingServiceProvider)
        {
            /*
			Container = new Container();
            // Delegates the resolution of unregistered types to the underlyingServiceProvider
            Container.ResolveUnregisteredType += (sender, args) =>
            {
                var registration = Lifestyle.Singleton.CreateRegistration(
                    args.UnregisteredServiceType,
                    () => underlyingServiceProvider.GetService(args.UnregisteredServiceType),
                    Container);

                args.Register(registration);
            };

            Container.RegisterSingleton<IMyInterface, MyClass>();
			*/

			TypeDictionary = new Dictionary<Type, object>();
			_underlyingServiceProvider = underlyingServiceProvider;
        }

        public object GetService(Type serviceType)
        {
            //return Container.GetInstance(serviceType);
			return TypeDictionary.ContainsKey(serviceType) ? TypeDictionary[serviceType] : _underlyingServiceProvider.GetService(serviceType);
        }
    }
}