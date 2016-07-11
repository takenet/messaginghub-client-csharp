using System;
using System.IO;
using System.Reflection;
using NLog;
using NLog.Config;
using SimpleInjector;
using Takenet.MessagingHub.Client.Host;

namespace Playground
{
    internal class ServiceProvider : IServiceContainer
    {
        private readonly Container _container;

        public ServiceProvider()
        {
            _container = new Container();
            _container.RegisterSingleton<ILogger>(() => LogManager.GetLogger(nameof(Playground)));

            var assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            LogManager.Configuration = new XmlLoggingConfiguration(assemblyFolder + "\\NLog.config", true);
        }

        public object GetService(Type serviceType)
        {
            return _container.GetInstance(serviceType);
        }

        public void RegisterService(Type serviceType, object instance)
        {
            _container.RegisterSingleton(serviceType, instance);
        }
    }
}