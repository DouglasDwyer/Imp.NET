using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace DouglasDwyer.Imp
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public class ShareAsAttribute : Attribute
    {
        public static IdentifiedCollection<Type> ProxyIndex { get; } = new IdentifiedCollection<Type>();
        public static List<ProxyType> ProxyData { get; } = new List<ProxyType>();
        public static Dictionary<Type, Type> SharedTypes { get; } = new Dictionary<Type, Type>();

        public Type TypeToShare { get; private set; }
        public Type InterfaceBinding { get; private set; }

        static ShareAsAttribute()
        {
            
        }

        public ShareAsAttribute(Type typeToShare, Type interfaceBinding)
        {
            TypeToShare = typeToShare;
            InterfaceBinding = interfaceBinding;
        }
    }
}
