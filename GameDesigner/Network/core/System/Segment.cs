using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Net.Event;

namespace Net.System
{
    /// <summary>
    /// 内存数据片段
    /// </summary>
    public class Segment : IDisposable, ISegment
    {
        /// <summary>
        /// 总内存
        /// </summary>
        protected byte[] buffer;
        public byte[] Buffer { get => buffer; set => buffer = value; }
        /// <summary>
        /// 片的开始位置
        /// </summary>
        public int Offset { get; set; }
        /// <summary>
        /// 片的长度
        /// </summary>
        public int Count { get; set; }
        /// <summary>
        /// 读写位置
        /// </summary>
        protected int position;
        public int Position { get => position; set => position = value; }
        /// <summary>
        /// 获取总长度
        /// </summary>
        public int Length { get; private set; }
        /// <summary>
        /// 是否已经释放
        /// </summary>
        public bool IsDespose { get; set; }
        /// <summary>
        /// 是否可回收
        /// </summary>
        public bool IsRecovery { get; set; }
        /// <summary>
        /// 引用次数
        /// </summary>
        public int ReferenceCount { get; set; }

        /// <summary>
        /// 获取或设置总内存位置索引
        /// </summary>
        /// <param name="index">内存位置索引</param>
        /// <returns></returns>
        public byte this[int index] { get { return buffer[index]; } set { buffer[index] = value; } }

        /// <summary>
        /// 构造内存分片
        /// </summary>
        /// <param name="buffer"></param>
        public Segment(byte[] buffer) : this(buffer, 0, buffer.Length)
        {
        }

        /// <summary>
        /// 构造内存分片
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="isRecovery"></param>
        public Segment(byte[] buffer, bool isRecovery) : this(buffer, 0, buffer.Length, isRecovery)
        {
        }

        /// <summary>
        /// 构造内存分片
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <param name="isRecovery"></param>
        public Segment(byte[] buffer, int index, int count, bool isRecovery = true)
        {
            this.buffer = buffer;
            Offset = index;
            Count = count;
            Length = buffer.Length;
            position = index;
            IsDespose = !isRecovery;//如果不回收，则已经释放状态，不允许压入数组池
            IsRecovery = isRecovery;
            ReferenceCount = 0;
        }

        public static implicit operator Segment(byte[] buffer)
        {
            return new Segment(buffer);
        }

        public static implicit operator byte[](Segment segment)
        {
            return segment.buffer;
        }

        ~Segment()
        {
            if (IsRecovery && BufferPool.Log)
                NDebug.LogError("片段内存泄漏!请检查代码正确Push内存池!");
            //Dispose(); //已经析构了就不需要放入池中了
        }

        public virtual void Init()
        {
            Offset = 0;
            Count = 0;
            position = 0;
        }

        public virtual void SetPosition(int position)
        {
            this.position = position;
        }

        public void Dispose()
        {
            if (!IsRecovery)
                return;
            BufferPool.Push(this);
        }

        public void Close()
        {
            IsRecovery = false;
            IsDespose = true;
            buffer = null;
        }

        public override string ToString()
        {
            return $"byte[{(buffer != null ? buffer.Length : 0)}] index:{Offset} count:{Count} size:{Count - Offset}";
        }

