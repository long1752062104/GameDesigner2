using System;
using System.Collections.Generic;

namespace Net.System
{
    public class ByteEqualityComparer : IEqualityComparer<byte>
    {
        public bool Equals(byte x, byte y)
        {
            return x == y;
        }

        public int GetHashCode(byte b)
        {
            return b.GetHashCode();
        }
    }

    public class SByteEqualityComparer : IEqualityComparer<sbyte>
    {
        public bool Equals(sbyte x, sbyte y)
        {
            return x == y;
        }

        public int GetHashCode(sbyte b)
        {
            return b.GetHashCode();
        }
    }

    public class BoolEqualityComparer : IEqualityComparer<bool>
    {
        public bool Equals(bool x, bool y)
        {
            return x == y;
        }

        public int GetHashCode(bool b)
        {
            return b.GetHashCode();
        }
    }

    public class ShortEqualityComparer : IEqualityComparer<short>
    {
        public bool Equals(short x, short y)
        {
            return x == y;
        }

        public int GetHashCode(short b)
        {
            return b.GetHashCode();
        }
    }
    public class UShortEqualityComparer : IEqualityComparer<ushort>
    {
        public bool Equals(ushort x, ushort y)
        {
            return x == y;
        }

        public int GetHashCode(ushort b)
        {
            return b.GetHashCode();
        }
    }
    public class IntEqualityComparer : IEqualityComparer<int>
    {
        public bool Equals(int x, int y)
        {
            return x == y;
        }

        public int GetHashCode(int b)
        {
            return b.GetHashCode();
        }
    }
    public class UIntEqualityComparer : IEqualityComparer<uint>
    {
        public bool Equals(uint x, uint y)
        {
            return x == y;
        }

        public int GetHashCode(uint b)
        {
            return b.GetHashCode();
        }
    }
    public class FloatEqualityComparer : IEqualityComparer<float>
    {
        public bool Equals(float x, float y)
        {
            return x == y;
        }

        public int GetHashCode(float b)
        {
            return b.GetHashCode();
        }
    }
    public class LongEqualityComparer : IEqualityComparer<long>
    {
        public bool Equals(long x, long y)
        {
            return x == y;
        }

        public int GetHashCode(long b)
        {
            return b.GetHashCode();
        }
    }
    public class ULongEqualityComparer : IEqualityComparer<ulong>
    {
        public bool Equals(ulong x, ulong y)
        {
            return x == y;
        }

        public int GetHashCode(ulong b)
        {
            return b.GetHashCode();
        }
    }
    public class DoubleEqualityComparer : IEqualityComparer<double>
    {
        public bool Equals(double x, double y)
        {
            return x == y;
        }

        public int GetHashCode(double b)
        {
            return b.GetHashCode();
        }
    }
    public class ChatEqualityComparer : IEqualityComparer<char>
    {
        public bool Equals(char x, char y)
        {
            return x == y;
        }

        public int GetHashCode(char b)
        {
            return b.GetHashCode();
        }
    }
    public class StringEqualityComparer : IEqualityComparer<string>
    {
        public bool Equals(string x, string y)
        {
            return x == y;
        }

        public int GetHashCode(string b)
        {
            return b.GetHashCode();
        }
    }
    public class DateTimeEqualityComparer : IEqualityComparer<DateTime>
    {
        public bool Equals(DateTime x, DateTime y)
        {
            return x == y;
        }

        public int GetHashCode(DateTime b)
        {
            return b.GetHashCode();
        }
    }
    public class DecimalEqualityComparer : IEqualityComparer<decimal>
    {
        public bool Equals(decimal x, decimal y)
        {
            return x == y;
        }

        public int GetHashCode(decimal b)
        {
            return b.GetHashCode();
        }
    }
}