using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.Imp
{
    public class RemoteSharedObject
    {
        public readonly SharedObjectPath Location;
        public ImpClient HostClient { get; protected set; }

        public RemoteSharedObject(SharedObjectPath path, ImpClient host)
        {
            Location = path;
            HostClient = host;
        }
    }
}