using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.Knetworking
{
    public class RemoteSharedObject
    {
        public readonly SharedObjectPath Location;
        public KnetworkingClient HostClient { get; protected set; }

        public RemoteSharedObject(SharedObjectPath path, KnetworkingClient host)
        {
            Location = path;
            HostClient = host;
        }
    }
}