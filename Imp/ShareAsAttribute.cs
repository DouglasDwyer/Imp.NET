using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace DouglasDwyer.Imp
{
    /// <summary>
    /// Indicates that a type should be shared across the network using a given interface.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class ShareAsAttribute : Attribute
    {
        /// <summary>
        /// The type that should be shared.
        /// </summary>
        public Type TypeToShare { get; private set; }
        /// <summary>
        /// The interface under which the type should be shared.
        /// </summary>
        public Type InterfaceBinding { get; private set; }

        /// <summary>
        /// Indicates that this type should be shared across the network using a given interface. 
        /// </summary>
        /// <param name="interfaceBinding">The interface under which this type should be shared.</param>
        public ShareAsAttribute(Type interfaceBinding)
        {
            InterfaceBinding = interfaceBinding;
        }

        /// <summary>
        /// Indicates that a type should be shared across the network using a given interface.
        /// </summary>
        /// <param name="typeToShare">The type that should be shared.</param>
        /// <param name="interfaceBinding">The interface under which the type should be shared.</param>
        public ShareAsAttribute(Type typeToShare, Type interfaceBinding)
        {
            TypeToShare = typeToShare;
            InterfaceBinding = interfaceBinding;
        }
    }
}