using System;

namespace Takenet.MessagingHub.Client.Host
{
    public interface ITypeProvider
    {
        Type GetType(string typeName);
    }
}