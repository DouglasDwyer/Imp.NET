using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DouglasDwyer.Imp
{
    /// <summary>
    /// Indicates that a given type should be utilized as the base type for remote interface implementations. Base types must inherit from their shared interface. Base types should be <c>abstract</c>; any members left unimplemented will be automatically implemented during proxy generation.
    /// </summary>
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

        /// <summary>
        /// The interface for which this base class acts as a remote proxy.
        /// </summary>
        public Type InterfaceType { get; private set; }

        /// <summary>
        /// Indicates that the given type should be utilized as the base type for remote interface implementations. Base types must inherit from their shared interface. Base types should be <c>abstract</c>; any members left unimplemented will be automatically implemented during proxy generation.
        /// </summary>
        /// <param name="interfaceType">The interface for which this class acts as a remote proxy.</param>
        public ProxyForAttribute(Type interfaceType)
        {
            InterfaceType = interfaceType;
        }
    }
}
