using System;
using System.Linq;
using Lime.Protocol.Serialization;

namespace Takenet.MessagingHub.Client.Host
{
    public sealed class TypeResolver : ITypeResolver
    {
        private TypeResolver()
        {
            
        }

        public static TypeResolver Instance => new TypeResolver();

        public Type GetType(string typeName) => 
            TypeUtil
                .GetAllLoadedTypes()
                .FirstOrDefault(t => t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase)) ?? 
            Type.GetType(typeName, true, true);
    }
}