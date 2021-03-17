using System;
using System.Collections.Generic;
using System.Security;
using System.Text;

namespace DouglasDwyer.Imp
{
    /// <summary>
    /// Acts as a proxy for an <see cref="ImpClient"/> on a remote host.
    /// </summary>
    [ProxyFor(typeof(IImpClient))]
    public abstract class RemoteImpClient : RemoteSharedObject, IImpClient
    {
        public IImpServer Server { get; private set; }

        /// <summary>
        /// Creates a new remote shared object with the given ID and host.
        /// </summary>
        /// <param name="path">The ID of the remote object.</param>
        /// <param name="host">The client that owns this object.</param>
        public RemoteImpClient(ushort path, ImpClient host) : base(path, host) {
            Server = host.Server;
        }
    }
}
