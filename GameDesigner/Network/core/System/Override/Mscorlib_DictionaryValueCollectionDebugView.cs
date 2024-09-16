using System.Collections.Generic;
using System.Diagnostics;

namespace Net.System
{
    internal sealed class Mscorlib_DictionaryValueCollectionDebugView<TKey, TValue>
    {
        private ICollection<TValue> collection;

        public Mscorlib_DictionaryValueCollectionDebugView(ICollection<TValue> collection)
        {
            if (collection == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.collection);
            }
            this.collection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public TValue[] Items
        {
            get
            {
                TValue[] array = new TValue[collection.Count];
                collection.CopyTo(array, 0);
                return array;
            }
        }
    }
}