        /// <summary>
        /// 复制分片数据
        /// </summary>
        /// <param name="recovery">复制数据后立即回收此分片?</param>
        /// <param name="resetPos"></param>
        /// <returns></returns>
        public byte[] ToArray(bool recovery = false, bool resetPos = false)
        {
            Flush(resetPos);
            var array = new byte[Count];
            Unsafe.CopyBlock(ref array[0], ref buffer[Offset], (uint)Count);
            if (recovery) BufferPool.Push(this);
            return array;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(void* ptr, int count)
        {
            fixed (void* ptr1 = &buffer[position])
            {
                Unsafe.CopyBlock(ptr1, ptr, (uint)count);
                position += count;
            }
        }

        public void WriteValue<T>(T value)
        {
            switch (value)
            {
                case byte value1:
                    Write(value1);
                    break;
                case sbyte value1:
                    Write(value1);
                    break;
                case bool value1:
                    Write(value1);
                    break;
                case short value1:
                    Write(value1);
                    break;
                case ushort value1:
                    Write(value1);
                    break;
                case char value1:
                    Write(value1);
                    break;
                case int value1:
                    Write(value1);
                    break;
                case uint value1:
                    Write(value1);
                    break;
                case float value1:
                    Write(value1);
                    break;
                case long value1:
                    Write(value1);
                    break;
                case ulong value1:
                    Write(value1);
                    break;
                case double value1:
                    Write(value1);
                    break;
                case DateTime value1:
                    Write(value1);
                    break;
                case TimeSpan value1:
                    Write(value1);
                    break;
#if CORE
                case TimeOnly value1:
                    Write(value1);
                    break;
#endif
                case DateTimeOffset value1:
                    Write(value1);
                    break;
                case decimal value1:
                    Write(value1);
                    break;
                case string value1:
                    Write(value1);
                    break;
                case Enum value1:
                    Write(value1.GetHashCode());
                    break;
                default:
                    throw new Exception($"错误!基类不能序列化这个类:{value}");
            }
        }

        public T ReadValue<T>()
        {
            return (T)ReadValue(typeof(T));
        }

        public object ReadValue(Type type)
        {
            return ReadValue(Type.GetTypeCode(type));
        }

        public object ReadValue(TypeCode type)
        {
            object value = null;
            switch (type)
            {
                case TypeCode.Byte:
                    value = ReadByte();
                    break;
                case TypeCode.SByte:
                    value = ReadSByte();
                    break;
                case TypeCode.Boolean:
                    value = ReadBoolean();
                    break;
                case TypeCode.Int16:
                    value = ReadInt16();
                    break;
                case TypeCode.UInt16:
                    value = ReadUInt16();
                    break;
                case TypeCode.Char:
                    value = ReadChar();
                    break;
                case TypeCode.Int32:
                    value = ReadInt32();
                    break;
                case TypeCode.UInt32:
                    value = ReadUInt32();
                    break;
                case TypeCode.Single:
                    value = ReadFloat();
                    break;
                case TypeCode.Int64:
                    value = ReadInt64();
                    break;
                case TypeCode.UInt64:
                    value = ReadUInt64();
                    break;
                case TypeCode.Double:
                    value = ReadDouble();
                    break;
                case TypeCode.DateTime:
                    value = ReadDateTime();
                    break;
                case TypeCode.Decimal:
                    value = ReadDecimal();
                    break;
                case TypeCode.String:
                    value = ReadString();
                    break;
            }
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe byte[] Read(int count)
        {
            var array = new byte[count];
            Unsafe.CopyBlock(ref array[0], ref buffer[position], (uint)count);
            position += count;
            return array;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Read(byte[] destination, int count)
        {
            Unsafe.CopyBlock(ref destination[0], ref buffer[position], (uint)count);
            position += count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe byte* ReadPtr(int count)
        {
            var ptr = (byte*)Unsafe.AsPointer(ref buffer[position]);
            position += count;
            return ptr;
        }

        public void WriteList(object value)
        {
            switch (value)
            {
                case List<byte> array1:
                    Write(array1);
                    break;
                case List<sbyte> array1:
                    Write(array1);
                    break;
                case List<bool> array1:
                    Write(array1);
                    break;
                case List<short> array1:
                    Write(array1);
                    break;
                case List<ushort> array1:
                    Write(array1);
                    break;
                case List<char> array1:
                    Write(array1);
                    break;
                case List<int> array1:
                    Write(array1);
                    break;
                case List<uint> array1:
                    Write(array1);
                    break;
                case List<float> array1:
                    Write(array1);
                    break;
                case List<long> array1:
                    Write(array1);
                    break;
                case List<ulong> array1:
                    Write(array1);
                    break;
                case List<double> array1:
                    Write(array1);
                    break;
                case List<DateTime> array1:
                    Write(array1);
                    break;
                case List<decimal> array1:
                    Write(array1);
                    break;
                case List<string> array1:
                    Write(array1);
                    break;
                case List<TimeSpan> array1:
                    Write(array1);
                    break;
#if CORE
                case List<TimeOnly> array1:
                    Write(array1);
                    break;
#endif
                case List<DateTimeOffset> array1:
                    Write(array1);
                    break;
                default:
                    {
                        var isEnum = value.GetType().GenericTypeArguments[0].IsEnum;
                        if (!isEnum)
                            throw new Exception($"错误!基类不能序列化这个类:{value}");
                        var array1 = value as IList;
                        Write(array1.Count);
                        for (int i = 0; i < array1.Count; i++)
                            Write((Enum)array1[i]);
                    }
                    break;
            }
        }

        public void WriteArray<T>(T[] array)
        {
            WriteArray(value: array);
        }
        public unsafe void WriteArray(object value)
        {
            switch (value)
            {
                case byte[] array1:
                    Write(array1);
                    break;
                case sbyte[] array1:
                    Write(array1);
                    break;
                case bool[] array1:
                    Write(array1);
                    break;
                case short[] array1:
                    Write(array1);
                    break;
                case ushort[] array1:
                    Write(array1);
                    break;
                case char[] array1:
                    Write(array1);
                    break;
                case int[] array1:
                    Write(array1);
                    break;
                case uint[] array1:
                    Write(array1);
                    break;
                case float[] array1:
                    Write(array1);
                    break;
                case long[] array1:
                    Write(array1);
                    break;
                case ulong[] array1:
                    Write(array1);
                    break;
                case double[] array1:
                    Write(array1);
                    break;
                case DateTime[] array1:
                    Write(array1);
                    break;
                case decimal[] array1:
                    Write(array1);
                    break;
                case string[] array1:
                    Write(array1);
                    break;
                case TimeSpan[] array1:
                    Write(array1);
                    break;
#if CORE
                case TimeOnly[] array1:
                    Write(array1);
                    break;
#endif
                case DateTimeOffset[] array1:
                    Write(array1);
                    break;
                default:
                    throw new Exception($"错误!基类不能序列化这个类:{value}");
            }
        }
        public List<T> ReadList<T>()
        {
            var array = ReadArray<T>();
            if (array == null)
                return new List<T>();
            return new List<T>(array);
        }
        public object ReadList(Type type)
        {
            var array = ReadArray(type);
            var listType = typeof(List<>);
            if (array == null)
                return Activator.CreateInstance(listType.MakeGenericType(type));
            if (type.IsEnum)
            {
                var array1 = array as int[];
                var list = (IList)Activator.CreateInstance(listType.MakeGenericType(type), array1.Length); //设置总量
                for (int i = 0; i < array1.Length; i++)
                    list.Add(array1[i]); //一个一个添加 不能 list[i] = array1[i]; 这个无效
                return list;
            }
            return Activator.CreateInstance(listType.MakeGenericType(type), array);
        }
        public T[] ReadArray<T>()
        {
            return ReadArray(typeof(T)) as T[];
        }
        public object ReadArray(Type type)
        {
            return ReadArray(Type.GetTypeCode(type));
        }
        public object ReadArray(TypeCode type)
        {
            object array;
            switch (type)
            {
                case TypeCode.Byte:
                    array = ReadByteArray();
                    break;
                case TypeCode.SByte:
                    array = ReadSByteArray();
                    break;
                case TypeCode.Boolean:
                    array = ReadBooleanArray();
                    break;
                case TypeCode.Int16:
                    array = ReadInt16Array();
                    break;
                case TypeCode.UInt16:
                    array = ReadUInt16Array();
                    break;
                case TypeCode.Char:
                    array = ReadCharArray();
                    break;
                case TypeCode.Int32:
                    array = ReadInt32Array();
                    break;
                case TypeCode.UInt32:
                    array = ReadUInt32Array();
                    break;
                case TypeCode.Single:
                    array = ReadFloatArray();
                    break;
                case TypeCode.Int64:
                    array = ReadInt64Array();
                    break;
                case TypeCode.UInt64:
                    array = ReadUInt64Array();
                    break;
                case TypeCode.Double:
                    array = ReadDoubleArray();
                    break;
                case TypeCode.DateTime:
                    array = ReadDateTimeArray();
                    break;
                case TypeCode.Decimal:
                    array = ReadDecimalArray();
                    break;
                case TypeCode.String:
                    array = ReadStringArray();
                    break;
                default:
                    throw new Exception("错误!");
            }
            return array;
        }
        public void SetLength(int length)
        {
            if (position > length)
                position = length;
            Count = length;
        }

        public void SetPositionLength(int length)
        {
            position = Offset + length; //解决如果有偏移，ToArray后错乱问题
            //Position = length;
            Count = length;
        }

        #region Write
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteByte(byte value)
        {
            buffer[position++] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteSByte(sbyte value)
        {
            buffer[position++] = (byte)value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte value)
        {
            WriteByte(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(sbyte value)
        {
            WriteSByte(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(bool value)
        {
            WriteByte((byte)(value ? 1 : 0));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(short value)
        {
            Write((ushort)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual unsafe void Write(ushort value)
        {
            if (value < 128)
            {
                WriteByte((byte)value);
                return;
            }
            while (value > 0)
            {
                if (value <= 127)
                {
                    WriteByte((byte)value);
                    break;
                }
                WriteByte((byte)((value & 127) | 128));
                value >>= 7;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void WriteFixed(ushort value)
        {
            Write(&value, 2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(char value)
        {
            Write((ushort)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(int value)
        {
            Write((uint)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual unsafe void Write(uint value)
        {
            if (value < 128U)
            {
                WriteByte((byte)value);
                return;
            }
            while (value > 0)
            {
                if (value <= 127U)
                {
                    WriteByte((byte)value);
                    break;
                }
                WriteByte((byte)((value & 127U) | 128U));
                value >>= 7;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void WriteFixed(uint value)
        {
            Write(&value, 4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(float value)
        {
            var ptr = *(uint*)&value;
            Write(ptr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(long value)
        {
            Write((ulong)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual unsafe void Write(ulong value)
        {
            if (value < 128UL)
            {
                WriteByte((byte)value);
                return;
            }
            while (value > 0)
            {
                if (value <= 127UL)
                {
                    WriteByte((byte)value);
                    break;
                }
                WriteByte((byte)((value & 127UL) | 128UL));
                value >>= 7;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void WriteFixed(ulong value)
        {
            Write(&value, 8);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(double value)
        {
            var ptr = *(ulong*)&value;
            Write(ptr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(decimal value)
        {
            var ptr = &value;
            Write(ptr, 16);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(DateTime value)
        {
            Write((ulong)value.Ticks);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(TimeSpan value)
        {
            Write((ulong)value.Ticks);
        }

#if CORE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(TimeOnly value)
        {
            Write((ulong)value.Ticks);
        }
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(DateTimeOffset value)
        {
            Write((ulong)value.Ticks);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(Enum value)
        {
            Write(value.GetHashCode());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual unsafe void Write(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                WriteByte(0);
                return;
            }
            var count = value.Length;
            fixed (char* ptr = value)
            {
                int byteCount = Encoding.UTF8.GetByteCount(ptr, count);
                Write(byteCount);
                fixed (byte* ptr1 = &buffer[position])
                {
                    Encoding.UTF8.GetBytes(ptr, count, ptr1, byteCount);
                    position += byteCount;
                }
            }
        }

        /// <summary>
        /// 写入字节数组
        /// </summary>
        /// <param name="value"></param>
        /// <param name="recordLength">是否记录此次写入的字节长度?</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte[] value, bool recordLength = true)
        {
            var count = value.Length;
            if (recordLength)
                Write(count);
            Unsafe.CopyBlock(ref buffer[position], ref value[0], (uint)count);
            position += count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(byte[] value, int index, int count)
        {
            if (index >= value.Length)
                return;
            Unsafe.CopyBlock(ref buffer[position], ref value[index], (uint)count);
            position += count;
        }

        /// <summary>
        /// 写入字节数组
        /// </summary>
        /// <param name="value"></param>
        /// <param name="recordLength">是否记录此次写入的字节长度?</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(sbyte[] value, bool recordLength = true)
        {
            var count = value.Length;
            if (recordLength)
                Write(count);
            var dest_ptr = Unsafe.AsPointer(ref buffer[position]);
            var source_ptr = Unsafe.AsPointer(ref value[0]);
            Unsafe.CopyBlock(dest_ptr, source_ptr, (uint)count);
            position += value.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(bool[] value)
        {
            Write(value.Length);
            fixed (bool* ptr = &value[0])
            {
                Write(ptr, value.Length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(short[] value)
        {
            Write(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                Write(value[i]);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(ushort[] value)
        {
            Write(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                Write(value[i]);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(char[] value)
        {
            Write(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                Write(value[i]);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(int[] value)
        {
            Write(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                Write(value[i]);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(uint[] value)
        {
            Write(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                Write(value[i]);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(float[] value)
        {
            Write(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                Write(value[i]);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(long[] value)
        {
            Write(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                Write(value[i]);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(ulong[] value)
        {
            Write(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                Write(value[i]);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(DateTime[] value)
        {
            Write(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                Write(value[i]);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(TimeSpan[] value)
        {
            Write(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                Write(value[i]);
            }
        }
#if CORE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(TimeOnly[] value)
        {
            Write(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                Write(value[i]);
            }
        }
#endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(DateTimeOffset[] value)
        {
            Write(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                Write(value[i]);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(double[] value)
        {
            Write(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                Write(value[i]);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(decimal[] value)
        {
            Write(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                Write(value[i]);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(string[] value)
        {
            Write(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                Write(value[i]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void WriteGeneric(IEnumerable value)
        {
            switch (value)
            {
                case ICollection<byte> value1:
                    Write(value1);
                    break;
                case ICollection<sbyte> value1:
                    Write(value1);
                    break;
                case ICollection<bool> value1:
                    Write(value1);
                    break;
                case ICollection<short> value1:
                    Write(value1);
                    break;
                case ICollection<ushort> value1:
                    Write(value1);
                    break;
                case ICollection<char> value1:
                    Write(value1);
                    break;
                case ICollection<int> value1:
                    Write(value1);
                    break;
                case ICollection<uint> value1:
                    Write(value1);
                    break;
                case ICollection<float> value1:
                    Write(value1);
                    break;
                case ICollection<long> value1:
                    Write(value1);
                    break;
                case ICollection<ulong> value1:
                    Write(value1);
                    break;
                case ICollection<double> value1:
                    Write(value1);
                    break;
                case ICollection<decimal> value1:
                    Write(value1);
                    break;
                case ICollection<string> value1:
                    Write(value1);
                    break;
                case ICollection<TimeSpan> value1:
                    Write(value1);
                    break;
                case ICollection<DateTime> value1:
                    Write(value1);
                    break;
#if CORE
                case ICollection<TimeOnly> value1:
                    Write(value1);
                    break;
#endif
                case ICollection<DateTimeOffset> value1:
                    Write(value1);
                    break;
                default:
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(ICollection<byte> value)
        {
            Write(value.Count);
            foreach (var val in value)
            {
                Write(val);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(ICollection<sbyte> value)
        {
            Write(value.Count);
            foreach (var val in value)
            {
                Write(val);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(ICollection<bool> value)
        {
            Write(value.Count);
            foreach (var val in value)
            {
                Write(val);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(ICollection<short> value)
        {
            Write(value.Count);
            foreach (var val in value)
            {
                Write(val);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(ICollection<ushort> value)
        {
            Write(value.Count);
            foreach (var val in value)
            {
                Write(val);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(ICollection<char> value)
        {
            Write(value.Count);
            foreach (var val in value)
            {
                Write(val);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(ICollection<int> value)
        {
            Write(value.Count);
            foreach (var val in value)
            {
                Write(val);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(ICollection<uint> value)
        {
            Write(value.Count);
            foreach (var val in value)
            {
                Write(val);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(ICollection<float> value)
        {
            Write(value.Count);
            foreach (var val in value)
            {
                Write(val);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(ICollection<long> value)
        {
            Write(value.Count);
            foreach (var val in value)
            {
                Write(val);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(ICollection<ulong> value)
        {
            Write(value.Count);
            foreach (var val in value)
            {
                Write(val);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(ICollection<DateTime> value)
        {
            Write(value.Count);
            foreach (var val in value)
            {
                Write(val);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(ICollection<TimeSpan> value)
        {
            Write(value.Count);
            foreach (var val in value)
            {
                Write(val);
            }
        }
#if CORE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(ICollection<TimeOnly> value)
        {
            Write(value.Count);
            foreach (var val in value)
            {
                Write(val);
            }
        }
#endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(ICollection<DateTimeOffset> value)
        {
            Write(value.Count);
            foreach (var val in value)
            {
                Write(val);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(ICollection<double> value)
        {
            Write(value.Count);
            foreach (var val in value)
            {
                Write(val);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(ICollection<decimal> value)
        {
            Write(value.Count);
            foreach (var val in value)
            {
                Write(val);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(ICollection<string> value)
        {
            Write(value.Count);
            foreach (var val in value)
            {
                Write(val);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(ICollection<Enum> value)
        {
            Write(value.Count);
            foreach (var val in value)
            {
                Write(val);
            }
        }
        #endregion

        #region Read
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadByte()
        {
            return buffer[position++];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte ReadSByte()
        {
            return (sbyte)buffer[position++];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadBoolean()
        {
            return buffer[position++] == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe short ReadInt16()
        {
            return (short)ReadUInt16();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual unsafe ushort ReadUInt16()
        {
            return (ushort)ReadUInt32();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ushort ReadUInt16Fixed()
        {
            fixed (byte* ptr = &buffer[position])
            {
                position += 2;
                return *(ushort*)ptr; //不处理大小端
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public char ReadChar()
        {
            return (char)ReadUInt16();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe int ReadInt32()
        {
            return (int)ReadUInt32();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual unsafe uint ReadUInt32()
        {
            fixed (byte* ptr = &buffer[position])
            {
                uint result = 0u;
                int count = 0;
                for (int i = 0; i < 5; i++) //最高值可达到5字节
                {
                    var value = ptr[i];
                    result |= (value & 127u) << (i * 7);
                    if (value < 128)
                    {
                        count = i + 1;
                        break;
                    }
                }
                position += count;
                return result;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe uint ReadUInt32Fixed()
        {
            fixed (byte* ptr = &buffer[position])
            {
                position += 4;
                return *(uint*)ptr; //不处理大小端
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe float ReadFloat()
        {
            uint value = ReadUInt32();
            return *(float*)&value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe float ReadSingle()
        {
            return ReadFloat();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe long ReadInt64()
        {
            return (long)ReadUInt64();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual unsafe ulong ReadUInt64()
        {
            fixed (byte* ptr = &buffer[position])
            {
                ulong result = 0u;
                int count = 0;
                for (int i = 0; i < 10; i++) //最高值可达到10个字节， 负数时可测试
                {
                    var value = ptr[i];
                    result |= (value & 127ul) << (i * 7);
                    if (value < 128)
                    {
                        count = i + 1;
                        break;
                    }
                }
                position += count;
                return result;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ulong ReadUInt64Fixed()
        {
            fixed (byte* ptr = &buffer[position])
            {
                position += 8;
                return *(ulong*)ptr; //不处理大小端
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe double ReadDouble()
        {
            ulong value = ReadUInt64();
            return *(double*)&value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual unsafe string ReadString()
        {
            var count = ReadInt32();
            if (count == 0)
                return string.Empty;
            var value = Encoding.UTF8.GetString(buffer, position, count);
            position += count;
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe decimal ReadDecimal()
        {
            decimal value = default;
            void* ptr = &value;
            fixed (void* ptr1 = &buffer[position])
                Unsafe.CopyBlock(ptr, ptr1, 16U);
            position += 16;
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTime ReadDateTime()
        {
            return new DateTime((long)ReadUInt64());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TimeSpan ReadTimeSpan()
        {
            return new TimeSpan((long)ReadUInt64());
        }
#if CORE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TimeOnly ReadTimeOnly()
        {
            return new TimeOnly((long)ReadUInt64());
        }
#endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTimeOffset ReadDateTimeOffset()
        {
            return new DateTimeOffset((long)ReadUInt64(), TimeSpan.Zero);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe T ReadEnum<T>() where T : Enum
        {
            return (T)Enum.ToObject(typeof(T), ReadInt32());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe object ReadEnum(Type type)
        {
            return Enum.ToObject(type, ReadInt32());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe byte[] ReadByteArray()
        {
            var count = ReadInt32();
            var value = new byte[count];
            for (int i = 0; i < count; i++)
            {
                value[i] = ReadByte();
            }
            return value;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe sbyte[] ReadSByteArray()
        {
            var count = ReadInt32();
            var value = new sbyte[count];
            for (int i = 0; i < count; i++)
            {
                value[i] = ReadSByte();
            }
            return value;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe bool[] ReadBooleanArray()
        {
            var count = ReadInt32();
            var value = new bool[count];
            for (int i = 0; i < count; i++)
            {
                value[i] = ReadBoolean();
            }
            return value;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe short[] ReadInt16Array()
        {
            var count = ReadInt32();
            var value = new short[count];
            for (int i = 0; i < count; i++)
            {
                value[i] = ReadInt16();
            }
            return value;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ushort[] ReadUInt16Array()
        {
            var count = ReadInt32();
            var value = new ushort[count];
            for (int i = 0; i < count; i++)
            {
                value[i] = ReadUInt16();
            }
            return value;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe char[] ReadCharArray()
        {
            var count = ReadInt32();
            var value = new char[count];
            for (int i = 0; i < count; i++)
            {
                value[i] = ReadChar();
            }
            return value;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe int[] ReadInt32Array()
        {
            var count = ReadInt32();
            var value = new int[count];
            for (int i = 0; i < count; i++)
            {
                value[i] = ReadInt32();
            }
            return value;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe uint[] ReadUInt32Array()
        {
            var count = ReadInt32();
            var value = new uint[count];
            for (int i = 0; i < count; i++)
            {
                value[i] = ReadUInt32();
            }
            return value;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe float[] ReadFloatArray()
        {
            var count = ReadInt32();
            var value = new float[count];
            for (int i = 0; i < count; i++)
            {
                value[i] = ReadFloat();
            }
            return value;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe float[] ReadSingleArray()
        {
            return ReadFloatArray();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe long[] ReadInt64Array()
        {
            var count = ReadInt32();
            var value = new long[count];
            for (int i = 0; i < count; i++)
            {
                value[i] = ReadInt64();
            }
            return value;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ulong[] ReadUInt64Array()
        {
            var count = ReadInt32();
            var value = new ulong[count];
            for (int i = 0; i < count; i++)
            {
                value[i] = ReadUInt64();
            }
            return value;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe double[] ReadDoubleArray()
        {
            var count = ReadInt32();
            var value = new double[count];
            for (int i = 0; i < count; i++)
            {
                value[i] = ReadDouble();
            }
            return value;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe DateTime[] ReadDateTimeArray()
        {
            var count = ReadInt32();
            var value = new DateTime[count];
            for (int i = 0; i < count; i++)
            {
                value[i] = ReadDateTime();
            }
            return value;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe TimeSpan[] ReadTimeSpanArray()
        {
            var count = ReadInt32();
            var value = new TimeSpan[count];
            for (int i = 0; i < count; i++)
            {
                value[i] = ReadTimeSpan();
            }
            return value;
        }
#if CORE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe TimeOnly[] ReadTimeOnlyArray()
        {
            var count = ReadInt32();
            var value = new TimeOnly[count];
            for (int i = 0; i < count; i++)
            {
                value[i] = ReadTimeOnly();
            }
            return value;
        }
#endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe DateTimeOffset[] ReadDateTimeOffsetArray()
        {
            var count = ReadInt32();
            var value = new DateTimeOffset[count];
            for (int i = 0; i < count; i++)
            {
                value[i] = ReadDateTimeOffset();
            }
            return value;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe decimal[] ReadDecimalArray()
        {
            var count = ReadInt32();
            var value = new decimal[count];
            for (int i = 0; i < count; i++)
            {
                value[i] = ReadDecimal();
            }
            return value;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe string[] ReadStringArray()
        {
            var count = ReadInt32();
            var value = new string[count];
            for (int i = 0; i < count; i++)
            {
                value[i] = ReadString();
            }
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ICollection ReadGeneric(Type type)
        {
            var count = ReadInt32();
            var value = (ICollection)Activator.CreateInstance(type);
            switch (value)
            {
                case ICollection<byte> value1:
                    for (int i = 0; i < count; i++)
                        value1.Add(ReadByte());
                    break;
                case ICollection<sbyte> value1:
                    for (int i = 0; i < count; i++)
                        value1.Add(ReadSByte());
                    break;
                case ICollection<bool> value1:
                    for (int i = 0; i < count; i++)
                        value1.Add(ReadBoolean());
                    break;
                case ICollection<short> value1:
                    for (int i = 0; i < count; i++)
                        value1.Add(ReadInt16());
                    break;
                case ICollection<ushort> value1:
                    for (int i = 0; i < count; i++)
                        value1.Add(ReadUInt16());
                    break;
                case ICollection<char> value1:
                    for (int i = 0; i < count; i++)
                        value1.Add(ReadChar());
                    break;
                case ICollection<int> value1:
                    for (int i = 0; i < count; i++)
                        value1.Add(ReadInt32());
                    break;
                case ICollection<uint> value1:
                    for (int i = 0; i < count; i++)
                        value1.Add(ReadUInt32());
                    break;
                case ICollection<float> value1:
                    for (int i = 0; i < count; i++)
                        value1.Add(ReadSingle());
                    break;
                case ICollection<long> value1:
                    for (int i = 0; i < count; i++)
                        value1.Add(ReadInt64());
                    break;
                case ICollection<ulong> value1:
                    for (int i = 0; i < count; i++)
                        value1.Add(ReadUInt64());
                    break;
                case ICollection<double> value1:
                    for (int i = 0; i < count; i++)
                        value1.Add(ReadDouble());
                    break;
                case ICollection<decimal> value1:
                    for (int i = 0; i < count; i++)
                        value1.Add(ReadDecimal());
                    break;
                case ICollection<string> value1:
                    for (int i = 0; i < count; i++)
                        value1.Add(ReadString());
                    break;
                case ICollection<TimeSpan> value1:
                    for (int i = 0; i < count; i++)
                        value1.Add(ReadTimeSpan());
                    break;
                case ICollection<DateTime> value1:
                    for (int i = 0; i < count; i++)
                        value1.Add(ReadDateTime());
                    break;
#if CORE
                case ICollection<TimeOnly> value1:
                    for (int i = 0; i < count; i++)
                        value1.Add(ReadTimeOnly());
                    break;
#endif
                case ICollection<DateTimeOffset> value1:
                    for (int i = 0; i < count; i++)
                        value1.Add(ReadDateTimeOffset());
                    break;
                default:
                    throw new Exception("不是基础类型! 请联系作者解决!");
            }
            return value;
        }

        public List<byte> ReadByteList() => ReadByteGeneric<List<byte>>();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ReadByteGeneric<T>() where T : ICollection<byte>, new()
        {
            var count = ReadInt32();
            var value = new T();
            for (int i = 0; i < count; i++)
            {
                value.Add(ReadByte());
            }
            return value;
        }
        public List<sbyte> ReadSByteList() => ReadSByteGeneric<List<sbyte>>();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ReadSByteGeneric<T>() where T : ICollection<sbyte>, new()
        {
            var count = ReadInt32();
            var value = new T();
            for (int i = 0; i < count; i++)
            {
                value.Add(ReadSByte());
            }
            return value;
        }
        public List<bool> ReadBooleanList() => ReadBooleanGeneric<List<bool>>();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ReadBooleanGeneric<T>() where T : ICollection<bool>, new()
        {
            var count = ReadInt32();
            var value = new T();
            for (int i = 0; i < count; i++)
            {
                value.Add(ReadBoolean());
            }
            return value;
        }
        public List<short> ReadInt16List() => ReadInt16Generic<List<short>>();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ReadInt16Generic<T>() where T : ICollection<short>, new()
        {
            var count = ReadInt32();
            var value = new T();
            for (int i = 0; i < count; i++)
            {
                value.Add(ReadInt16());
            }
            return value;
        }
        public List<ushort> ReadUInt16List() => ReadUInt16Generic<List<ushort>>();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ReadUInt16Generic<T>() where T : ICollection<ushort>, new()
        {
            var count = ReadInt32();
            var value = new T();
            for (int i = 0; i < count; i++)
            {
                value.Add(ReadUInt16());
            }
            return value;
        }
        public List<char> ReadCharList() => ReadCharGeneric<List<char>>();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ReadCharGeneric<T>() where T : ICollection<char>, new()
        {
            var count = ReadInt32();
            var value = new T();
            for (int i = 0; i < count; i++)
            {
                value.Add(ReadChar());
            }
            return value;
        }
        public List<int> ReadInt32List() => ReadInt32Generic<List<int>>();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ReadInt32Generic<T>() where T : ICollection<int>, new()
        {
            var count = ReadInt32();
            var value = new T();
            for (int i = 0; i < count; i++)
            {
                value.Add(ReadInt32());
            }
            return value;
        }
        public List<uint> ReadUInt32List() => ReadUInt32Generic<List<uint>>();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ReadUInt32Generic<T>() where T : ICollection<uint>, new()
        {
            var count = ReadInt32();
            var value = new T();
            for (int i = 0; i < count; i++)
            {
                value.Add(ReadUInt32());
            }
            return value;
        }
        public List<float> ReadFloatList() => ReadSingleGeneric<List<float>>();
        public List<float> ReadSingleList() => ReadSingleGeneric<List<float>>();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ReadSingleGeneric<T>() where T : ICollection<float>, new()
        {
            var count = ReadInt32();
            var value = new T();
            for (int i = 0; i < count; i++)
            {
                value.Add(ReadFloat());
            }
            return value;
        }
        public List<long> ReadInt64List() => ReadInt64Generic<List<long>>();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ReadInt64Generic<T>() where T : ICollection<long>, new()
        {
            var count = ReadInt32();
            var value = new T();
            for (int i = 0; i < count; i++)
            {
                value.Add(ReadInt64());
            }
            return value;
        }
        public List<ulong> ReadUInt64List() => ReadUInt64Generic<List<ulong>>();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ReadUInt64Generic<T>() where T : ICollection<ulong>, new()
        {
            var count = ReadInt32();
            var value = new T();
            for (int i = 0; i < count; i++)
            {
                value.Add(ReadUInt64());
            }
            return value;
        }
        public List<double> ReadDoubleList() => ReadDoubleGeneric<List<double>>();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ReadDoubleGeneric<T>() where T : ICollection<double>, new()
        {
            var count = ReadInt32();
            var value = new T();
            for (int i = 0; i < count; i++)
            {
                value.Add(ReadDouble());
            }
            return value;
        }
        public List<DateTime> ReadDateTimeList() => ReadDateTimeGeneric<List<DateTime>>();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ReadDateTimeGeneric<T>() where T : ICollection<DateTime>, new()
        {
            var count = ReadInt32();
            var value = new T();
            for (int i = 0; i < count; i++)
            {
                value.Add(ReadDateTime());
            }
            return value;
        }
        public List<TimeSpan> ReadTimeSpanList() => ReadTimeSpanGeneric<List<TimeSpan>>();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ReadTimeSpanGeneric<T>() where T : ICollection<TimeSpan>, new()
        {
            var count = ReadInt32();
            var value = new T();
            for (int i = 0; i < count; i++)
            {
                value.Add(ReadTimeSpan());
            }
            return value;
        }
#if CORE
        public List<TimeOnly> ReadTimeOnlyList() => ReadTimeOnlyGeneric<List<TimeOnly>>();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ReadTimeOnlyGeneric<T>() where T : ICollection<TimeOnly>, new()
        {
            var count = ReadInt32();
            var value = new T();
            for (int i = 0; i < count; i++)
            {
                value.Add(ReadTimeOnly());
            }
            return value;
        }
#endif
        public List<DateTimeOffset> ReadDateTimeOffsetList() => ReadDateTimeOffsetGeneric<List<DateTimeOffset>>();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ReadDateTimeOffsetGeneric<T>() where T : ICollection<DateTimeOffset>, new()
        {
            var count = ReadInt32();
            var value = new T();
            for (int i = 0; i < count; i++)
            {
                value.Add(ReadDateTimeOffset());
            }
            return value;
        }
        public List<decimal> ReadDecimalList() => ReadDecimalGeneric<List<decimal>>();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ReadDecimalGeneric<T>() where T : ICollection<decimal>, new()
        {
            var count = ReadInt32();
            var value = new T();
            for (int i = 0; i < count; i++)
            {
                value.Add(ReadDecimal());
            }
            return value;
        }
        public List<string> ReadStringList() => ReadStringGeneric<List<string>>();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ReadStringGeneric<T>() where T : ICollection<string>, new()
        {
            var count = ReadInt32();
            var value = new T();
            for (int i = 0; i < count; i++)
            {
                value.Add(ReadString());
            }
            return value;
        }
        #endregion

        public virtual void Flush(bool resetPos = true)
        {
            var count = position - Offset; //当存在偏移后要处理，否则错乱
            if (count > Count)
                Count = count;
            if (resetPos)
                position = Offset;
        }
    }
}