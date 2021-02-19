using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.Imp
{
    /// <summary>
    /// Classes that implement <see cref="IProxyBinder"/> provide the ability to identify and convert between local shared types, their shared interfaces, and remote shared types.
    /// </summary>
    public interface IProxyBinder
    {
        /// <summary>
        /// Obtains the proxy data for the shared interface type with the given type ID.
        /// </summary>
        /// <param name="id">The ID of the type to identify.</param>
        /// <returns>An object containing sharing data about the type.</returns>
        //ProxyType GetDataForProxy(ushort id);
        /// <summary>
        ///  Obtains the proxy data for the given shared interface type.
        /// </summary>
        /// <param name="proxyInterface">The interface type to identify.</param>
        /// <returns>An object containing sharing data about the type.</returns>
        ProxyType GetDataForProxy(Type proxyInterface);
        /// <summary>
        /// Obtains the proxy data for the given shared class.
        /// </summary>
        /// <param name="sharedType">The shared class to identify.</param>
        /// <returns>An object containing sharing data about the type.</returns>
        ProxyType GetDataForSharedType(Type sharedType);
        /// <summary>
        /// Returns a list of all the shared proxy interfaces that this binder supports.
        /// </summary>
        /// <returns>A list of shared interfaces.</returns>
        IEnumerable<Type> GetProxyTypes();
        /// <summary>
        /// Returns the remote type with the given type ID.
        /// </summary>
        /// <param name="id">The ID of the shared interface represented by the remote type.</param>
        /// <returns>The remote type.</returns>
        //Type GetRemoteType(ushort id);
        /// <summary>
        /// Returns the remote type that represents the given shared interface.
        /// </summary>
        /// <param name="id">The interface implemented by the remote type.</param>
        /// <returns>The remote type.</returns>
        Type GetRemoteType(Type proxyInterface);
        /// <summary>
        /// Returns the shared interface represented by the given remote class.
        /// </summary>
        /// <param name="id">The remote type which implements the interface.</param>
        /// <returns>The shared interface.</returns>
        Type GetProxyForRemoteType(Type remoteClass);
        /// <summary>
        /// Returns the shared interface that represents the given local shared class.
        /// </summary>
        /// <param name="localType">The shared class which implements the interface.</param>
        /// <returns>The shared interface.</returns>
        Type GetProxyForLocalType(Type localType);
        /// <summary>
        /// Gets the type ID for the given proxy interface.
        /// </summary>
        /// <param name="proxyInterface">The interface to identify.</param>
        /// <returns>The ID of the shared interface, or null if no proxy is found.</returns>
        //ushort? GetIDForProxy(Type proxyInterface);
        /// <summary>
        /// Gets the type ID of the interface related to the given type. If the type is a local or remote shared class, then the ID of the shared interface which the type implements is returned.
        /// </summary>
        /// <param name="proxyInterface">The interface to identify.</param>
        /// <returns>The ID of the shared interface, or null if no proxy is found.</returns>
        //ushort? GetIDForSharedType(Type sharedType);
        /// <summary>
        /// Gets the type ID for the given local shared type.
        /// </summary>
        /// <param name="proxyInterface">The shared class to identify.</param>
        /// <returns>The ID of the shared interface, or null if no proxy is found.</returns>
        //ushort? GetIDForLocalType(Type localType);
        /// <summary>
        /// Gets the type ID for the given remote shared type.
        /// </summary>
        /// <param name="proxyInterface">The remote type to identify.</param>
        /// <returns>The ID of the shared interface, or null if no proxy is found.</returns>
        //ushort? GetIDForRemoteType(Type remoteType);
    }
}
