using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DouglasDwyer.Imp
{
    /// <summary>
    /// Stores information about a shared interface type.
    /// </summary>
    public sealed class ProxyType
    {
        /// <summary>
        /// The ID of this interface to be used in cross-network identification.
        /// </summary>
        public ushort InterfaceID { get; }
        /// <summary>
        /// The interface that should be shared.
        /// </summary>
        public Type InterfaceType { get; }
        /// <summary>
        /// The remote base type that should be inherited when generating a remote proxy class for this interface.
        /// </summary>
        public Type RemoteBaseType { get; }
        /// <summary>
        /// All of the methods that can be called on this interface.
        /// </summary>
        public IReadOnlyList<RemoteMethodInvoker> Methods { get; }
        /// <summary>
        /// All of the properties that can be accessed on this interface.
        /// </summary>
        public IReadOnlyList<PropertyInfo> Properties { get; }

        //public ProxyType(ushort id, Type mainType) : this(id, mainType, typeof(RemoteSharedObject)) { }

        /// <summary>
        /// Creates a new <see cref="ProxyType"/> with the specified ID and type data.
        /// </summary>
        /// <param name="id">The ID of this shared interface.</param>
        /// <param name="mainType">The interface to share.</param>
        /// <param name="remoteBaseType">The remote base type that should be inherited when generating a remote proxy class.</param>
        public ProxyType(ushort id, Type mainType, Type remoteBaseType)
        {
            InterfaceID = id;
            InterfaceType = mainType;
            RemoteBaseType = remoteBaseType;
            List<RemoteMethodInvoker> methods = new List<RemoteMethodInvoker>();
            List<PropertyInfo> properties = new List<PropertyInfo>();
            foreach (Type type in mainType.GetInterfaces().Concat(new[] { mainType }))
            {
                List<MethodInfo> accessorMethods = new List<MethodInfo>();
                foreach (PropertyInfo property in type.GetProperties())
                {
                    properties.Add(property);
                    if(property.CanWrite)
                    {
                        accessorMethods.Add(property.SetMethod);
                    }
                    if (property.CanRead)
                    {
                        accessorMethods.Add(property.GetMethod);
                    }
                }
                foreach (MethodInfo method in type.GetMethods().Where(x => !accessorMethods.Contains(x)))
                {
                    int callingClientLocation = -1;
                    int i = 0;
                    foreach(ParameterInfo para in method.GetParameters())
                    {
                        if(para.GetCustomAttribute<CallingClientAttribute>() != null)
                        {
                            if(callingClientLocation > -1)
                            {
                                throw new ArgumentException("Method " + method + " has multiple parameters marked with [CallingClient], but only one parameter may act as a calling client parameter.");
                            }
                            else
                            {
                                callingClientLocation = i;
                            }
                        }
                        i++;
                    }
                    if (callingClientLocation > -1)
                    {
                        methods.Add(new CallingClientMethodInvoker(method, callingClientLocation));
                    }
                    else
                    {
                        methods.Add(new RemoteMethodInvoker(method));
                    }
                }
            }
            Methods = methods.AsReadOnly();
            Properties = properties.AsReadOnly();
        }
    }
}
