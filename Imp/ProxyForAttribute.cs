using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DouglasDwyer.Imp
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ProxyForAttribute : Attribute
    {
        /*public static Dictionary<Type, Type> ProxyInterfaceToRemoteClass
        {
            get
            {
                if (InterfaceToClass is null)
                {

                    InterfaceToClass = new Dictionary<Type, Type>();
                    ClassToInterface = new Dictionary<Type, Type>();
                    foreach (Type type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()))
                    {
                        ProxyForAttribute attribute = type.GetCustomAttribute<ProxyForAttribute>();
                        if(attribute != null)
                        {
                            InterfaceToClass[attribute.InterfaceType] = type;
                            ClassToInterface[type] = attribute.InterfaceType;
                        }
                    }
                }
                return InterfaceToClass;
            }
        }
        public static Dictionary<Type,Type> RemoteClassToProxyInterface
        {
            get
            {
                if(InterfaceToClass is null)
                {
                    //initialize the interfaces here
                    Dictionary<Type, Type> a = ProxyInterfaceToRemoteClass;
                }
                return ClassToInterface;
            }
        }
        private static Dictionary<Type, Type> InterfaceToClass;
        private static Dictionary<Type, Type> ClassToInterface;*/

        public Type InterfaceType { get; private set; }

        public ProxyForAttribute(Type interfaceType)
        {
            InterfaceType = interfaceType;
        }
    }
}
