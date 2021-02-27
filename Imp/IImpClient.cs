using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.Imp
{
    public interface IImpClient
    {
        IImpServer Server { get; }
    }

    public interface IImpClient<T> : IImpClient { }
}