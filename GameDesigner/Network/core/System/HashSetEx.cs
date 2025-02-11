﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
#if !SILVERLIGHT
using System.Runtime.Serialization;
#endif
using System.Security.Permissions;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using System.Security;
using Net.System;
#if SILVERLIGHT
using System.Core; // for System.Core.SR
#endif

namespace Net.System
{
    /// <summary>
    /// Debug view for HashSetEx
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class HashSetExDebugView<T>
    {
        private HashSetEx<T> set;

        public HashSetExDebugView(HashSetEx<T> set)
        {
            if (set == null)
            {
                throw new ArgumentNullException("set");
            }

            this.set = set;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get
            {
                return set.ToArray();
            }
        }
    }

    /// <summary>
    /// Implementation notes:
    /// This uses an array-based implementation similar to Dictionary<T>, using a buckets array
    /// to map hash values to the Slots array. Items in the Slots array that hash to the same value
    /// are chained together through the "next" indices. 
    /// 
    /// The capacity is always prime; so during resizing, the capacity is chosen as the next prime
    /// greater than double the last capacity. 
    /// 
    /// The underlying data structures are lazily initialized. Because of the observation that, 
    /// in practice, hashtables tend to contain only a few elements, the initial capacity is
    /// set very small (3 elements) unless the ctor with a collection is used.
    /// 
    /// The +/- 1 modifications in methods that add, check for containment, etc allow us to 
    /// distinguish a hash code of 0 from an uninitialized bucket. This saves us from having to 
    /// reset each bucket to -1 when resizing. See Contains, for example.
    /// 
    /// Set methods such as UnionWith, IntersectWith, ExceptWith, and SymmetricExceptWith modify
    /// this set.
    /// 
    /// Some operations can perform faster if we can assume "other" contains unique elements
    /// according to this equality comparer. The only times this is efficient to check is if
    /// other is a hashset. Note that checking that it's a hashset alone doesn't suffice; we
    /// also have to check that the hashset is using the same equality comparer. If other 
    /// has a different equality comparer, it will have unique elements according to its own
    /// equality comparer, but not necessarily according to ours. Therefore, to go these 
    /// optimized routes we check that other is a hashset using the same equality comparer.
    /// 
    /// A HashSetEx with no elements has the properties of the empty set. (See IsSubset, etc. for 
    /// special empty set checks.)
    /// 
    /// A couple of methods have a special case if other is this (e.g. SymmetricExceptWith). 
    /// If we didn't have these checks, we could be iterating over the set and modifying at
    /// the same time. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DebuggerTypeProxy(typeof(HashSetExDebugView<>))]
    [DebuggerDisplay("Count = {Count}")]
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "By design")]
#if SILVERLIGHT
    public class HashSetEx<T> : ICollection<T>, ISet<T>, IReadOnlyCollection<T>
#else
    [Serializable()]
    public class HashSetEx<T> : ICollection<T>, ISet<T>, IReadOnlyCollection<T>
#endif
    {

        // store lower 31 bits of hash code
        private const int Lower31BitMask = 0x7FFFFFFF;
        // cutoff point, above which we won't do stackallocs. This corresponds to 100 integers.
        private const int StackAllocThreshold = 100;
        // when constructing a hashset from an existing collection, it may contain duplicates, 
        // so this is used as the max acceptable excess ratio of capacity to count. Note that
        // this is only used on the ctor and not to automatically shrink if the hashset has, e.g,
        // a lot of adds followed by removes. Users must explicitly shrink by calling TrimExcess.
        // This is set to 3 because capacity is acceptable as 2x rounded up to nearest prime.
        private const int ShrinkThreshold = 3;

#if !SILVERLIGHT
        // constants for serialization
        private const String CapacityName = "Capacity";
        private const String ElementsName = "Elements";
        private const String ComparerName = "Comparer";
        private const String VersionName = "Version";
#endif

        private int[] m_buckets;
        private Slot[] m_slots;
        private int m_count;
        private int m_lastIndex;
        private int m_freeList;
        private IEqualityComparer<T> m_comparer;

#if !SILVERLIGHT
        // temporary variable needed during deserialization
        private SerializationInfo m_siInfo;
#endif

        #region Constructors

        public HashSetEx()
            : this(EqualityComparer<T>.Default) { }

        public HashSetEx(int capacity)
            : this(capacity, EqualityComparer<T>.Default) { }

        public HashSetEx(IEqualityComparer<T> comparer)
        {
            if (comparer == null)
            {
                comparer = EqualityComparer<T>.Default;
            }

            this.m_comparer = comparer;
            m_lastIndex = 0;
            m_count = 0;
            m_freeList = -1;
        }

        public HashSetEx(IEnumerable<T> collection)
            : this(collection, EqualityComparer<T>.Default) { }

        /// <summary>
        /// Implementation Notes:
        /// Since resizes are relatively expensive (require rehashing), this attempts to minimize 
        /// the need to resize by setting the initial capacity based on size of collection. 
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="comparer"></param>
        public HashSetEx(IEnumerable<T> collection, IEqualityComparer<T> comparer)
            : this(comparer)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            Contract.EndContractBlock();

            var otherAsHashSetEx = collection as HashSetEx<T>;
            if (otherAsHashSetEx != null && AreEqualityComparersEqual(this, otherAsHashSetEx))
            {
                CopyFrom(otherAsHashSetEx);
            }
            else
            {
                // to avoid excess resizes, first set size based on collection's count. Collection
                // may contain duplicates, so call TrimExcess if resulting hashset is larger than
                // threshold
                ICollection<T> coll = collection as ICollection<T>;
                int suggestedCapacity = coll == null ? 0 : coll.Count;
                Initialize(suggestedCapacity);

                this.UnionWith(collection);

                if (m_count > 0 && m_slots.Length / m_count > ShrinkThreshold)
                {
                    TrimExcess();
                }
            }
        }

        // Initializes the HashSetEx from another HashSetEx with the same element type and
        // equality comparer.
        private void CopyFrom(HashSetEx<T> source)
        {
            int count = source.m_count;
            if (count == 0)
            {
                // As well as short-circuiting on the rest of the work done,
                // this avoids errors from trying to access otherAsHashSetEx.m_buckets
                // or otherAsHashSetEx.m_slots when they aren't initialized.
                return;
            }

            int capacity = source.m_buckets.Length;
            int threshold = HashHelpers.ExpandPrime(count + 1);

            if (threshold >= capacity)
            {
                m_buckets = (int[])source.m_buckets.Clone();
                m_slots = (Slot[])source.m_slots.Clone();

                m_lastIndex = source.m_lastIndex;
                m_freeList = source.m_freeList;
            }
            else
            {
                int lastIndex = source.m_lastIndex;
                Slot[] slots = source.m_slots;
                Initialize(count);
                int index = 0;
                for (int i = 0; i < lastIndex; ++i)
                {
                    int hashCode = slots[i].hashCode;
                    if (hashCode >= 0)
                    {
                        AddValue(index, hashCode, slots[i].value);
                        ++index;
                    }
                }
                Debug.Assert(index == count);
                m_lastIndex = index;
            }
            m_count = count;
        }

