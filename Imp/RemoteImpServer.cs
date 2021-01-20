using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.Imp
{
    [ProxyFor(typeof(IImpServer))]
    public class RemoteImpServer : RemoteSharedObject, IImpServer
    {
        public RemoteImpServer(SharedObjectPath path, ImpClient host) : base(path, host) { }
    }
}
