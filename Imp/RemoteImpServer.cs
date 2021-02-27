using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.Imp
{
    [ProxyFor(typeof(IImpServer))]
    public abstract class RemoteImpServer : RemoteSharedObject, IImpServer
    {
        public RemoteImpServer(ushort path, ImpClient host) : base(path, host) { }
    }
}