#if !SILVERLIGHT
        protected HashSetEx(SerializationInfo info, StreamingContext context)
        {
            // We can't do anything with the keys and values until the entire graph has been 
            // deserialized and we have a reasonable estimate that GetHashCode is not going to 
            // fail.  For the time being, we'll just cache this.  The graph is not valid until 
            // OnDeserialization has been called.
            m_siInfo = info;
        }
#endif

        public HashSetEx(int capacity, IEqualityComparer<T> comparer)
            : this(comparer)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException("capacity");
            }
            Contract.EndContractBlock();

            if (capacity > 0)
            {
                Initialize(capacity);
            }
        }

        #endregion

        #region ICollection<T> methods

        /// <summary>
        /// Add item to this hashset. This is the explicit implementation of the ICollection<T>
        /// interface. The other Add method returns bool indicating whether item was added.
        /// </summary>
        /// <param name="item">item to add</param>
        void ICollection<T>.Add(T item)
        {
            AddIfNotPresent(item);
        }

        /// <summary>
        /// Remove all items from this set. This clears the elements but not the underlying 
        /// buckets and slots array. Follow this call by TrimExcess to release these.
        /// </summary>
        public virtual void Clear()
        {
            if (m_lastIndex > 0)
            {
                Debug.Assert(m_buckets != null, "m_buckets was null but m_lastIndex > 0");

                // clear the elements so that the gc can reclaim the references.
                // clear only up to m_lastIndex for m_slots 
                Array.Clear(m_slots, 0, m_lastIndex);
                Array.Clear(m_buckets, 0, m_buckets.Length);
                m_lastIndex = 0;
                m_count = 0;
                m_freeList = -1;
            }
        }

        /// <summary>
        /// Checks if this hashset contains the item
        /// </summary>
        /// <param name="item">item to check for containment</param>
        /// <returns>true if item contained; false if not</returns>
        public bool Contains(T item)
        {
            if (m_buckets != null)
            {
                int hashCode = InternalGetHashCode(item);
                // see note at "HashSetEx" level describing why "- 1" appears in for loop
                for (int i = m_buckets[hashCode % m_buckets.Length] - 1; i >= 0; i = m_slots[i].next)
                {
                    if (m_slots[i].hashCode == hashCode && m_comparer.Equals(m_slots[i].value, item))
                    {
                        return true;
                    }
                }
            }
            // either m_buckets is null or wasn't found
            return false;
        }

        /// <summary>
        /// Copy items in this hashset to array, starting at arrayIndex
        /// </summary>
        /// <param name="array">array to add items to</param>
        /// <param name="arrayIndex">index to start at</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            CopyTo(array, arrayIndex, m_count);
        }

        /// <summary>
        /// Remove item from this hashset
        /// </summary>
        /// <param name="item">item to remove</param>
        /// <returns>true if removed; false if not (i.e. if the item wasn't in the HashSetEx)</returns>
        public virtual bool Remove(T item)
        {
            if (m_buckets != null)
            {
                int hashCode = InternalGetHashCode(item);
                int bucket = hashCode % m_buckets.Length;
                int last = -1;
                for (int i = m_buckets[bucket] - 1; i >= 0; last = i, i = m_slots[i].next)
                {
                    if (m_slots[i].hashCode == hashCode && m_comparer.Equals(m_slots[i].value, item))
                    {
                        if (last < 0)
                        {
                            // first iteration; update buckets
                            m_buckets[bucket] = m_slots[i].next + 1;
                        }
                        else
                        {
                            // subsequent iterations; update 'next' pointers
                            m_slots[last].next = m_slots[i].next;
                        }
                        m_slots[i].hashCode = -1;
                        m_slots[i].value = default(T);
                        m_slots[i].next = m_freeList;

                        m_count--;
                        if (m_count == 0)
                        {
                            m_lastIndex = 0;
                            m_freeList = -1;
                        }
                        else
                        {
                            m_freeList = i;
                        }
                        return true;
                    }
                }
            }
            // either m_buckets is null or wasn't found
            return false;
        }

        /// <summary>
        /// Number of elements in this hashset
        /// </summary>
        public int Count
        {
            get { return m_count; }
        }

        /// <summary>
        /// Whether this is readonly
        /// </summary>
        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }

        #endregion

        #region IEnumerable methods

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        #endregion

        #region HashSetEx methods

        /// <summary>
        /// Add item to this HashSetEx. Returns bool indicating whether item was added (won't be 
        /// added if already present)
        /// </summary>
        /// <param name="item"></param>
        /// <returns>true if added, false if already present</returns>
        public virtual bool Add(T item)
        {
            return AddIfNotPresent(item);
        }

        /// <summary>
        /// Searches the set for a given value and returns the equal value it finds, if any.
        /// </summary>
        /// <param name="equalValue">The value to search for.</param>
        /// <param name="actualValue">The value from the set that the search found, or the default value of <typeparamref name="T"/> when the search yielded no match.</param>
        /// <returns>A value indicating whether the search was successful.</returns>
        /// <remarks>
        /// This can be useful when you want to reuse a previously stored reference instead of 
        /// a newly constructed one (so that more sharing of references can occur) or to look up
        /// a value that has more complete data than the value you currently have, although their
        /// comparer functions indicate they are equal.
        /// </remarks>
        public bool TryGetValue(T equalValue, out T actualValue)
        {
            if (m_buckets != null)
            {
                int i = InternalIndexOf(equalValue);
                if (i >= 0)
                {
                    actualValue = m_slots[i].value;
                    return true;
                }
            }
            actualValue = default(T);
            return false;
        }

        /// <summary>
        /// Take the union of this HashSetEx with other. Modifies this set.
        /// 
        /// Implementation note: GetSuggestedCapacity (to increase capacity in advance avoiding 
        /// multiple resizes ended up not being useful in practice; quickly gets to the 
        /// point where it's a wasteful check.
        /// </summary>
        /// <param name="other">enumerable with items to add</param>
        public void UnionWith(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }
            Contract.EndContractBlock();

            foreach (T item in other)
            {
                AddIfNotPresent(item);
            }
        }

        /// <summary>
        /// Takes the intersection of this set with other. Modifies this set.
        /// 
        /// Implementation Notes: 
        /// We get better perf if other is a hashset using same equality comparer, because we 
        /// get constant contains check in other. Resulting cost is O(n1) to iterate over this.
        /// 
        /// If we can't go above route, iterate over the other and mark intersection by checking
        /// contains in this. Then loop over and delete any unmarked elements. Total cost is n2+n1. 
        /// 
        /// Attempts to return early based on counts alone, using the property that the 
        /// intersection of anything with the empty set is the empty set.
        /// </summary>
        /// <param name="other">enumerable with items to add </param>
        public void IntersectWith(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }
            Contract.EndContractBlock();

            // intersection of anything with empty set is empty set, so return if count is 0
            if (m_count == 0)
            {
                return;
            }

            // if other is empty, intersection is empty set; remove all elements and we're done
            // can only figure this out if implements ICollection<T>. (IEnumerable<T> has no count)
            ICollection<T> otherAsCollection = other as ICollection<T>;
            if (otherAsCollection != null)
            {
                if (otherAsCollection.Count == 0)
                {
                    Clear();
                    return;
                }

                HashSetEx<T> otherAsSet = other as HashSetEx<T>;
                // faster if other is a hashset using same equality comparer; so check 
                // that other is a hashset using the same equality comparer.
                if (otherAsSet != null && AreEqualityComparersEqual(this, otherAsSet))
                {
                    IntersectWithHashSetExWithSameEC(otherAsSet);
                    return;
                }
            }

            IntersectWithEnumerable(other);
        }

        /// <summary>
        /// Remove items in other from this set. Modifies this set.
        /// </summary>
        /// <param name="other">enumerable with items to remove</param>
        public void ExceptWith(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }
            Contract.EndContractBlock();

            // this is already the enpty set; return
            if (m_count == 0)
            {
                return;
            }

            // special case if other is this; a set minus itself is the empty set
            if (other == this)
            {
                Clear();
                return;
            }

            // remove every element in other from this
            foreach (T element in other)
            {
                Remove(element);
            }
        }

        /// <summary>
        /// Takes symmetric difference (XOR) with other and this set. Modifies this set.
        /// </summary>
        /// <param name="other">enumerable with items to XOR</param>
        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }
            Contract.EndContractBlock();

            // if set is empty, then symmetric difference is other
            if (m_count == 0)
            {
                UnionWith(other);
                return;
            }

            // special case this; the symmetric difference of a set with itself is the empty set
            if (other == this)
            {
                Clear();
                return;
            }

            HashSetEx<T> otherAsSet = other as HashSetEx<T>;
            // If other is a HashSetEx, it has unique elements according to its equality comparer,
            // but if they're using different equality comparers, then assumption of uniqueness
            // will fail. So first check if other is a hashset using the same equality comparer;
            // symmetric except is a lot faster and avoids bit array allocations if we can assume
            // uniqueness
            if (otherAsSet != null && AreEqualityComparersEqual(this, otherAsSet))
            {
                SymmetricExceptWithUniqueHashSetEx(otherAsSet);
            }
            else
            {
                SymmetricExceptWithEnumerable(other);
            }
        }

        /// <summary>
        /// Checks if this is a subset of other.
        /// 
        /// Implementation Notes:
        /// The following properties are used up-front to avoid element-wise checks:
        /// 1. If this is the empty set, then it's a subset of anything, including the empty set
        /// 2. If other has unique elements according to this equality comparer, and this has more
        /// elements than other, then it can't be a subset.
        /// 
        /// Furthermore, if other is a hashset using the same equality comparer, we can use a 
        /// faster element-wise check.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>true if this is a subset of other; false if not</returns>
        public bool IsSubsetOf(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }
            Contract.EndContractBlock();

            // The empty set is a subset of any set
            if (m_count == 0)
            {
                return true;
            }

            HashSetEx<T> otherAsSet = other as HashSetEx<T>;
            // faster if other has unique elements according to this equality comparer; so check 
            // that other is a hashset using the same equality comparer.
            if (otherAsSet != null && AreEqualityComparersEqual(this, otherAsSet))
            {
                // if this has more elements then it can't be a subset
                if (m_count > otherAsSet.Count)
                {
                    return false;
                }

                // already checked that we're using same equality comparer. simply check that 
                // each element in this is contained in other.
                return IsSubsetOfHashSetExWithSameEC(otherAsSet);
            }
            else
            {
                ElementCount result = CheckUniqueAndUnfoundElements(other, false);
                return (result.uniqueCount == m_count && result.unfoundCount >= 0);
            }
        }

        /// <summary>
        /// Checks if this is a proper subset of other (i.e. strictly contained in)
        /// 
        /// Implementation Notes:
        /// The following properties are used up-front to avoid element-wise checks:
        /// 1. If this is the empty set, then it's a proper subset of a set that contains at least
        /// one element, but it's not a proper subset of the empty set.
        /// 2. If other has unique elements according to this equality comparer, and this has >=
        /// the number of elements in other, then this can't be a proper subset.
        /// 
        /// Furthermore, if other is a hashset using the same equality comparer, we can use a 
        /// faster element-wise check.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>true if this is a proper subset of other; false if not</returns>
        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }
            Contract.EndContractBlock();

            ICollection<T> otherAsCollection = other as ICollection<T>;
            if (otherAsCollection != null)
            {
                // the empty set is a proper subset of anything but the empty set
                if (m_count == 0)
                {
                    return otherAsCollection.Count > 0;
                }
                HashSetEx<T> otherAsSet = other as HashSetEx<T>;
                // faster if other is a hashset (and we're using same equality comparer)
                if (otherAsSet != null && AreEqualityComparersEqual(this, otherAsSet))
                {
                    if (m_count >= otherAsSet.Count)
                    {
                        return false;
                    }
                    // this has strictly less than number of items in other, so the following
                    // check suffices for proper subset.
                    return IsSubsetOfHashSetExWithSameEC(otherAsSet);
                }
            }

            ElementCount result = CheckUniqueAndUnfoundElements(other, false);
            return (result.uniqueCount == m_count && result.unfoundCount > 0);

        }

        /// <summary>
        /// Checks if this is a superset of other
        /// 
        /// Implementation Notes:
        /// The following properties are used up-front to avoid element-wise checks:
        /// 1. If other has no elements (it's the empty set), then this is a superset, even if this
        /// is also the empty set.
        /// 2. If other has unique elements according to this equality comparer, and this has less 
        /// than the number of elements in other, then this can't be a superset
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns>true if this is a superset of other; false if not</returns>
        public bool IsSupersetOf(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }
            Contract.EndContractBlock();

            // try to fall out early based on counts
            ICollection<T> otherAsCollection = other as ICollection<T>;
            if (otherAsCollection != null)
            {
                // if other is the empty set then this is a superset
                if (otherAsCollection.Count == 0)
                {
                    return true;
                }
                HashSetEx<T> otherAsSet = other as HashSetEx<T>;
                // try to compare based on counts alone if other is a hashset with
                // same equality comparer
                if (otherAsSet != null && AreEqualityComparersEqual(this, otherAsSet))
                {
                    if (otherAsSet.Count > m_count)
                    {
                        return false;
                    }
                }
            }

            return ContainsAllElements(other);
        }

        /// <summary>
        /// Checks if this is a proper superset of other (i.e. other strictly contained in this)
        /// 
        /// Implementation Notes: 
        /// This is slightly more complicated than above because we have to keep track if there
        /// was at least one element not contained in other.
        /// 
        /// The following properties are used up-front to avoid element-wise checks:
        /// 1. If this is the empty set, then it can't be a proper superset of any set, even if 
        /// other is the empty set.
        /// 2. If other is an empty set and this contains at least 1 element, then this is a proper
        /// superset.
        /// 3. If other has unique elements according to this equality comparer, and other's count
        /// is greater than or equal to this count, then this can't be a proper superset
        /// 
        /// Furthermore, if other has unique elements according to this equality comparer, we can
        /// use a faster element-wise check.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>true if this is a proper superset of other; false if not</returns>
        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }
            Contract.EndContractBlock();

            // the empty set isn't a proper superset of any set.
            if (m_count == 0)
            {
                return false;
            }

            ICollection<T> otherAsCollection = other as ICollection<T>;
            if (otherAsCollection != null)
            {
                // if other is the empty set then this is a superset
                if (otherAsCollection.Count == 0)
                {
                    // note that this has at least one element, based on above check
                    return true;
                }
                HashSetEx<T> otherAsSet = other as HashSetEx<T>;
                // faster if other is a hashset with the same equality comparer
                if (otherAsSet != null && AreEqualityComparersEqual(this, otherAsSet))
                {
                    if (otherAsSet.Count >= m_count)
                    {
                        return false;
                    }
                    // now perform element check
                    return ContainsAllElements(otherAsSet);
                }
            }
            // couldn't fall out in the above cases; do it the long way
            ElementCount result = CheckUniqueAndUnfoundElements(other, true);
            return (result.uniqueCount < m_count && result.unfoundCount == 0);

        }

        /// <summary>
        /// Checks if this set overlaps other (i.e. they share at least one item)
        /// </summary>
        /// <param name="other"></param>
        /// <returns>true if these have at least one common element; false if disjoint</returns>
        public bool Overlaps(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }
            Contract.EndContractBlock();

            if (m_count == 0)
            {
                return false;
            }

            foreach (T element in other)
            {
                if (Contains(element))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if this and other contain the same elements. This is set equality: 
        /// duplicates and order are ignored
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool SetEquals(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }
            Contract.EndContractBlock();

            HashSetEx<T> otherAsSet = other as HashSetEx<T>;
            // faster if other is a hashset and we're using same equality comparer
            if (otherAsSet != null && AreEqualityComparersEqual(this, otherAsSet))
            {
                // attempt to return early: since both contain unique elements, if they have 
                // different counts, then they can't be equal
                if (m_count != otherAsSet.Count)
                {
                    return false;
                }

                // already confirmed that the sets have the same number of distinct elements, so if
                // one is a superset of the other then they must be equal
                return ContainsAllElements(otherAsSet);
            }
            else
            {
                ICollection<T> otherAsCollection = other as ICollection<T>;
                if (otherAsCollection != null)
                {
                    // if this count is 0 but other contains at least one element, they can't be equal
                    if (m_count == 0 && otherAsCollection.Count > 0)
                    {
                        return false;
                    }
                }
                ElementCount result = CheckUniqueAndUnfoundElements(other, true);
                return (result.uniqueCount == m_count && result.unfoundCount == 0);
            }
        }

        public void CopyTo(T[] array) { CopyTo(array, 0, m_count); }

        public void CopyTo(T[] array, int arrayIndex, int count)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            Contract.EndContractBlock();

            // check array index valid index into array
            if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException("arrayIndex");
            }

            // also throw if count less than 0
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            // will array, starting at arrayIndex, be able to hold elements? Note: not
            // checking arrayIndex >= array.Length (consistency with list of allowing
            // count of 0; subsequent check takes care of the rest)
            if (arrayIndex > array.Length || count > array.Length - arrayIndex)
            {
                throw new ArgumentException();
            }

            int numCopied = 0;
            for (int i = 0; i < m_lastIndex && numCopied < count; i++)
            {
                if (m_slots[i].hashCode >= 0)
                {
                    array[arrayIndex + numCopied] = m_slots[i].value;
                    numCopied++;
                }
            }
        }

        /// <summary>
        /// Remove elements that match specified predicate. Returns the number of elements removed
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public virtual int RemoveWhere(Predicate<T> match)
        {
            if (match == null)
            {
                throw new ArgumentNullException("match");
            }
            Contract.EndContractBlock();

            int numRemoved = 0;
            for (int i = 0; i < m_lastIndex; i++)
            {
                if (m_slots[i].hashCode >= 0)
                {
                    // cache value in case delegate removes it
                    T value = m_slots[i].value;
                    if (match(value))
                    {
                        // check again that remove actually removed it
                        if (Remove(value))
                        {
                            numRemoved++;
                        }
                    }
                }
            }
            return numRemoved;
        }

        /// <summary>
        /// Gets the IEqualityComparer that is used to determine equality of keys for 
        /// the HashSetEx.
        /// </summary>
        public IEqualityComparer<T> Comparer
        {
            get
            {
                return m_comparer;
            }
        }

        /// <summary>
        /// Sets the capacity of this list to the size of the list (rounded up to nearest prime),
        /// unless count is 0, in which case we release references.
        /// 
        /// This method can be used to minimize a list's memory overhead once it is known that no
        /// new elements will be added to the list. To completely clear a list and release all 
        /// memory referenced by the list, execute the following statements:
        /// 
        /// list.Clear();
        /// list.TrimExcess(); 
        /// </summary>
        public void TrimExcess()
        {
            Debug.Assert(m_count >= 0, "m_count is negative");

            if (m_count == 0)
            {
                // if count is zero, clear references
                m_buckets = null;
                m_slots = null;
            }
            else
            {
                Debug.Assert(m_buckets != null, "m_buckets was null but m_count > 0");

                // similar to IncreaseCapacity but moves down elements in case add/remove/etc
                // caused fragmentation
                int newSize = HashHelpers.GetPrime(m_count);
                Slot[] newSlots = new Slot[newSize];
                int[] newBuckets = new int[newSize];

                // move down slots and rehash at the same time. newIndex keeps track of current 
                // position in newSlots array
                int newIndex = 0;
                for (int i = 0; i < m_lastIndex; i++)
                {
                    if (m_slots[i].hashCode >= 0)
                    {
                        newSlots[newIndex] = m_slots[i];

                        // rehash
                        int bucket = newSlots[newIndex].hashCode % newSize;
                        newSlots[newIndex].next = newBuckets[bucket] - 1;
                        newBuckets[bucket] = newIndex + 1;

                        newIndex++;
                    }
                }

                Debug.Assert(newSlots.Length <= m_slots.Length, "capacity increased after TrimExcess");

                m_lastIndex = newIndex;
                m_slots = newSlots;
                m_buckets = newBuckets;
                m_freeList = -1;
            }
        }

