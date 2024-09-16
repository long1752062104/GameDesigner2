using System.Collections;

namespace Net.System
{
    public interface IWellKnownStringEqualityComparer
    {
        IEqualityComparer GetRandomizedEqualityComparer();
        IEqualityComparer GetEqualityComparerForSerialization();
    }
}