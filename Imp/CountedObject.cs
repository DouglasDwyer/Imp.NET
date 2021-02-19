using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace DouglasDwyer.Imp
{
    internal sealed class CountedObject<T>
    {
        public T ReferencedObject { get; }
        public int Count => InnerCount;
        private int InnerCount;
        
        public CountedObject(T obj) : this(obj, 1) { }

        public CountedObject(T obj, int initialCount)
        {
            ReferencedObject = obj;
            InnerCount = initialCount;
        }

        public void SetCount(int count)
        {
            InnerCount = count;
        }

        public static CountedObject<T> operator ++(CountedObject<T> obj)
        {
            Interlocked.Increment(ref obj.InnerCount);
            return obj;
        }

        public static CountedObject<T> operator --(CountedObject<T> obj)
        {
            Interlocked.Decrement(ref obj.InnerCount);
            return obj;
        }
    }
}
