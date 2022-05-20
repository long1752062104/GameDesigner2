using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Net.System
{
    public class HashSetSafe<T> : HashSet<T>
    {
        [NonSerialized]
        private object _syncRoot;

        public object SyncRoot
        {

            get
            {
                if (_syncRoot == null)
                    Interlocked.CompareExchange<object>(ref _syncRoot, new object(), null);
                return _syncRoot;
            }
        }

        public bool Add(T item)
        {
            lock(SyncRoot)
                return base.Add(item);
        }

        public bool Remove(T item) 
        {
            lock (SyncRoot)
                return base.Remove(item);
        }
    }
}
