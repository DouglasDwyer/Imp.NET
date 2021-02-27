using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.Imp
{
    /// <summary>
    /// This is the base class for all interface implementations that represent an object on a remote host. Custom remote interface implementations should inherit from this class.
    /// </summary>
    public abstract class RemoteSharedObject
    {
        /// <summary>
        /// The ID of this object used to identify on the remote host.
        /// </summary>
        public ushort ObjectID { get; internal set; }
        /// <summary>
        /// The client that owns this object.
        /// </summary>
        public ImpClient HostClient { get; internal set; }

        /// <summary>
        /// Creates a new remote shared object with the given ID and host.
        /// </summary>
        /// <param name="path">The ID of the remote object.</param>
        /// <param name="host">The client that owns this object.</param>
        public RemoteSharedObject(ushort path, ImpClient host)
        {
            ObjectID = path;
            HostClient = host;
        }

        public override string ToString()
        {
            string name = GetType().Name;
            if(name.Length > 16)
            {
                name = name.Remove(name.Length - 32);
            }
            return name + "(" + HostClient.NetworkID + "," + ObjectID + ")";
        }

        ~RemoteSharedObject()
        {
            try
            {
                HostClient.ReleaseRemoteSharedObject(ObjectID);
            }
            catch(Exception e) {
                Console.WriteLine(e);
            }
        }
    }
}