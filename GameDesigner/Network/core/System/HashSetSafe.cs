using System;
using System.Collections;
using System.Collections.Generic;

namespace Net.System
{
    public class HashSetSafe<T> : HashSetEx<T>
    {
        private readonly object SyncRoot = new object();

        public HashSetSafe() : base(1) { }

        public override bool Add(T item)
        {
            lock (SyncRoot) return base.Add(item);
        }
        
        public override bool Remove(T item)
        {
            lock (SyncRoot) return base.Remove(item);
        }

        public override void Clear()
        {
            lock (SyncRoot) base.Clear();
        }

        public override int RemoveWhere(Predicate<T> match)
        {
            lock (SyncRoot) return base.RemoveWhere(match);
        }
    }
}