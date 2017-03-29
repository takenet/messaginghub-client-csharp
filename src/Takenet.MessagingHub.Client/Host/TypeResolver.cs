using System;
using System.Linq;

namespace Takenet.MessagingHub.Client.Host
{
    public sealed class TypeResolver : ITypeResolver
    {
        private TypeResolver()
        {
        }

        public static TypeResolver Instance => new TypeResolver();

        public Type Resolve(string typeName)
        {
            var types = ReferencesUtil
                            .GetAllLoadedTypes()
                            .Where(t => t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase));

            if (types.Count() == 1) return types.First();
            else if (types.Count() == 0) return Type.GetType(typeName, true, true);
            else throw new Exception($"There are multiple types named {typeName}");
        }
    }
}