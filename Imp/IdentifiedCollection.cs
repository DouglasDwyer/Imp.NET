using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.Imp
{
    public class IdentifiedCollection<T>
    {
        public IEnumerable<ushort> Keys => InternalDictionary.Keys;
        public IEnumerable<T> Values => InternalDictionary.Values;

        private ConcurrentDictionary<ushort, T> InternalDictionary = new ConcurrentDictionary<ushort, T>();
        private ConcurrentDictionary<T, ushort> ReverseDictionary = new ConcurrentDictionary<T, ushort>();

        public T this[ushort id]
        {
            get => InternalDictionary[id];
        }

        public ushort this[T item]
        {
            get => ReverseDictionary[item];
        }

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

        public void Remove(ushort id)
        {
            T removedItem;
            InternalDictionary.TryRemove(id, out removedItem);
            ReverseDictionary.TryRemove(removedItem, out _);
        }

        public bool TryGetValue(ushort id, out T value)
        {
            return InternalDictionary.TryGetValue(id, out value);
        }

        public bool TryGetID(T value, out ushort id)
        {
            return ReverseDictionary.TryGetValue(value, out id);
        }

        public bool ContainsID(ushort id)
        {
            return InternalDictionary.ContainsKey(id);
        }

        public bool ContainsValue(T value)
        {
            return ReverseDictionary.ContainsKey(value);
        }
    }
}
