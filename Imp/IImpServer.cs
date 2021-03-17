using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.Imp
{
    /// <summary>
    /// The base interface by which all <see cref="ImpServer"/> objects are shared across the network.
    /// </summary>
    public interface IImpServer
    {
    }

    /// <summary>
    /// The base interface by which all <see cref="ImpServer{T}"/> objects are shared across the network.
    /// </summary>
    /// <typeparam name="T">The type of client that this server uses.</typeparam>
    public interface IImpServer<T> : IImpServer { }
}
