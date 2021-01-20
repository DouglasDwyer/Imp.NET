using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.Knetworking
{
    [ProxyFor(typeof(IKnetworkingServer))]
    public class RemoteKnetworkingServer : RemoteSharedObject, IKnetworkingServer
    {
        public RemoteKnetworkingServer(SharedObjectPath path, KnetworkingClient host) : base(path, host) { }
    }
}
