using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DouglasDwyer.Imp
{
    internal class RuntimeProxyBinder : GeneratorProxyBinder
    {
        private RuntimeProxyBinder() { }

        public static RuntimeProxyBinder CreateAndBind()
        {
            RuntimeProxyBinder binder = new RuntimeProxyBinder();
            binder.Generate();
            return binder;
        }

        private void Generate()
        {
            foreach (ShareAsAttribute attribute in AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetCustomAttributes(typeof(ShareAsAttribute))))
            {
                if (!attribute.InterfaceBinding.IsInterface)
                {
                    throw new ArgumentException("Type " + attribute.InterfaceBinding + " is not an interface and thus cannot be shared.");
                }
                else if (!attribute.InterfaceBinding.IsAssignableFrom(attribute.TypeToShare))
                {
                    throw new ArgumentException("Type " + attribute.TypeToShare + " cannot be casted to interface " + attribute.TypeToShare + " and thus cannot be shared.");
                }
                LocalClassToInterface[attribute.TypeToShare] = attribute.InterfaceBinding;
            }
            foreach (Type type in LocalClassToInterface.Values.Distinct())
            {
                ProxyData.Add(new ProxyType(ProxyIndex.Add(type), type));
            }
            GenerateProxies(ProxyIndex.Values);
        }
    }
}
