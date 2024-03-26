using Net.System;
using Net.Serialize;

namespace Binding
{
    #region "基元类型绑定"
    public readonly struct SystemByteBind : ISerialize<System.Byte>, ISerialize
    {
        public ushort HashCode { get { return 1; } }
        public void Write(System.Byte value, ISegment stream)
        {
            stream.Write(value);
        }
        public System.Byte Read(ISegment stream)
        {
            return stream.ReadByte();
        }

        public void WriteValue(object value, ISegment stream)
        {
            stream.Write((System.Byte)value);
        }

        object ISerialize.ReadValue(ISegment stream)
        {
            return stream.ReadByte();
        }

        public void Bind()
        {
            SerializeCache<System.Byte>.Serialize = this;
        }
    }
    public readonly struct SystemSByteBind : ISerialize<System.SByte>, ISerialize
    {
        public ushort HashCode { get { return 2; } }
        public void Write(System.SByte value, ISegment stream)
        {
            stream.Write(value);
        }
        public System.SByte Read(ISegment stream)
        {
            return stream.ReadSByte();
        }

        public void WriteValue(object value, ISegment stream)
        {
            stream.Write((System.SByte)value);
        }

        object ISerialize.ReadValue(ISegment stream)
        {
            return stream.ReadSByte();
        }

        public void Bind()
        {
            SerializeCache<System.SByte>.Serialize = this;
        }
    }
    public readonly struct SystemBooleanBind : ISerialize<System.Boolean>, ISerialize
    {
        public ushort HashCode { get { return 3; } }
        public void Write(System.Boolean value, ISegment stream)
        {
            stream.Write(value);
        }
        public System.Boolean Read(ISegment stream)
        {
            return stream.ReadBoolean();
        }

        public void WriteValue(object value, ISegment stream)
        {
            stream.Write((System.Boolean)value);
        }

        object ISerialize.ReadValue(ISegment stream)
        {
            return stream.ReadBoolean();
        }

        public void Bind()
        {
            SerializeCache<System.Boolean>.Serialize = this;
        }
    }
    public readonly struct SystemInt16Bind : ISerialize<System.Int16>, ISerialize
    {
        public ushort HashCode { get { return 4; } }
        public void Write(System.Int16 value, ISegment stream)
        {
            stream.Write(value);
        }
        public System.Int16 Read(ISegment stream)
        {
            return stream.ReadInt16();
        }

        public void WriteValue(object value, ISegment stream)
        {
            stream.Write((System.Int16)value);
        }

        object ISerialize.ReadValue(ISegment stream)
        {
            return stream.ReadInt16();
        }

        public void Bind()
        {
            SerializeCache<System.Int16>.Serialize = this;
        }
    }
    public readonly struct SystemUInt16Bind : ISerialize<System.UInt16>, ISerialize
    {
        public ushort HashCode { get { return 5; } }
        public void Write(System.UInt16 value, ISegment stream)
        {
            stream.Write(value);
        }
        public System.UInt16 Read(ISegment stream)
        {
            return stream.ReadUInt16();
        }

        public void WriteValue(object value, ISegment stream)
        {
            stream.Write((System.UInt16)value);
        }

        object ISerialize.ReadValue(ISegment stream)
        {
            return stream.ReadUInt16();
        }

        public void Bind()
        {
            SerializeCache<System.UInt16>.Serialize = this;
        }
    }
    public readonly struct SystemCharBind : ISerialize<System.Char>, ISerialize
    {
        public ushort HashCode { get { return 6; } }
        public void Write(System.Char value, ISegment stream)
        {
            stream.Write(value);
        }
        public System.Char Read(ISegment stream)
        {
            return stream.ReadChar();
        }

        public void WriteValue(object value, ISegment stream)
        {
            stream.Write((System.Char)value);
        }

        object ISerialize.ReadValue(ISegment stream)
        {
            return stream.ReadChar();
        }

        public void Bind()
        {
            SerializeCache<System.Char>.Serialize = this;
        }
    }
    public readonly struct SystemInt32Bind : ISerialize<System.Int32>, ISerialize
    {
        public ushort HashCode { get { return 7; } }
        public void Write(System.Int32 value, ISegment stream)
        {
            stream.Write(value);
        }
        public System.Int32 Read(ISegment stream)
        {
            return stream.ReadInt32();
        }

        public void WriteValue(object value, ISegment stream)
        {
            stream.Write((System.Int32)value);
        }

        object ISerialize.ReadValue(ISegment stream)
        {
            return stream.ReadInt32();
        }

        public void Bind()
        {
            SerializeCache<System.Int32>.Serialize = this;
        }
    }
    public readonly struct SystemUInt32Bind : ISerialize<System.UInt32>, ISerialize
    {
        public ushort HashCode { get { return 8; } }
        public void Write(System.UInt32 value, ISegment stream)
        {
            stream.Write(value);
        }
        public System.UInt32 Read(ISegment stream)
        {
            return stream.ReadUInt32();
        }

        public void WriteValue(object value, ISegment stream)
        {
            stream.Write((System.UInt32)value);
        }

        object ISerialize.ReadValue(ISegment stream)
        {
            return stream.ReadUInt32();
        }

        public void Bind()
        {
            SerializeCache<System.UInt32>.Serialize = this;
        }
    }
    public readonly struct SystemSingleBind : ISerialize<System.Single>, ISerialize
    {
        public ushort HashCode { get { return 9; } }
        public void Write(System.Single value, ISegment stream)
        {
            stream.Write(value);
        }
        public System.Single Read(ISegment stream)
        {
            return stream.ReadSingle();
        }

        public void WriteValue(object value, ISegment stream)
        {
            stream.Write((System.Single)value);
        }

        object ISerialize.ReadValue(ISegment stream)
        {
            return stream.ReadSingle();
        }

        public void Bind()
        {
            SerializeCache<System.Single>.Serialize = this;
        }
    }
    public readonly struct SystemInt64Bind : ISerialize<System.Int64>, ISerialize
    {
        public ushort HashCode { get { return 10; } }
        public void Write(System.Int64 value, ISegment stream)
        {
            stream.Write(value);
        }
        public System.Int64 Read(ISegment stream)
        {
            return stream.ReadInt64();
        }

        public void WriteValue(object value, ISegment stream)
        {
            stream.Write((System.Int64)value);
        }

        object ISerialize.ReadValue(ISegment stream)
        {
            return stream.ReadInt64();
        }

        public void Bind()
        {
            SerializeCache<System.Int64>.Serialize = this;
        }
    }
    public readonly struct SystemUInt64Bind : ISerialize<System.UInt64>, ISerialize
    {
        public ushort HashCode { get { return 11; } }
        public void Write(System.UInt64 value, ISegment stream)
        {
            stream.Write(value);
        }
        public System.UInt64 Read(ISegment stream)
        {
            return stream.ReadUInt64();
        }

        public void WriteValue(object value, ISegment stream)
        {
            stream.Write((System.UInt64)value);
        }

        object ISerialize.ReadValue(ISegment stream)
        {
            return stream.ReadUInt64();
        }

        public void Bind()
        {
            SerializeCache<System.UInt64>.Serialize = this;
        }
    }
    public readonly struct SystemDoubleBind : ISerialize<System.Double>, ISerialize
    {
        public ushort HashCode { get { return 12; } }
        public void Write(System.Double value, ISegment stream)
        {
            stream.Write(value);
        }
        public System.Double Read(ISegment stream)
        {
            return stream.ReadDouble();
        }

        public void WriteValue(object value, ISegment stream)
        {
            stream.Write((System.Double)value);
        }

        object ISerialize.ReadValue(ISegment stream)
        {
            return stream.ReadDouble();
        }

        public void Bind()
        {
            SerializeCache<System.Double>.Serialize = this;
        }
    }
    public readonly struct SystemStringBind : ISerialize<System.String>, ISerialize
    {
        public ushort HashCode { get { return 13; } }
        public void Write(System.String value, ISegment stream)
        {
            stream.Write(value);
        }
        public System.String Read(ISegment stream)
        {
            return stream.ReadString();
        }

        public void WriteValue(object value, ISegment stream)
        {
            stream.Write((System.String)value);
        }

        object ISerialize.ReadValue(ISegment stream)
        {
            return stream.ReadString();
        }

        public void Bind()
        {
            SerializeCache<System.String>.Serialize = this;
        }
    }
    public readonly struct SystemDecimalBind : ISerialize<System.Decimal>, ISerialize
    {
        public ushort HashCode { get { return 14; } }
        public void Write(System.Decimal value, ISegment stream)
        {
            stream.Write(value);
        }
        public System.Decimal Read(ISegment stream)
        {
            return stream.ReadDecimal();
        }

        public void WriteValue(object value, ISegment stream)
        {
            stream.Write((System.Decimal)value);
        }

        object ISerialize.ReadValue(ISegment stream)
        {
            return stream.ReadDecimal();
        }

        public void Bind()
        {
            SerializeCache<System.Decimal>.Serialize = this;
        }
    }
    public readonly struct SystemDateTimeBind : ISerialize<System.DateTime>, ISerialize
    {
        public ushort HashCode { get { return 15; } }
        public void Write(System.DateTime value, ISegment stream)
        {
            stream.Write(value);
        }
        public System.DateTime Read(ISegment stream)
        {
            return stream.ReadDateTime();
        }

        public void WriteValue(object value, ISegment stream)
        {
            stream.Write((System.DateTime)value);
        }

        object ISerialize.ReadValue(ISegment stream)
        {
            return stream.ReadDateTime();
        }

        public void Bind()
        {
            SerializeCache<System.DateTime>.Serialize = this;
        }
    }
    public readonly struct SystemTimeSpanBind : ISerialize<System.TimeSpan>, ISerialize
    {
        public ushort HashCode { get { return 16; } }
        public void Write(System.TimeSpan value, ISegment stream)
        {
            stream.Write(value);
        }
        public System.TimeSpan Read(ISegment stream)
        {
            return stream.ReadTimeSpan();
        }

        public void WriteValue(object value, ISegment stream)
        {
            stream.Write((System.TimeSpan)value);
        }

        object ISerialize.ReadValue(ISegment stream)
        {
            return stream.ReadTimeSpan();
        }

        public void Bind()
        {
            SerializeCache<System.TimeSpan>.Serialize = this;
        }
    }
#if CORE
    public readonly struct SystemTimeOnlyBind : ISerialize<System.TimeOnly>, ISerialize
    {
        public ushort HashCode { get { return 17; } }
        public void Write(System.TimeOnly value, ISegment stream)
        {
            stream.Write(value);
        }
        public System.TimeOnly Read(ISegment stream)
        {
            return stream.ReadTimeOnly();
        }

        public void WriteValue(object value, ISegment stream)
        {
            stream.Write((System.TimeOnly)value);
        }

        object ISerialize.ReadValue(ISegment stream)
        {
            return stream.ReadTimeOnly();
        }

        public void Bind()
        {
            SerializeCache<System.TimeOnly>.Serialize = this;
        }
    }
#endif
    public readonly struct SystemDateTimeOffsetBind : ISerialize<System.DateTimeOffset>, ISerialize
    {
        public ushort HashCode { get { return 18; } }
        public void Write(System.DateTimeOffset value, ISegment stream)
        {
            stream.Write(value);
        }
        public System.DateTimeOffset Read(ISegment stream)
        {
            return stream.ReadDateTimeOffset();
        }

        public void WriteValue(object value, ISegment stream)
        {
            stream.Write((System.DateTimeOffset)value);
        }

        object ISerialize.ReadValue(ISegment stream)
        {
            return stream.ReadDateTimeOffset();
        }

        public void Bind()
        {
            SerializeCache<System.DateTimeOffset>.Serialize = this;
        }
    }
    public readonly struct SystemDBNullBind : ISerialize<System.DBNull>, ISerialize //这个类用作Null参数, 不需要写入和返回null即可
    {
        public ushort HashCode { get { return 19; } }
        public void Write(System.DBNull value, ISegment stream)
        {
        }
        public System.DBNull Read(ISegment stream)
        {
            return null;
        }

        public void WriteValue(object value, ISegment stream)
        {
        }

        object ISerialize.ReadValue(ISegment stream)
        {
            return null;
        }

        public void Bind()
        {
            SerializeCache<System.DBNull>.Serialize = this;
        }
    }
    public class SystemEnumBind<T> : ISerialize<T>, ISerialize where T : System.Enum
    {
        public ushort HashCode { get; private set; }
        public void Write(T value, ISegment stream)
        {
            stream.Write(value);
        }
        public T Read(ISegment stream)
        {
            return stream.ReadEnum<T>();
        }

