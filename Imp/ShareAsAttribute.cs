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
            foreach (ShareAsAttribute attribute in AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetCustomAttributes(typeof(ShareAsAttribute))))
            {
                SharedTypes[attribute.TypeToShare] = attribute.InterfaceBinding;
            }
            foreach(Type type in SharedTypes.Values.Distinct())
            {
                ProxyData.Add(new ProxyType(ProxyIndex.Add(type), type));
            }
        }

        public ShareAsAttribute(Type typeToShare, Type interfaceBinding)
        {
            TypeToShare = typeToShare;
            InterfaceBinding = interfaceBinding;
        }
    }
}
