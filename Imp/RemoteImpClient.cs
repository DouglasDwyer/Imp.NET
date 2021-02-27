using System;
using System.Collections.Generic;
using System.Security;
using System.Text;

namespace DouglasDwyer.Imp
{
    [ProxyFor(typeof(IImpClient))]
    public abstract class RemoteImpClient : RemoteSharedObject, IImpClient
    {
        public IImpServer Server { get; private set; }

        public RemoteImpClient(ushort path, ImpClient host) : base(path, host) {
            Server = host.Server;
        }

        public void Disconnect()
        {
            if (ObjectID == 0)
            {
                HostClient.Disconnect();
            }
            else
            {
                throw new SecurityException("Clients do not have the authority to terminate the connection of other clients.");
            }
        }
    }
}