#if !SILVERLIGHT || FEATURE_NETCORE
        /// <summary>
        /// Used for deep equality of HashSetEx testing
        /// </summary>
        /// <returns></returns>
        public static IEqualityComparer<HashSetEx<T>> CreateSetComparer()
        {
            return new HashSetExEqualityComparer<T>();
        }
#endif

        #endregion

        #region Helper methods

        /// <summary>
        /// Initializes buckets and slots arrays. Uses suggested capacity by finding next prime
        /// greater than or equal to capacity.
        /// </summary>
        /// <param name="capacity"></param>
        private void Initialize(int capacity)
        {
            Debug.Assert(m_buckets == null, "Initialize was called but m_buckets was non-null");

            int size = HashHelpers.GetPrime(capacity);

            m_buckets = new int[size];
            m_slots = new Slot[size];
        }

        /// <summary>
        /// Expand to new capacity. New capacity is next prime greater than or equal to suggested 
        /// size. This is called when the underlying array is filled. This performs no 
        /// defragmentation, allowing faster execution; note that this is reasonable since 
        /// AddIfNotPresent attempts to insert new elements in re-opened spots.
        /// </summary>
        /// <param name="sizeSuggestion"></param>
        private void IncreaseCapacity()
        {
            Debug.Assert(m_buckets != null, "IncreaseCapacity called on a set with no elements");

            int newSize = HashHelpers.ExpandPrime(m_count);
            if (newSize <= m_count)
            {
                throw new ArgumentException();
            }

            // Able to increase capacity; copy elements to larger array and rehash
            SetCapacity(newSize, false);
        }

        /// <summary>
        /// Set the underlying buckets array to size newSize and rehash.  Note that newSize
        /// *must* be a prime.  It is very likely that you want to call IncreaseCapacity()
        /// instead of this method.
        /// </summary>
        private void SetCapacity(int newSize, bool forceNewHashCodes)
        {
            Contract.Assert(HashHelpers.IsPrime(newSize), "New size is not prime!");

            Contract.Assert(m_buckets != null, "SetCapacity called on a set with no elements");

            Slot[] newSlots = new Slot[newSize];
            if (m_slots != null)
            {
                Array.Copy(m_slots, 0, newSlots, 0, m_lastIndex);
            }

            if (forceNewHashCodes)
            {
                for (int i = 0; i < m_lastIndex; i++)
                {
                    if (newSlots[i].hashCode != -1)
                    {
                        newSlots[i].hashCode = InternalGetHashCode(newSlots[i].value);
                    }
                }
            }

            int[] newBuckets = new int[newSize];
            for (int i = 0; i < m_lastIndex; i++)
            {
                int bucket = newSlots[i].hashCode % newSize;
                newSlots[i].next = newBuckets[bucket] - 1;
                newBuckets[bucket] = i + 1;
            }
            m_slots = newSlots;
            m_buckets = newBuckets;
        }

        /// <summary>
        /// Adds value to HashSetEx if not contained already
        /// Returns true if added and false if already present
        /// </summary>
        /// <param name="value">value to find</param>
        /// <returns></returns>
        private bool AddIfNotPresent(T value)
        {
            if (m_buckets == null)
            {
                Initialize(0);
            }

            int hashCode = InternalGetHashCode(value);
            int bucket = hashCode % m_buckets.Length;
#if FEATURE_RANDOMIZED_STRING_HASHING && !FEATURE_NETCORE
            int collisionCount = 0;
#endif
            for (int i = m_buckets[hashCode % m_buckets.Length] - 1; i >= 0; i = m_slots[i].next)
            {
                if (m_slots[i].hashCode == hashCode && m_comparer.Equals(m_slots[i].value, value))
                {
                    return false;
                }
#if FEATURE_RANDOMIZED_STRING_HASHING && !FEATURE_NETCORE
                collisionCount++;
#endif
            }

            int index;
            if (m_freeList >= 0)
            {
                index = m_freeList;
                m_freeList = m_slots[index].next;
            }
            else
            {
                if (m_lastIndex == m_slots.Length)
                {
                    IncreaseCapacity();
                    // this will change during resize
                    bucket = hashCode % m_buckets.Length;
                }
                index = m_lastIndex;
                m_lastIndex++;
            }
            m_slots[index].hashCode = hashCode;
            m_slots[index].value = value;
            m_slots[index].next = m_buckets[bucket] - 1;
            m_buckets[bucket] = index + 1;
            m_count++;

#if FEATURE_RANDOMIZED_STRING_HASHING && !FEATURE_NETCORE
            if(collisionCount > HashHelpers.HashCollisionThreshold && HashHelpers.IsWellKnownEqualityComparer(m_comparer)) {
                m_comparer = (IEqualityComparer<T>) HashHelpers.GetRandomizedEqualityComparer(m_comparer);
                SetCapacity(m_buckets.Length, true);
            }
#endif // FEATURE_RANDOMIZED_STRING_HASHING

            return true;
        }

        // Add value at known index with known hash code. Used only
        // when constructing from another HashSetEx.
        private void AddValue(int index, int hashCode, T value)
        {
            int bucket = hashCode % m_buckets.Length;

#if DEBUG
            Debug.Assert(InternalGetHashCode(value) == hashCode);
            for (int i = m_buckets[bucket] - 1; i >= 0; i = m_slots[i].next)
            {
                Debug.Assert(!m_comparer.Equals(m_slots[i].value, value));
            }
#endif

            Debug.Assert(m_freeList == -1);
            m_slots[index].hashCode = hashCode;
            m_slots[index].value = value;
            m_slots[index].next = m_buckets[bucket] - 1;
            m_buckets[bucket] = index + 1;
        }

        /// <summary>
        /// Checks if this contains of other's elements. Iterates over other's elements and 
        /// returns false as soon as it finds an element in other that's not in this.
        /// Used by SupersetOf, ProperSupersetOf, and SetEquals.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        private bool ContainsAllElements(IEnumerable<T> other)
        {
            foreach (T element in other)
            {
                if (!Contains(element))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Implementation Notes:
        /// If other is a hashset and is using same equality comparer, then checking subset is 
        /// faster. Simply check that each element in this is in other.
        /// 
        /// Note: if other doesn't use same equality comparer, then Contains check is invalid,
        /// which is why callers must take are of this.
        /// 
        /// If callers are concerned about whether this is a proper subset, they take care of that.
        ///
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        private bool IsSubsetOfHashSetExWithSameEC(HashSetEx<T> other)
        {

            foreach (T item in this)
            {
                if (!other.Contains(item))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// If other is a hashset that uses same equality comparer, intersect is much faster 
        /// because we can use other's Contains
        /// </summary>
        /// <param name="other"></param>
        private void IntersectWithHashSetExWithSameEC(HashSetEx<T> other)
        {
            for (int i = 0; i < m_lastIndex; i++)
            {
                if (m_slots[i].hashCode >= 0)
                {
                    T item = m_slots[i].value;
                    if (!other.Contains(item))
                    {
                        Remove(item);
                    }
                }
            }
        }

        /// <summary>
        /// Iterate over other. If contained in this, mark an element in bit array corresponding to
        /// its position in m_slots. If anything is unmarked (in bit array), remove it.
        /// 
        /// This attempts to allocate on the stack, if below StackAllocThreshold.
        /// </summary>
        /// <param name="other"></param>
        [SecuritySafeCritical]
        private unsafe void IntersectWithEnumerable(IEnumerable<T> other)
        {
            Debug.Assert(m_buckets != null, "m_buckets shouldn't be null; callers should check first");

            // keep track of current last index; don't want to move past the end of our bit array
            // (could happen if another thread is modifying the collection)
            int originalLastIndex = m_lastIndex;
            int intArrayLength = BitHelper.ToIntArrayLength(originalLastIndex);

            BitHelper bitHelper;
            if (intArrayLength <= StackAllocThreshold)
            {
                int* bitArrayPtr = stackalloc int[intArrayLength];
                bitHelper = new BitHelper(bitArrayPtr, intArrayLength);
            }
            else
            {
                int[] bitArray = new int[intArrayLength];
                bitHelper = new BitHelper(bitArray, intArrayLength);
            }

            // mark if contains: find index of in slots array and mark corresponding element in bit array
            foreach (T item in other)
            {
                int index = InternalIndexOf(item);
                if (index >= 0)
                {
                    bitHelper.MarkBit(index);
                }
            }

            // if anything unmarked, remove it. Perf can be optimized here if BitHelper had a 
            // FindFirstUnmarked method.
            for (int i = 0; i < originalLastIndex; i++)
            {
                if (m_slots[i].hashCode >= 0 && !bitHelper.IsMarked(i))
                {
                    Remove(m_slots[i].value);
                }
            }
        }

        /// <summary>
        /// Used internally by set operations which have to rely on bit array marking. This is like
        /// Contains but returns index in slots array. 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private int InternalIndexOf(T item)
        {
            Debug.Assert(m_buckets != null, "m_buckets was null; callers should check first");

            int hashCode = InternalGetHashCode(item);
            for (int i = m_buckets[hashCode % m_buckets.Length] - 1; i >= 0; i = m_slots[i].next)
            {
                if ((m_slots[i].hashCode) == hashCode && m_comparer.Equals(m_slots[i].value, item))
                {
                    return i;
                }
            }
            // wasn't found
            return -1;
        }

        /// <summary>
        /// if other is a set, we can assume it doesn't have duplicate elements, so use this
        /// technique: if can't remove, then it wasn't present in this set, so add.
        /// 
        /// As with other methods, callers take care of ensuring that other is a hashset using the
        /// same equality comparer.
        /// </summary>
        /// <param name="other"></param>
        private void SymmetricExceptWithUniqueHashSetEx(HashSetEx<T> other)
        {
            foreach (T item in other)
            {
                if (!Remove(item))
                {
                    AddIfNotPresent(item);
                }
            }
        }

        /// <summary>
        /// Implementation notes:
        /// 
        /// Used for symmetric except when other isn't a HashSetEx. This is more tedious because 
        /// other may contain duplicates. HashSetEx technique could fail in these situations:
        /// 1. Other has a duplicate that's not in this: HashSetEx technique would add then 
        /// remove it.
        /// 2. Other has a duplicate that's in this: HashSetEx technique would remove then add it
        /// back.
        /// In general, its presence would be toggled each time it appears in other. 
        /// 
        /// This technique uses bit marking to indicate whether to add/remove the item. If already
        /// present in collection, it will get marked for deletion. If added from other, it will
        /// get marked as something not to remove.
        ///
        /// </summary>
        /// <param name="other"></param>
        [SecuritySafeCritical]
        private unsafe void SymmetricExceptWithEnumerable(IEnumerable<T> other)
        {
            int originalLastIndex = m_lastIndex;
            int intArrayLength = BitHelper.ToIntArrayLength(originalLastIndex);

            BitHelper itemsToRemove;
            BitHelper itemsAddedFromOther;
            if (intArrayLength <= StackAllocThreshold / 2)
            {
                int* itemsToRemovePtr = stackalloc int[intArrayLength];
                itemsToRemove = new BitHelper(itemsToRemovePtr, intArrayLength);

                int* itemsAddedFromOtherPtr = stackalloc int[intArrayLength];
                itemsAddedFromOther = new BitHelper(itemsAddedFromOtherPtr, intArrayLength);
            }
            else
            {
                int[] itemsToRemoveArray = new int[intArrayLength];
                itemsToRemove = new BitHelper(itemsToRemoveArray, intArrayLength);

                int[] itemsAddedFromOtherArray = new int[intArrayLength];
                itemsAddedFromOther = new BitHelper(itemsAddedFromOtherArray, intArrayLength);
            }

            foreach (T item in other)
            {
                int location = 0;
                bool added = AddOrGetLocation(item, out location);
                if (added)
                {
                    // wasn't already present in collection; flag it as something not to remove
                    // *NOTE* if location is out of range, we should ignore. BitHelper will
                    // detect that it's out of bounds and not try to mark it. But it's 
                    // expected that location could be out of bounds because adding the item
                    // will increase m_lastIndex as soon as all the free spots are filled.
                    itemsAddedFromOther.MarkBit(location);
                }
                else
                {
                    // already there...if not added from other, mark for remove. 
                    // *NOTE* Even though BitHelper will check that location is in range, we want 
                    // to check here. There's no point in checking items beyond originalLastIndex
                    // because they could not have been in the original collection
                    if (location < originalLastIndex && !itemsAddedFromOther.IsMarked(location))
                    {
                        itemsToRemove.MarkBit(location);
                    }
                }
            }

            // if anything marked, remove it
            for (int i = 0; i < originalLastIndex; i++)
            {
                if (itemsToRemove.IsMarked(i))
                {
                    Remove(m_slots[i].value);
                }
            }
        }

        /// <summary>
        /// Add if not already in hashset. Returns an out param indicating index where added. This 
        /// is used by SymmetricExcept because it needs to know the following things:
        /// - whether the item was already present in the collection or added from other
        /// - where it's located (if already present, it will get marked for removal, otherwise
        /// marked for keeping)
        /// </summary>
        /// <param name="value"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        private bool AddOrGetLocation(T value, out int location)
        {
            Debug.Assert(m_buckets != null, "m_buckets is null, callers should have checked");

            int hashCode = InternalGetHashCode(value);
            int bucket = hashCode % m_buckets.Length;
            for (int i = m_buckets[hashCode % m_buckets.Length] - 1; i >= 0; i = m_slots[i].next)
            {
                if (m_slots[i].hashCode == hashCode && m_comparer.Equals(m_slots[i].value, value))
                {
                    location = i;
                    return false; //already present
                }
            }
            int index;
            if (m_freeList >= 0)
            {
                index = m_freeList;
                m_freeList = m_slots[index].next;
            }
            else
            {
                if (m_lastIndex == m_slots.Length)
                {
                    IncreaseCapacity();
                    // this will change during resize
                    bucket = hashCode % m_buckets.Length;
                }
                index = m_lastIndex;
                m_lastIndex++;
            }
            m_slots[index].hashCode = hashCode;
            m_slots[index].value = value;
            m_slots[index].next = m_buckets[bucket] - 1;
            m_buckets[bucket] = index + 1;
            m_count++;
            location = index;
            return true;
        }

        /// <summary>
        /// Determines counts that can be used to determine equality, subset, and superset. This
        /// is only used when other is an IEnumerable and not a HashSetEx. If other is a HashSetEx
        /// these properties can be checked faster without use of marking because we can assume 
        /// other has no duplicates.
        /// 
        /// The following count checks are performed by callers:
        /// 1. Equals: checks if unfoundCount = 0 and uniqueFoundCount = m_count; i.e. everything 
        /// in other is in this and everything in this is in other
        /// 2. Subset: checks if unfoundCount >= 0 and uniqueFoundCount = m_count; i.e. other may
        /// have elements not in this and everything in this is in other
        /// 3. Proper subset: checks if unfoundCount > 0 and uniqueFoundCount = m_count; i.e
        /// other must have at least one element not in this and everything in this is in other
        /// 4. Proper superset: checks if unfound count = 0 and uniqueFoundCount strictly less
        /// than m_count; i.e. everything in other was in this and this had at least one element
        /// not contained in other.
        /// 
        /// An earlier implementation used delegates to perform these checks rather than returning
        /// an ElementCount struct; however this was changed due to the perf overhead of delegates.
        /// </summary>
        /// <param name="other"></param>
        /// <param name="returnIfUnfound">Allows us to finish faster for equals and proper superset
        /// because unfoundCount must be 0.</param>
        /// <returns></returns>
        [SecuritySafeCritical]
        private unsafe ElementCount CheckUniqueAndUnfoundElements(IEnumerable<T> other, bool returnIfUnfound)
        {
            ElementCount result;

            // need special case in case this has no elements. 
            if (m_count == 0)
            {
                int numElementsInOther = 0;
                foreach (T item in other)
                {
                    numElementsInOther++;
                    // break right away, all we want to know is whether other has 0 or 1 elements
                    break;
                }
                result.uniqueCount = 0;
                result.unfoundCount = numElementsInOther;
                return result;
            }


            Debug.Assert((m_buckets != null) && (m_count > 0), "m_buckets was null but count greater than 0");

            int originalLastIndex = m_lastIndex;
            int intArrayLength = BitHelper.ToIntArrayLength(originalLastIndex);

            BitHelper bitHelper;
            if (intArrayLength <= StackAllocThreshold)
            {
                int* bitArrayPtr = stackalloc int[intArrayLength];
                bitHelper = new BitHelper(bitArrayPtr, intArrayLength);
            }
            else
            {
                int[] bitArray = new int[intArrayLength];
                bitHelper = new BitHelper(bitArray, intArrayLength);
            }

            // count of items in other not found in this
            int unfoundCount = 0;
            // count of unique items in other found in this
            int uniqueFoundCount = 0;

            foreach (T item in other)
            {
                int index = InternalIndexOf(item);
                if (index >= 0)
                {
                    if (!bitHelper.IsMarked(index))
                    {
                        // item hasn't been seen yet
                        bitHelper.MarkBit(index);
                        uniqueFoundCount++;
                    }
                }
                else
                {
                    unfoundCount++;
                    if (returnIfUnfound)
                    {
                        break;
                    }
                }
            }

            result.uniqueCount = uniqueFoundCount;
            result.unfoundCount = unfoundCount;
            return result;
        }

        /// <summary>
        /// Copies this to an array. Used for DebugView
        /// </summary>
        /// <returns></returns>
        internal T[] ToArray()
        {
            T[] newArray = new T[Count];
            CopyTo(newArray);
            return newArray;
        }

        /// <summary>
        /// Internal method used for HashSetExEqualityComparer. Compares set1 and set2 according 
        /// to specified comparer.
        /// 
        /// Because items are hashed according to a specific equality comparer, we have to resort
        /// to n^2 search if they're using different equality comparers.
        /// </summary>
        /// <param name="set1"></param>
        /// <param name="set2"></param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        internal static bool HashSetExEquals(HashSetEx<T> set1, HashSetEx<T> set2, IEqualityComparer<T> comparer)
        {
            // handle null cases first
            if (set1 == null)
            {
                return (set2 == null);
            }
            else if (set2 == null)
            {
                // set1 != null
                return false;
            }

            // all comparers are the same; this is faster
            if (AreEqualityComparersEqual(set1, set2))
            {
                if (set1.Count != set2.Count)
                {
                    return false;
                }
                // suffices to check subset
                foreach (T item in set2)
                {
                    if (!set1.Contains(item))
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {  // n^2 search because items are hashed according to their respective ECs
                foreach (T set2Item in set2)
                {
                    bool found = false;
                    foreach (T set1Item in set1)
                    {
                        if (comparer.Equals(set2Item, set1Item))
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// Checks if equality comparers are equal. This is used for algorithms that can
        /// speed up if it knows the other item has unique elements. I.e. if they're using 
        /// different equality comparers, then uniqueness assumption between sets break.
        /// </summary>
        /// <param name="set1"></param>
        /// <param name="set2"></param>
        /// <returns></returns>
        private static bool AreEqualityComparersEqual(HashSetEx<T> set1, HashSetEx<T> set2)
        {
            return set1.Comparer.Equals(set2.Comparer);
        }

        /// <summary>
        /// Workaround Comparers that throw ArgumentNullException for GetHashCode(null).
        /// </summary>
        /// <param name="item"></param>
        /// <returns>hash code</returns>
        private int InternalGetHashCode(T item)
        {
            if (item == null)
            {
                return 0;
            }
            return m_comparer.GetHashCode(item) & Lower31BitMask;
        }

        #endregion

        // used for set checking operations (using enumerables) that rely on counting
        internal struct ElementCount
        {
            internal int uniqueCount;
            internal int unfoundCount;
        }

        internal struct Slot
        {
            internal int hashCode;      // Lower 31 bits of hash code, -1 if unused
            internal int next;          // Index of next entry, -1 if last
            internal T value;
        }

#if !SILVERLIGHT
        [Serializable()]
#endif
        public struct Enumerator : IEnumerator<T>, IEnumerator
        {
            private HashSetEx<T> set;
            private int index;
            private T current;

            internal Enumerator(HashSetEx<T> set)
            {
                this.set = set;
                index = 0;
                current = default(T);
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                while (index < set.m_lastIndex)
                {
                    if (set.m_slots[index].hashCode >= 0)
                    {
                        current = set.m_slots[index].value;
                        index++;
                        return true;
                    }
                    index++;
                }
                index = set.m_lastIndex + 1;
                current = default(T);
                return false;
            }

            public T Current
            {
                get
                {
                    return current;
                }
            }

            Object IEnumerator.Current
            {
                get
                {
                    if (index == 0 || index == set.m_lastIndex + 1)
                    {
                        throw new InvalidOperationException();
                    }
                    return Current;
                }
            }

            void IEnumerator.Reset()
            {
                index = 0;
                current = default(T);
            }
        }
    }

}