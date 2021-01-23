using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DouglasDwyer.Imp
{
    public class PredefinedProxyBinder : GeneratorProxyBinder
    {
        private PredefinedProxyBinder() { }

        public static PredefinedProxyBinder CreateAndBind(IList<string> types)
        {
            PredefinedProxyBinder binder = new PredefinedProxyBinder();
            binder.Generate(types);
            return binder;
        }

        private void Generate(IList<string> types)
        {
            Dictionary<Type, Type> shareAsTypes = new Dictionary<Type, Type>();
            foreach (ShareAsAttribute attribute in AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetCustomAttributes(typeof(ShareAsAttribute))))
            {
                if (!attribute.InterfaceBinding.IsInterface)
                {
                    throw new ArgumentException("Type " + attribute.TypeToShare + " is not an interface and thus cannot be shared.");
                }
                else if (!attribute.InterfaceBinding.IsAssignableFrom(attribute.TypeToShare))
                {
                    throw new ArgumentException("Type " + attribute.TypeToShare + " cannot be casted to interface " + attribute.TypeToShare + " and thus cannot be shared.");
                }
                shareAsTypes[attribute.InterfaceBinding] = attribute.TypeToShare;
            }
            foreach (string type in types)
            {
                Type givenType = Type.GetType(type);
                if (givenType is null)
                {
                    throw new ArgumentException("Could not find type " + type);
                }
                if (!shareAsTypes.ContainsKey(givenType))
                {
                    throw new ArgumentException(givenType + " is not a shared type.");
                }
                LocalClassToInterface[shareAsTypes[givenType]] = givenType;
            }
            foreach (Type type in LocalClassToInterface.Values.Distinct())
            {
                ProxyData.Add(new ProxyType(ProxyIndex.Add(type), type));
            }
            GenerateProxies(ProxyIndex.Values);
        }
    }
}
