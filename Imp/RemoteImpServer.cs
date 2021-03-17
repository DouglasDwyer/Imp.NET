using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.Imp
{
    /// <summary>
    /// Acts as a proxy for an <see cref="ImpServer"/> on a remote host.
    /// </summary>
    [ProxyFor(typeof(IImpServer))]
    public abstract class RemoteImpServer : RemoteSharedObject, IImpServer
    {
        /// <summary>
        /// Creates a new remote shared object with the given ID and host.
        /// </summary>
        /// <param name="path">The ID of the remote object.</param>
        /// <param name="host">The client that owns this object.</param>
        public RemoteImpServer(ushort path, ImpClient host) : base(path, host) { }
    }
}
