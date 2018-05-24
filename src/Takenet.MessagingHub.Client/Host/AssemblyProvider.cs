using System.Collections.Generic;
using System.Reflection;

namespace Takenet.MessagingHub.Client.Host
{
    public sealed class AssemblyProvider : IAssemblyProvider
    {
        private readonly Assembly[] _assemblies;

        public AssemblyProvider(params Assembly[] assemblies)
        {
            _assemblies = assemblies;
        }

        public IEnumerable<Assembly> GetAssemblies() => _assemblies;
    }
}
