using System.Text;
#if !CORE
using System.Linq;
using System.Collections.Generic;
#endif
using System.Runtime.CompilerServices;
#if SERIALIZE_SIZE32
using SIZE = System.Int32;
#else
using SIZE = System.UInt16;
#endif

namespace Net.Serialize
{
    public class NetConvertHelper
    {
#if SERIALIZE_SIZE32
        private const ushort LENGTH = 4;
#else
        private const ushort LENGTH = 2;
#endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void WriteArray<T>(byte* ptr, ref int offset, T[] value) where T : struct
        {
            if (value != null)
            {
                void* ptr1 = Unsafe.AsPointer(ref value[0]);
                int len = value.Length;
                Unsafe.WriteUnaligned(ptr + offset, (SIZE)len);
                offset += LENGTH;
                int count = len * Unsafe.SizeOf<T>();
                Unsafe.CopyBlock(ptr + offset, ptr1, (uint)count);
                offset += count;
            }
            else
            {
                Unsafe.WriteUnaligned<SIZE>(ptr + offset, 0);
                offset += LENGTH;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void WriteArray(byte* ptr, ref int offset, string[] value)
        {
            if (value != null)
            {
                int len = value.Length;
                Unsafe.WriteUnaligned(ptr + offset, (SIZE)len);
                offset += LENGTH;
                for (int i = 0; i < len; i++)
                    Write(ptr, ref offset, value[i]);
            }
            else
            {
                Unsafe.WriteUnaligned<SIZE>(ptr + offset, 0);
                offset += LENGTH;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void WriteCollection<T>(byte* ptr, ref int offset, ICollection<T> value) where T : struct
        {
            if (value != null)
            {
                int len = value.Count;
                Unsafe.WriteUnaligned(ptr + offset, (SIZE)len);
                offset += LENGTH;
                var enumerator = value.GetEnumerator();
                while (enumerator.MoveNext())
                    Write(ptr, ref offset, enumerator.Current);
            }
            else
            {
                Unsafe.WriteUnaligned<SIZE>(ptr + offset, 0);
                offset += LENGTH;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void WriteCollection(byte* ptr, ref int offset, ICollection<string> value)
        {
            if (value != null)
            {
                int len = value.Count;
                Unsafe.WriteUnaligned(ptr + offset, (SIZE)len);
                offset += LENGTH;
                var enumerator = value.GetEnumerator();
                while (enumerator.MoveNext())
                    Write(ptr, ref offset, enumerator.Current);
            }
            else
            {
                Unsafe.WriteUnaligned<SIZE>(ptr + offset, 0);
                offset += LENGTH;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe T[] ReadArray<T>(byte* ptr, ref int offset) where T : struct
        {
            var arrayLen = Unsafe.ReadUnaligned<SIZE>(ptr + offset);
            offset += LENGTH;
            if (arrayLen > 0)
            {
                var value = new T[arrayLen];
                void* ptr1 = Unsafe.AsPointer(ref value[0]);
                int count = arrayLen * Unsafe.SizeOf<T>();
                Unsafe.CopyBlock(ptr1, ptr + offset, (uint)count);
                offset += count;
                return value;
            }
            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe T ReadCollection<T, T1>(byte* ptr, ref int offset) where T : ICollection<T1>, new() where T1 : struct
        {
            var arrayLen = Unsafe.ReadUnaligned<SIZE>(ptr + offset);
            offset += LENGTH;
            if (arrayLen > 0)
            {
                var value = new T();
                var newValue = new T1[arrayLen];
                void* ptr1 = Unsafe.AsPointer(ref newValue[0]);
                int count = arrayLen * Unsafe.SizeOf<T1>();
                Unsafe.CopyBlock(ptr1, ptr + offset, (uint)count);
                offset += count;
                for (int i = 0; i < arrayLen; i++)
                    value.Add(newValue[i]);
                return value;
            }
            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe T ReadCollection<T>(byte* ptr, ref int offset) where T : ICollection<string>, new()
        {
            var newValue = ReadArray(ptr, ref offset);
            if (newValue == null)
                return default;
            var value = new T();
            for (int i = 0; i < newValue.Length; i++)
                value.Add(newValue[i]);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe string[] ReadArray(byte* ptr, ref int offset)
        {
            var arrayLen = Unsafe.ReadUnaligned<SIZE>(ptr + offset);
            offset += LENGTH;
            if (arrayLen > 0)
            {
                var value = new string[arrayLen];
                for (int i = 0; i < arrayLen; i++)
                    value[i] = Read(ptr, ref offset);
                return value;
            }
            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Write<T>(byte* ptr, ref int offset, T value) where T : struct
        {
            Unsafe.WriteUnaligned(ptr + offset, value);
            offset += Unsafe.SizeOf<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Write(byte* ptr, ref int offset, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
#if CORE
                var charSpan = value.AsSpan();
                var byteSpan = new Span<byte>(ptr + offset + LENGTH, value.Length * 3);
                var count = Encoding.UTF8.GetBytes(charSpan, byteSpan);
                Unsafe.WriteUnaligned(ptr + offset, (SIZE)count);
                offset += LENGTH + count;
#else
                int count = value.Length;
                fixed (char* ptr1 = value)
                {
                    int byteCount = Encoding.UTF8.GetByteCount(ptr1, count);
                    Unsafe.WriteUnaligned(ptr + offset, (SIZE)byteCount);
                    offset += LENGTH;
                    Encoding.UTF8.GetBytes(ptr1, count, ptr + offset, byteCount);
                    offset += byteCount;
                }
#endif
            }
            else
            {
                Unsafe.WriteUnaligned<SIZE>(ptr + offset, 0);
                offset += LENGTH;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe T Read<T>(byte* ptr, ref int offset) where T : struct
        {
            var value = Unsafe.ReadUnaligned<T>(ptr + offset);
            offset += Unsafe.SizeOf<T>();
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe string Read(byte* ptr, ref int offset)
        {
            var count = Unsafe.ReadUnaligned<SIZE>(ptr + offset);
            offset += LENGTH;
            if (count > 0)
            {
#if CORE
                var value = Encoding.UTF8.GetString(new ReadOnlySpan<byte>(ptr + offset, count));
                offset += count;
                return value;
#else
                var value = Encoding.UTF8.GetString(ptr + offset, count);
                offset += count;
                return value;
#endif
            }
            return string.Empty;
        }
    }
}