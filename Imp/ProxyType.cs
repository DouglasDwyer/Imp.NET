using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DouglasDwyer.Imp
{
    public sealed class ProxyType
    {
        public ushort InterfaceID { get; }
        public Type InterfaceType { get; }
        public Type RemoteBaseType { get; }
        public IReadOnlyList<RemoteMethodInvoker> Methods { get; }
        public IReadOnlyList<PropertyInfo> Properties { get; }

        //public ProxyType(ushort id, Type mainType) : this(id, mainType, typeof(RemoteSharedObject)) { }

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