        public void WriteValue(object value, ISegment stream)
        {
            stream.Write((T)value);
        }

        object ISerialize.ReadValue(ISegment stream)
        {
            return stream.ReadEnum<T>();
        }

        public void Bind()
        {
            HashCode = EnumHashCodeBind.BaseHashCode++;
            SerializeCache<T>.Serialize = this;
        }
    }
    #endregion

    #region "基元类型数组绑定"
    public readonly struct SystemByteArrayBind : ISerialize<System.Byte[]>, ISerialize
    {
        public ushort HashCode { get { return 21; } }
        public void Write(System.Byte[] value, ISegment stream)
        {
            stream.Write(value);
        }

        public System.Byte[] Read(ISegment stream)
        {
            return stream.ReadByteArray();
        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((System.Byte[])value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<System.Byte[]>.Serialize = this;
        }
    }
    public readonly struct SystemSByteArrayBind : ISerialize<System.SByte[]>, ISerialize
    {
        public ushort HashCode { get { return 22; } }
        public void Write(System.SByte[] value, ISegment stream)
        {
            stream.Write(value);
        }

        public System.SByte[] Read(ISegment stream)
        {
            return stream.ReadSByteArray();
        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((System.SByte[])value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<System.SByte[]>.Serialize = this;
        }
    }
    public readonly struct SystemBooleanArrayBind : ISerialize<System.Boolean[]>, ISerialize
    {
        public ushort HashCode { get { return 23; } }
        public void Write(System.Boolean[] value, ISegment stream)
        {
            stream.Write(value);
        }

        public System.Boolean[] Read(ISegment stream)
        {
            return stream.ReadBooleanArray();
        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((System.Boolean[])value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<System.Boolean[]>.Serialize = this;
        }
    }
    public readonly struct SystemInt16ArrayBind : ISerialize<System.Int16[]>, ISerialize
    {
        public ushort HashCode { get { return 24; } }
        public void Write(System.Int16[] value, ISegment stream)
        {
            stream.Write(value);
        }

        public System.Int16[] Read(ISegment stream)
        {
            return stream.ReadInt16Array();
        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((System.Int16[])value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<System.Int16[]>.Serialize = this;
        }
    }
    public readonly struct SystemUInt16ArrayBind : ISerialize<System.UInt16[]>, ISerialize
    {
        public ushort HashCode { get { return 25; } }
        public void Write(System.UInt16[] value, ISegment stream)
        {
            stream.Write(value);
        }

        public System.UInt16[] Read(ISegment stream)
        {
            return stream.ReadUInt16Array();
        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((System.UInt16[])value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<System.UInt16[]>.Serialize = this;
        }
    }
    public readonly struct SystemCharArrayBind : ISerialize<System.Char[]>, ISerialize
    {
        public ushort HashCode { get { return 26; } }
        public void Write(System.Char[] value, ISegment stream)
        {
            stream.Write(value);
        }

        public System.Char[] Read(ISegment stream)
        {
            return stream.ReadCharArray();
        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((System.Char[])value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<System.Char[]>.Serialize = this;
        }
    }
    public readonly struct SystemInt32ArrayBind : ISerialize<System.Int32[]>, ISerialize
    {
        public ushort HashCode { get { return 27; } }
        public void Write(System.Int32[] value, ISegment stream)
        {
            stream.Write(value);
        }

        public System.Int32[] Read(ISegment stream)
        {
            return stream.ReadInt32Array();
        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((System.Int32[])value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<System.Int32[]>.Serialize = this;
        }
    }
    public readonly struct SystemUInt32ArrayBind : ISerialize<System.UInt32[]>, ISerialize
    {
        public ushort HashCode { get { return 28; } }
        public void Write(System.UInt32[] value, ISegment stream)
        {
            stream.Write(value);
        }

        public System.UInt32[] Read(ISegment stream)
        {
            return stream.ReadUInt32Array();
        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((System.UInt32[])value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<System.UInt32[]>.Serialize = this;
        }
    }
    public readonly struct SystemSingleArrayBind : ISerialize<System.Single[]>, ISerialize
    {
        public ushort HashCode { get { return 29; } }
        public void Write(System.Single[] value, ISegment stream)
        {
            stream.Write(value);
        }

        public System.Single[] Read(ISegment stream)
        {
            return stream.ReadSingleArray();
        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((System.Single[])value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<System.Single[]>.Serialize = this;
        }
    }
    public readonly struct SystemInt64ArrayBind : ISerialize<System.Int64[]>, ISerialize
    {
        public ushort HashCode { get { return 30; } }
        public void Write(System.Int64[] value, ISegment stream)
        {
            stream.Write(value);
        }

        public System.Int64[] Read(ISegment stream)
        {
            return stream.ReadInt64Array();
        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((System.Int64[])value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<System.Int64[]>.Serialize = this;
        }
    }
    public readonly struct SystemUInt64ArrayBind : ISerialize<System.UInt64[]>, ISerialize
    {
        public ushort HashCode { get { return 31; } }
        public void Write(System.UInt64[] value, ISegment stream)
        {
            stream.Write(value);
        }

        public System.UInt64[] Read(ISegment stream)
        {
            return stream.ReadUInt64Array();
        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((System.UInt64[])value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<System.UInt64[]>.Serialize = this;
        }
    }
    public readonly struct SystemDoubleArrayBind : ISerialize<System.Double[]>, ISerialize
    {
        public ushort HashCode { get { return 32; } }
        public void Write(System.Double[] value, ISegment stream)
        {
            stream.Write(value);
        }

        public System.Double[] Read(ISegment stream)
        {
            return stream.ReadDoubleArray();
        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((System.Double[])value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<System.Double[]>.Serialize = this;
        }
    }
    public readonly struct SystemStringArrayBind : ISerialize<System.String[]>, ISerialize
    {
        public ushort HashCode { get { return 33; } }
        public void Write(System.String[] value, ISegment stream)
        {
            stream.Write(value);
        }

        public System.String[] Read(ISegment stream)
        {
            return stream.ReadStringArray();
        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((System.String[])value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<System.String[]>.Serialize = this;
        }
    }
    public readonly struct SystemDecimalArrayBind : ISerialize<System.Decimal[]>, ISerialize
    {
        public ushort HashCode { get { return 34; } }
        public void Write(System.Decimal[] value, ISegment stream)
        {
            stream.Write(value);
        }

        public System.Decimal[] Read(ISegment stream)
        {
            return stream.ReadDecimalArray();
        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((System.Decimal[])value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<System.Decimal[]>.Serialize = this;
        }
    }
    public readonly struct SystemDateTimeArrayBind : ISerialize<System.DateTime[]>, ISerialize
    {
        public ushort HashCode { get { return 35; } }
        public void Write(System.DateTime[] value, ISegment stream)
        {
            stream.Write(value);
        }

        public System.DateTime[] Read(ISegment stream)
        {
            return stream.ReadDateTimeArray();
        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((System.DateTime[])value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<System.DateTime[]>.Serialize = this;
        }
    }
    public readonly struct SystemTimeSpanArrayBind : ISerialize<System.TimeSpan[]>, ISerialize
    {
        public ushort HashCode { get { return 36; } }
        public void Write(System.TimeSpan[] value, ISegment stream)
        {
            stream.Write(value);
        }

        public System.TimeSpan[] Read(ISegment stream)
        {
            return stream.ReadTimeSpanArray();
        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((System.TimeSpan[])value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<System.TimeSpan[]>.Serialize = this;
        }
    }
#if CORE
    public readonly struct SystemTimeOnlyArrayBind : ISerialize<System.TimeOnly[]>, ISerialize
    {
        public ushort HashCode { get { return 37; } }
        public void Write(System.TimeOnly[] value, ISegment stream)
        {
            stream.Write(value);
        }

        public System.TimeOnly[] Read(ISegment stream)
        {
            return stream.ReadTimeOnlyArray();
        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((System.TimeOnly[])value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<System.TimeOnly[]>.Serialize = this;
        }
    }
#endif
    public readonly struct SystemDateTimeOffsetArrayBind : ISerialize<System.DateTimeOffset[]>, ISerialize
    {
        public ushort HashCode { get { return 38; } }
        public void Write(System.DateTimeOffset[] value, ISegment stream)
        {
            stream.Write(value);
        }

        public System.DateTimeOffset[] Read(ISegment stream)
        {
            return stream.ReadDateTimeOffsetArray();
        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((System.DateTimeOffset[])value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<System.DateTimeOffset[]>.Serialize = this;
        }
    }
    public readonly struct SystemDBNullArrayBind : ISerialize<System.DBNull[]>, ISerialize //这个类用作Null参数, 不需要写入和返回null即可
    {
        public ushort HashCode { get { return 39; } }
        public void Write(System.DBNull[] value, ISegment stream)
        {
        }
        public System.DBNull[] Read(ISegment stream)
        {
            return null;
        }

        public void WriteValue(object value, ISegment stream)
        {
        }

        object ISerialize.ReadValue(ISegment stream)
        {
            return null;
        }

        public void Bind()
        {
            SerializeCache<System.DBNull[]>.Serialize = this;
        }
    }
    public class SystemEnumArrayBind<T> : ISerialize<T[]>, ISerialize where T : System.Enum
    {
        public ushort HashCode { get; private set; }
        public void Write(T[] value, ISegment stream)
        {
            stream.Write(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                stream.Write(value[i]);
            }
        }
        public T[] Read(ISegment stream)
        {
            var count = stream.ReadInt32();
            var value = new T[count];
            for (int i = 0; i < count; i++)
            {
                value[i] = stream.ReadEnum<T>();
            }
            return value;
        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((T[])value, stream);
        }

        object ISerialize.ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            HashCode = EnumHashCodeBind.ArrayHashCode++;
            SerializeCache<T[]>.Serialize = this;
        }
    }
    #endregion

    #region "基元类型List绑定"
    public readonly struct SystemCollectionsGenericListSystemByteBind : ISerialize<System.Collections.Generic.List<System.Byte>>, ISerialize
    {
        public ushort HashCode { get { return 41; } }
        public void Write(System.Collections.Generic.List<System.Byte> value, ISegment stream)
        {
            stream.Write(value);
        }

        public System.Collections.Generic.List<System.Byte> Read(ISegment stream)
        {
            return stream.ReadByteList();
        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((System.Collections.Generic.List<System.Byte>)value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<System.Collections.Generic.List<System.Byte>>.Serialize = this;
        }
    }
    public readonly struct SystemCollectionsGenericListSystemSByteBind : ISerialize<System.Collections.Generic.List<System.SByte>>, ISerialize
    {
        public ushort HashCode { get { return 42; } }
        public void Write(System.Collections.Generic.List<System.SByte> value, ISegment stream)
        {
            stream.Write(value);
        }

        public System.Collections.Generic.List<System.SByte> Read(ISegment stream)
        {
            return stream.ReadSByteList();
        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((System.Collections.Generic.List<System.SByte>)value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<System.Collections.Generic.List<System.SByte>>.Serialize = this;
        }
    }
    public readonly struct SystemCollectionsGenericListSystemBooleanBind : ISerialize<System.Collections.Generic.List<System.Boolean>>, ISerialize
    {
        public ushort HashCode { get { return 43; } }
        public void Write(System.Collections.Generic.List<System.Boolean> value, ISegment stream)
        {
            stream.Write(value);
        }

        public System.Collections.Generic.List<System.Boolean> Read(ISegment stream)
        {
            return stream.ReadBooleanList();
        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((System.Collections.Generic.List<System.Boolean>)value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<System.Collections.Generic.List<System.Boolean>>.Serialize = this;
        }
    }
    public readonly struct SystemCollectionsGenericListSystemInt16Bind : ISerialize<System.Collections.Generic.List<System.Int16>>, ISerialize
    {
        public ushort HashCode { get { return 44; } }
        public void Write(System.Collections.Generic.List<System.Int16> value, ISegment stream)
        {
            stream.Write(value);
        }

        public System.Collections.Generic.List<System.Int16> Read(ISegment stream)
        {
            return stream.ReadInt16List();
        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((System.Collections.Generic.List<System.Int16>)value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<System.Collections.Generic.List<System.Int16>>.Serialize = this;
        }
    }
    public readonly struct SystemCollectionsGenericListSystemUInt16Bind : ISerialize<System.Collections.Generic.List<System.UInt16>>, ISerialize
    {
        public ushort HashCode { get { return 45; } }
        public void Write(System.Collections.Generic.List<System.UInt16> value, ISegment stream)
        {
            stream.Write(value);
        }

        public System.Collections.Generic.List<System.UInt16> Read(ISegment stream)
        {
            return stream.ReadUInt16List();
        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((System.Collections.Generic.List<System.UInt16>)value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<System.Collections.Generic.List<System.UInt16>>.Serialize = this;
        }
    }
    public readonly struct SystemCollectionsGenericListSystemCharBind : ISerialize<System.Collections.Generic.List<System.Char>>, ISerialize
    {
        public ushort HashCode { get { return 46; } }
        public void Write(System.Collections.Generic.List<System.Char> value, ISegment stream)
        {
            stream.Write(value);
        }

        public System.Collections.Generic.List<System.Char> Read(ISegment stream)
        {
            return stream.ReadCharList();
        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((System.Collections.Generic.List<System.Char>)value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<System.Collections.Generic.List<System.Char>>.Serialize = this;
        }
    }
    public readonly struct SystemCollectionsGenericListSystemInt32Bind : ISerialize<System.Collections.Generic.List<System.Int32>>, ISerialize
    {
        public ushort HashCode { get { return 47; } }
        public void Write(System.Collections.Generic.List<System.Int32> value, ISegment stream)
        {
            stream.Write(value);
        }

        public System.Collections.Generic.List<System.Int32> Read(ISegment stream)
        {
            return stream.ReadInt32List();
        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((System.Collections.Generic.List<System.Int32>)value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<System.Collections.Generic.List<System.Int32>>.Serialize = this;
        }
    }
    public readonly struct SystemCollectionsGenericListSystemUInt32Bind : ISerialize<System.Collections.Generic.List<System.UInt32>>, ISerialize
    {
        public ushort HashCode { get { return 48; } }
        public void Write(System.Collections.Generic.List<System.UInt32> value, ISegment stream)
        {
            stream.Write(value);
        }

        public System.Collections.Generic.List<System.UInt32> Read(ISegment stream)
        {
            return stream.ReadUInt32List();
        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((System.Collections.Generic.List<System.UInt32>)value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<System.Collections.Generic.List<System.UInt32>>.Serialize = this;
        }
    }
    public readonly struct SystemCollectionsGenericListSystemSingleBind : ISerialize<System.Collections.Generic.List<System.Single>>, ISerialize
    {
        public ushort HashCode { get { return 49; } }
        public void Write(System.Collections.Generic.List<System.Single> value, ISegment stream)
        {
            stream.Write(value);
        }

        public System.Collections.Generic.List<System.Single> Read(ISegment stream)
        {
            return stream.ReadSingleList();
        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((System.Collections.Generic.List<System.Single>)value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<System.Collections.Generic.List<System.Single>>.Serialize = this;
        }
    }
    public readonly struct SystemCollectionsGenericListSystemInt64Bind : ISerialize<System.Collections.Generic.List<System.Int64>>, ISerialize
    {
        public ushort HashCode { get { return 50; } }
        public void Write(System.Collections.Generic.List<System.Int64> value, ISegment stream)
        {
            stream.Write(value);
        }

        public System.Collections.Generic.List<System.Int64> Read(ISegment stream)
        {
            return stream.ReadInt64List();
        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((System.Collections.Generic.List<System.Int64>)value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<System.Collections.Generic.List<System.Int64>>.Serialize = this;
        }
    }
    public readonly struct SystemCollectionsGenericListSystemUInt64Bind : ISerialize<System.Collections.Generic.List<System.UInt64>>, ISerialize
    {
        public ushort HashCode { get { return 51; } }
        public void Write(System.Collections.Generic.List<System.UInt64> value, ISegment stream)
        {
            stream.Write(value);
        }

        public System.Collections.Generic.List<System.UInt64> Read(ISegment stream)
        {
            return stream.ReadUInt64List();
        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((System.Collections.Generic.List<System.UInt64>)value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<System.Collections.Generic.List<System.UInt64>>.Serialize = this;
        }
    }
    public readonly struct SystemCollectionsGenericListSystemDoubleBind : ISerialize<System.Collections.Generic.List<System.Double>>, ISerialize
    {
        public ushort HashCode { get { return 52; } }
        public void Write(System.Collections.Generic.List<System.Double> value, ISegment stream)
        {
            stream.Write(value);
        }

        public System.Collections.Generic.List<System.Double> Read(ISegment stream)
        {
            return stream.ReadDoubleList();
        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((System.Collections.Generic.List<System.Double>)value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<System.Collections.Generic.List<System.Double>>.Serialize = this;
        }
    }
    public readonly struct SystemCollectionsGenericListSystemStringBind : ISerialize<System.Collections.Generic.List<System.String>>, ISerialize
    {
        public ushort HashCode { get { return 53; } }
        public void Write(System.Collections.Generic.List<System.String> value, ISegment stream)
        {
            stream.Write(value);
        }

        public System.Collections.Generic.List<System.String> Read(ISegment stream)
        {
            return stream.ReadStringList();
        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((System.Collections.Generic.List<System.String>)value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<System.Collections.Generic.List<System.String>>.Serialize = this;
        }
    }
    public readonly struct SystemCollectionsGenericListSystemDecimalBind : ISerialize<System.Collections.Generic.List<System.Decimal>>, ISerialize
    {
        public ushort HashCode { get { return 54; } }
        public void Write(System.Collections.Generic.List<System.Decimal> value, ISegment stream)
        {
            stream.Write(value);
        }

        public System.Collections.Generic.List<System.Decimal> Read(ISegment stream)
        {
            return stream.ReadDecimalList();
        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((System.Collections.Generic.List<System.Decimal>)value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<System.Collections.Generic.List<System.Decimal>>.Serialize = this;
        }
    }
    public readonly struct SystemCollectionsGenericListSystemDateTimeBind : ISerialize<System.Collections.Generic.List<System.DateTime>>, ISerialize
    {
        public ushort HashCode { get { return 55; } }
        public void Write(System.Collections.Generic.List<System.DateTime> value, ISegment stream)
        {
            stream.Write(value);
        }

        public System.Collections.Generic.List<System.DateTime> Read(ISegment stream)
        {
            return stream.ReadDateTimeList();
        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((System.Collections.Generic.List<System.DateTime>)value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<System.Collections.Generic.List<System.DateTime>>.Serialize = this;
        }
    }
    public readonly struct SystemCollectionsGenericListSystemTimeSpanBind : ISerialize<System.Collections.Generic.List<System.TimeSpan>>, ISerialize
    {
        public ushort HashCode { get { return 56; } }
        public void Write(System.Collections.Generic.List<System.TimeSpan> value, ISegment stream)
        {
            stream.Write(value);
        }

        public System.Collections.Generic.List<System.TimeSpan> Read(ISegment stream)
        {
            return stream.ReadTimeSpanList();
        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((System.Collections.Generic.List<System.TimeSpan>)value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<System.Collections.Generic.List<System.TimeSpan>>.Serialize = this;
        }
    }
#if CORE
    public readonly struct SystemCollectionsGenericListSystemTimeOnlyBind : ISerialize<System.Collections.Generic.List<System.TimeOnly>>, ISerialize
    {
        public ushort HashCode { get { return 57; } }
        public void Write(System.Collections.Generic.List<System.TimeOnly> value, ISegment stream)
        {
            stream.Write(value);
        }

        public System.Collections.Generic.List<System.TimeOnly> Read(ISegment stream)
        {
            return stream.ReadTimeOnlyList();
        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((System.Collections.Generic.List<System.TimeOnly>)value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<System.Collections.Generic.List<System.TimeOnly>>.Serialize = this;
        }
    }
#endif
    public readonly struct SystemCollectionsGenericListSystemDateTimeOffsetBind : ISerialize<System.Collections.Generic.List<System.DateTimeOffset>>, ISerialize
    {
        public ushort HashCode { get { return 58; } }
        public void Write(System.Collections.Generic.List<System.DateTimeOffset> value, ISegment stream)
        {
            stream.Write(value);
        }

        public System.Collections.Generic.List<System.DateTimeOffset> Read(ISegment stream)
        {
            return stream.ReadDateTimeOffsetList();
        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((System.Collections.Generic.List<System.DateTimeOffset>)value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<System.Collections.Generic.List<System.DateTimeOffset>>.Serialize = this;
        }
    }
    public readonly struct SystemCollectionsGenericListSystemDBNullBind : ISerialize<System.Collections.Generic.List<System.DBNull>>, ISerialize //这个类用作Null参数, 不需要写入和返回null即可
    {
        public ushort HashCode { get { return 59; } }
        public void Write(System.Collections.Generic.List<System.DBNull> value, ISegment stream)
        {
        }
        public System.Collections.Generic.List<System.DBNull> Read(ISegment stream)
        {
            return null;
        }

        public void WriteValue(object value, ISegment stream)
        {
        }

        object ISerialize.ReadValue(ISegment stream)
        {
            return null;
        }

        public void Bind()
        {
            SerializeCache<System.Collections.Generic.List<System.DBNull>>.Serialize = this;
        }
    }
    public class SystemCollectionsGenericListSystemEnumBind<T> : ISerialize<System.Collections.Generic.List<T>>, ISerialize where T : System.Enum
    {

        public ushort HashCode { get; private set; }
        public void Write(System.Collections.Generic.List<T> value, ISegment stream)
        {
            stream.Write(value.Count);
            for (int i = 0; i < value.Count; i++)
            {
                stream.Write(value[i]);
            }
        }
        public System.Collections.Generic.List<T> Read(ISegment stream)
        {
            var count = stream.ReadInt32();
            var value = new System.Collections.Generic.List<T>(count);
            for (int i = 0; i < count; i++)
            {
                value.Add(stream.ReadEnum<T>());
            }
            return value;
        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((System.Collections.Generic.List<T>)value, stream);
        }

        object ISerialize.ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            HashCode = EnumHashCodeBind.GenericHashCode++;
            SerializeCache<System.Collections.Generic.List<T>>.Serialize = this;
        }
    }
    #endregion

    static class EnumHashCodeBind
    {
        public static ushort BaseHashCode = 200; //基元类型到框架核心类使用1-156，枚举从200-300，可以定义100个枚举类型
        public static ushort ArrayHashCode = 300; //基元类型到框架核心类使用1-156，枚举从300-400，可以定义100个枚举类型
        public static ushort GenericHashCode = 400; //基元类型到框架核心类使用1-156，枚举从400-500，可以定义100个枚举类型
    }
}