using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DouglasDwyer.Knetworking
{
    public sealed class ProxyType
    {
        public ushort InterfaceID { get; }
        public IReadOnlyList<MethodInfo> Methods { get; }
        public IReadOnlyList<PropertyInfo> Properties { get; }

        public ProxyType(ushort id, Type mainType)
        {
            InterfaceID = id;
            List<MethodInfo> methods = new List<MethodInfo>();
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
                    methods.Add(method);
                }
            }
            Methods = methods.AsReadOnly();
            Properties = properties.AsReadOnly();
        }
    }
}
