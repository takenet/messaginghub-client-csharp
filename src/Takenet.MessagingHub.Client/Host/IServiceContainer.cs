using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client.Host
{
    /// <summary>
    /// Defines a service container that allows the registration of service type instances.
    /// </summary>
    /// <seealso cref="System.IServiceProvider" />
    public interface IServiceContainer : IServiceProvider
    {
        /// <summary>
        /// Registers the service instance.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="instance">The instance.</param>
        void RegisterService(Type serviceType, object instance);

        void RegisterService(Type serviceType, Func<object> instanceFactory);
    }
}
