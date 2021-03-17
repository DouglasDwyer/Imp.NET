using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.Imp
{
    /// <summary>
    /// The base interface by which all <see cref="ImpClient"/> objects are shared across the network.
    /// </summary>
    public interface IImpClient
    {
        /// <summary>
        /// The remote server object, or local server object if this is a server-owned client.
        /// </summary>
        IImpServer Server { get; }
    }

    /// <summary>
    /// The base interface by which all <see cref="ImpClient"/> objects are shared across the network.
    /// </summary>
    /// <typeparam name="T">The type of server that this client uses.</typeparam>
    public interface IImpClient<T> : IImpClient { }
}