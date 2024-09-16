using System.Collections;
using System.Security;

namespace Net.System
{
    public sealed class RandomizedObjectEqualityComparer : IEqualityComparer, IWellKnownStringEqualityComparer
    {
        private long _entropy;

        public RandomizedObjectEqualityComparer()
        {
            _entropy = HashHelpers.GetEntropy();
        }

        public bool Equals(object x, object y)
        {
            if (x != null)
            {
                return y != null && x.Equals(y);
            }
            return y == null;
        }

        [SecuritySafeCritical]
        public int GetHashCode(object obj)
        {
            if (obj == null)
            {
                return 0;
            }
            string text = obj as string;
            if (text != null)
            {
                return text.GetHashCode();
            }
            return obj.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            RandomizedObjectEqualityComparer randomizedObjectEqualityComparer = obj as RandomizedObjectEqualityComparer;
            return randomizedObjectEqualityComparer != null && _entropy == randomizedObjectEqualityComparer._entropy;
        }

        public override int GetHashCode()
        {
            return GetType().Name.GetHashCode() ^ (int)(_entropy & 2147483647L);
        }

        IEqualityComparer IWellKnownStringEqualityComparer.GetRandomizedEqualityComparer()
        {
            return new RandomizedObjectEqualityComparer();
        }

        IEqualityComparer IWellKnownStringEqualityComparer.GetEqualityComparerForSerialization()
        {
            return null;
        }
    }
}