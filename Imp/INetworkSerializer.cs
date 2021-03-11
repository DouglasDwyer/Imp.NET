using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.Imp
{
    public interface INetworkSerializer : ICloneable
    {
        ImpClient Client { get; set; }

        byte[] Serialize(object obj);
        object Deserialize(byte[] data);
        T Deserialize<T>(byte[] data);
    }
}
