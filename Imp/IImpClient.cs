using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.Imp
{
    public interface IImpClient
    {
        IImpServer Server { get; }
        ushort NetworkID { get; }

        void Disconnect();
    }
}