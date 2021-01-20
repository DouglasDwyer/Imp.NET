using System;
using System.Collections.Generic;
using System.Security;
using System.Text;

namespace DouglasDwyer.Knetworking
{
    [ProxyFor(typeof(IKnetworkingClient))]
    public class RemoteKnetworkingClient : RemoteSharedObject, IKnetworkingClient
    {
        public IKnetworkingServer Server { get; private set; }
        public ushort NetworkID => Location.OwnerID;

        public RemoteKnetworkingClient(SharedObjectPath path, KnetworkingClient host) : base(path, host) {
            Server = host.Server;
        }

        public void Disconnect()
        {
            if (NetworkID == HostClient.NetworkID)
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
