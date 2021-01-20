using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.ImpGenerator
{
    public class ProxyType
    {
        public ITypeSymbol ProxyInterface;
        public ITypeSymbol RemoteClass;

        public ProxyType(ITypeSymbol proxyInterface, ITypeSymbol remoteClass)
        {
            ProxyInterface = proxyInterface;
            RemoteClass = remoteClass;
        }
    }
}