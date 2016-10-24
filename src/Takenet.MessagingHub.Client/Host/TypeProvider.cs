using System;
using System.Linq;
using Lime.Protocol.Serialization;

namespace Takenet.MessagingHub.Client.Host
{
    public sealed class TypeProvider : ITypeProvider
    {
        private TypeProvider()
        {
            
        }

        public static TypeProvider Instance => new TypeProvider();

        public Type GetType(string typeName) => 
            TypeUtil
                .GetAllLoadedTypes()
                .FirstOrDefault(t => t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase)) ?? 
            Type.GetType(typeName, true, true);
    }
}