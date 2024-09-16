using System.Collections;
using System.Collections.Generic;

namespace Net.System
{
    public sealed class RandomizedStringEqualityComparer : IEqualityComparer<string>, IEqualityComparer, IWellKnownStringEqualityComparer
    {
        private long _entropy;

        public RandomizedStringEqualityComparer()
        {
            _entropy = HashHelpers.GetEntropy();
        }

        public bool Equals(object x, object y)
        {
            if (x == y)
            {
                return true;
            }
            if (x == null || y == null)
            {
                return false;
            }
            if (x is string && y is string)
            {
                return Equals((string)x, (string)y);
            }
            ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArgumentForComparison);
            return false;
        }

        public bool Equals(string x, string y)
        {
            if (x != null)
            {
                return y != null && x.Equals(y);
            }
            return y == null;
        }

        public int GetHashCode(string obj)
        {
            if (obj == null)
            {
                return 0;
            }
            return obj.GetHashCode();
        }

        public int GetHashCode(object obj)
        {
            if (obj == null)
            {
                return 0;
            }
            return obj.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            RandomizedStringEqualityComparer randomizedStringEqualityComparer = obj as RandomizedStringEqualityComparer;
            return randomizedStringEqualityComparer != null && _entropy == randomizedStringEqualityComparer._entropy;
        }

        public override int GetHashCode()
        {
            return GetType().Name.GetHashCode() ^ (int)(_entropy & 2147483647L);
        }

        IEqualityComparer IWellKnownStringEqualityComparer.GetRandomizedEqualityComparer()
        {
            return new RandomizedStringEqualityComparer();
        }

        IEqualityComparer IWellKnownStringEqualityComparer.GetEqualityComparerForSerialization()
        {
            return EqualityComparer<string>.Default;
        }
    }
}