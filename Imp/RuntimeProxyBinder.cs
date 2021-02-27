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
            Dictionary<Type, Type> remoteBaseTypes = new Dictionary<Type, Type>();
            foreach(Type type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()))
            {
                ProxyForAttribute attribute = type.GetCustomAttribute<ProxyForAttribute>();
                if (attribute != null)
                {
                    if (remoteBaseTypes.ContainsKey(attribute.InterfaceType))
                    {
                        throw new ArgumentException("Types " + type + " and " + remoteBaseTypes[attribute.InterfaceType] + " are both declared as remote base types for the shared interface " + attribute.InterfaceType + ". Only one remote base type may act as proxy for a given shared interface.");
                    }
                    else
                    {
                        remoteBaseTypes[attribute.InterfaceType] = type;
                    }
                }
            }

            foreach (ShareAsAttribute attribute in AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetCustomAttributes(typeof(ShareAsAttribute))))
            {
                if (!attribute.InterfaceBinding.IsInterface)
                {
                    throw new ArgumentException("Type " + attribute.InterfaceBinding + " is not an interface and thus cannot be shared.");
                }
                else if (!attribute.InterfaceBinding.IsAssignableFrom(attribute.TypeToShare)
                    && !(attribute.InterfaceBinding.IsGenericType
                        && !attribute.InterfaceBinding.IsConstructedGenericType
                        && attribute.TypeToShare.IsGenericType
                        && !attribute.TypeToShare.IsConstructedGenericType
                        && attribute.TypeToShare.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == attribute.InterfaceBinding))
                        && attribute.TypeToShare.GetGenericArguments().Count() == attribute.InterfaceBinding.GetGenericArguments().Count())
                {
                    throw new ArgumentException("Type " + attribute.TypeToShare + " cannot be casted to interface " + attribute.TypeToShare + " and thus cannot be shared.");
                }
                LocalClassToInterface[attribute.TypeToShare] = attribute.InterfaceBinding;
            }
            Dictionary<Type, Type> thinClassBindings = new Dictionary<Type, Type>();
            List<Type> doubledTypes = new List<Type>();
            foreach(KeyValuePair<Type,Type> pair in LocalClassToInterface)
            {
                if(thinClassBindings.ContainsKey(pair.Value))
                {
                    thinClassBindings.Remove(pair.Value);
                    doubledTypes.Add(pair.Value);
                }
                else if(!doubledTypes.Contains(pair.Value))
                {
                    thinClassBindings.Add(pair.Value, pair.Key);
                }
            }

            foreach (Type type in LocalClassToInterface.Values.Distinct())
            {
                ProxyData.Add(new ProxyType(ProxyIndex.Add(type), type, GetRemoteTypeForInterface(type, remoteBaseTypes, thinClassBindings, LocalClassToInterface)));
            }
            GenerateProxies(ProxyIndex.Values);
        }

        private Type GetRemoteTypeForInterface(Type inter, Dictionary<Type, Type> bindings, Dictionary<Type, Type> thinClassBindings, Dictionary<Type,Type> thinInterfaceBindings)
        {
            if (bindings.ContainsKey(inter))
            {
                return bindings[inter];
            }
            else
            {
                if(thinClassBindings.ContainsKey(inter))
                {
                    Type baseType = thinClassBindings[inter].BaseType;
                    while (baseType != null)
                    {
                        if (thinInterfaceBindings.ContainsKey(baseType))
                        {
                            return GetRemoteTypeForInterface(thinInterfaceBindings[baseType], bindings, thinClassBindings, thinInterfaceBindings);
                        }
                        else
                        {
                            baseType = baseType.BaseType;
                        }
                    }
                }
                return typeof(RemoteSharedObject);
            }
        }
    }
}
