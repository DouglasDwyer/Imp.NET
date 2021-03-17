using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.Imp
{
    /// <summary>
    /// Represents a serializer that can be utilized send shared objects across the network, preserving their reference identities.
    /// </summary>
    public interface INetworkSerializer : ICloneable
    {
        /// <summary>
        /// The client to which this serializer belongs.
        /// </summary>
        ImpClient Client { get; set; }

        /// <summary>
        /// Serializes an object to a byte array.
        /// </summary>
        /// <param name="obj">The object to serialize.</param>
        /// <returns>A byte-based representation of the object.</returns>
        byte[] Serialize(object obj);
        /// <summary>
        /// Deserializes an object from a byte array.
        /// </summary>
        /// <param name="data">The byte-based representation of the object.</param>
        /// <returns>The deserialized object.</returns>
        object Deserialize(byte[] data);
        /// <summary>
        /// Deserializes an object from a byte array.
        /// </summary>
        /// <typeparam name="T">The type of the deserialized object.</typeparam>
        /// <param name="data">The byte-based representation of the object.</param>
        /// <returns>The deserialized object.</returns>
        T Deserialize<T>(byte[] data);
    }
}