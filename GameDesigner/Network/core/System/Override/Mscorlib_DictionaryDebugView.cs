using System.Collections.Generic;
using System.Diagnostics;

namespace Net.System
{
    internal sealed class Mscorlib_DictionaryDebugView<K, V>
    {
        private IDictionary<K, V> dict;

        public Mscorlib_DictionaryDebugView(IDictionary<K, V> dictionary)
        {
            if (dictionary == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.dictionary);
            }
            dict = dictionary;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public KeyValuePair<K, V>[] Items
        {
            get
            {
                KeyValuePair<K, V>[] array = new KeyValuePair<K, V>[dict.Count];
                dict.CopyTo(array, 0);
                return array;
            }
        }
    }
}