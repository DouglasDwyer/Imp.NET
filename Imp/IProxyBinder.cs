using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.Imp
{
    public interface IProxyBinder
    {
        ProxyType GetDataForProxy(ushort id);
        ProxyType GetDataForProxy(Type proxyInterface);
        ProxyType GetDataForSharedType(Type sharedType);
        IEnumerable<Type> GetProxyTypes();
        Type GetRemoteType(ushort id);
        Type GetRemoteType(Type proxyInterface);
        Type GetProxyForRemoteType(Type remoteClass);
        Type GetProxyForLocalType(Type localType);
        ushort? GetIDForProxy(Type proxyInterface);
        ushort? GetIDForSharedType(Type sharedType);
        ushort? GetIDForLocalType(Type localType);
        ushort? GetIDForRemoteType(Type remoteType);
    }
}
