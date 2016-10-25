using System;

namespace Takenet.MessagingHub.Client.Host
{
    /// <summary>
    /// Defines a type resolver service.
    /// </summary>
    public interface ITypeResolver
    {
        /// <summary>
        /// Resolves a type by its name.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        /// <returns></returns>
        Type GetType(string typeName);
    }
}