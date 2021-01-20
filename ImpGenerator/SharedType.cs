using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.ImpGenerator
{
    public class SharedType
    {
        public ITypeSymbol TypeSymbol;
        public string InterfaceName;
        public string InterfaceNamespace;
        public string RemoteName;
        public string RemoteNamespace;
        public List<string> UsingDirectives;

        public string FullInterfaceName => "global::" + (string.IsNullOrEmpty(InterfaceNamespace) ? InterfaceName : InterfaceNamespace + "." + InterfaceName);
        public string FullRemoteName => "global::" + (string.IsNullOrEmpty(RemoteNamespace) ? RemoteName : RemoteNamespace + "." + RemoteName);

        public SharedType(ITypeSymbol typeSymbol, string interfaceName, string interfaceNamespace, string remoteName, string remoteNamespace, List<string> usingDirectives)
        {
            TypeSymbol = typeSymbol;
            InterfaceName = interfaceName;
            InterfaceNamespace = interfaceNamespace;
            RemoteName = remoteName;
            RemoteNamespace = remoteNamespace;
            UsingDirectives = usingDirectives;
        }
    }
}
