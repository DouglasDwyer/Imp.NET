using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.Knetworking
{
    public interface IKnetworkingClient
    {
        IKnetworkingServer Server { get; }
        ushort NetworkID { get; }

        void Disconnect();
    }
}