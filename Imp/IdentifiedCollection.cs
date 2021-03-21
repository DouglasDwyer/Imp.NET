using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.Imp
{
    /// <summary>
    /// Represents a two-way, thread-safe hashtable between <see cref="ushort"/> keys and values of type T.
    /// </summary>
    /// <typeparam name="T">The type of object that this collection will store.</typeparam>
    public class IdentifiedCollection<T>
    {
        /// <summary>
        /// The set of keys in this collection.
        /// </summary>
        public IEnumerable<ushort> Keys => InternalDictionary.Keys;
        /// <summary>
        /// The set of values in this collection.
        /// </summary>
        public IEnumerable<T> Values => InternalDictionary.Values;
        /// <summary>
        /// The number of elements in this collection.
        /// </summary>
        public int Count => InternalDictionary.Count;

        private ConcurrentDictionary<ushort, T> InternalDictionary = new ConcurrentDictionary<ushort, T>();
        private ConcurrentDictionary<T, ushort> ReverseDictionary = new ConcurrentDictionary<T, ushort>();

        /// <summary>
        /// Obtains a value using its ID.
        /// </summary>
        /// <param name="id">The ID of the value to obtain.</param>
        /// <returns>The value with the specified ID.</returns>
        public T this[ushort id]
        {
            get => InternalDictionary[id];
        }

        /// <summary>
        /// Obtains the ID of a given value.
        /// </summary>
        /// <param name="item">The value with the ID to obtain.</param>
        /// <returns>The ID of the specified value.</returns>
        public ushort this[T item]
        {
            get => ReverseDictionary[item];
        }

        /// <summary>
        /// Adds an item to the collection.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>The ID of the object.</returns>
        public ushort Add(T item)
        {
            lock(InternalDictionary)
            {
                ushort newID = (ushort)InternalDictionary.Count;
                while(InternalDictionary.ContainsKey(newID)) { newID++; }
                InternalDictionary[newID] = item;
                ReverseDictionary[item] = newID;
                return newID;
            }
        }

        /// <summary>
        /// Adds an item to the collection, allowing for creation of the object in a thread-safe manner.
        /// </summary>
        /// <param name="itemGenerator">A function that takes in the item's ID and returns the new item.</param>
        /// <returns>The ID of the object.</returns>
        public ushort Add(Func<ushort,T> itemGenerator)
        {
            lock (InternalDictionary)
            {
                ushort newID = (ushort)InternalDictionary.Count;
                while (InternalDictionary.ContainsKey(newID)) { newID++; }
                T item = itemGenerator(newID);
                InternalDictionary[newID] = item;
                ReverseDictionary[item] = newID;
                return newID;
            }
        }

        /// <summary>
        /// Removes an object from the collection.
        /// </summary>
        /// <param name="id">The ID of the value to remove.</param>
        public void Remove(ushort id)
        {
            lock (InternalDictionary)
            {
                T removedItem;
                InternalDictionary.TryRemove(id, out removedItem);
                ReverseDictionary.TryRemove(removedItem, out _);
            }
        }

        /// <summary>
        /// Removes an object from the collection.
        /// </summary>
        /// <param name="value">The value to remove.</param>
        public void Remove(T value)
        {
            lock (InternalDictionary) {
                ushort id;
                ReverseDictionary.TryRemove(value, out id);
                InternalDictionary.TryRemove(id, out _);
            }
        }

        /// <summary>
        /// Attempts to obtain an object from the collection.
        /// </summary>
        /// <param name="id">The ID of the value to obtain.</param>
        /// <param name="value">The obtained value.</param>
        /// <returns>Whether the object was successfully obtained.</returns>
        public bool TryGetValue(ushort id, out T value)
        {
            return InternalDictionary.TryGetValue(id, out value);
        }

        /// <summary>
        /// Attempts to obtain an object from the collection.
        /// </summary>
        /// <param name="value">The value with the ID to obtain.</param>
        /// <param name="id">The ID of the given value.</param>
        /// <returns>Whether the ID of the object was successfully obtained.</returns>
        public bool TryGetID(T value, out ushort id)
        {
            return ReverseDictionary.TryGetValue(value, out id);
        }

        /// <summary>
        /// Returns whether the collection contains an object with the specified ID.
        /// </summary>
        /// <param name="id">The ID of the object to lookup.</param>
        /// <returns>Whether an object with the given ID exists in the collection.</returns>
        public bool ContainsID(ushort id)
        {
            return InternalDictionary.ContainsKey(id);
        }

        /// <summary>
        /// Returns whether the collection contains the specified object.
        /// </summary>
        /// <param name="id">The object to lookup.</param>
        /// <returns>Whether the given object exists in the collection.</returns>
        public bool ContainsValue(T value)
        {
            return ReverseDictionary.ContainsKey(value);
        }
    }
}
