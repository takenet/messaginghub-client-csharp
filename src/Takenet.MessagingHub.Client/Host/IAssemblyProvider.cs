using System.Collections.Generic;
using System.Reflection;

namespace Takenet.MessagingHub.Client.Host
{
    /// <summary>
    /// Defines a service for providing assemblies.
    /// </summary>
    public interface IAssemblyProvider
    {
        IEnumerable<Assembly> GetAssemblies();
    }
}