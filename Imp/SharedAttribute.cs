using System;

namespace DouglasDwyer.Imp
{
    /// <summary>
    /// Indicates that this type should be shared across the network and that an interface for sharing should be automatically generated. If an interface for sharing already exists, use <see cref="ShareAsAttribute"/> instead.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class SharedAttribute : Attribute
    {
        /// <summary>
        /// Indicates that this type should be shared across the network and that an interface for sharing should be automatically generated. If an interface for sharing already exists, use <see cref="ShareAsAttribute"/> instead.
        /// </summary>
        public SharedAttribute() { }
        /// <summary>
        /// Indicates that this type should be shared across the network and that an interface for sharing should be automatically generated. If an interface for sharing already exists, use <see cref="ShareAsAttribute"/> instead.
        /// </summary>
        /// <param name="interfaceName">The namespace-qualified name of the interface to generate.</param>
        public SharedAttribute(string interfaceName) { }
    }
}
