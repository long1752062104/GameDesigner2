using System.Collections.Generic;
using Net.Serialize;
using Net.System;

namespace Binding
{
    /// <summary>
    /// 基础类型绑定
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct BaseBind<T> : ISerialize<T>, ISerialize
    {
        public void Write(T value, Segment stream)
        {
            stream.WriteValue(value);
        }
        public T Read(Segment stream)
        {
            return stream.ReadValue<T>();
        }

        public void WriteValue(object value, Segment stream)
        {
            stream.WriteValue(value);
        }

        object ISerialize.ReadValue(Segment stream)
        {
            return stream.ReadValue<T>();
        }
    }

    /// <summary>
    /// 基础类型数组绑定
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct BaseArrayBind<T> : ISerialize<T[]>, ISerialize
    {
        public void Write(T[] value, Segment stream)
        {
            stream.WriteArray(value);
        }
        public T[] Read(Segment stream)
        {
            return stream.ReadArray<T>();
        }

        public void WriteValue(object value, Segment stream)
        {
            stream.WriteArray(value);
        }

        object ISerialize.ReadValue(Segment stream)
        {
            return stream.ReadArray<T>();
        }
    }

    /// <summary>
    /// 基础类型泛型绑定
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct BaseListBind<T> : ISerialize<List<T>>, ISerialize
    {
        public void Write(List<T> value, Segment stream)
        {
            stream.WriteList(value);
        }
        public List<T> Read(Segment stream)
        {
            return stream.ReadList<T>();
        }

        public void WriteValue(object value, Segment stream)
        {
            stream.WriteList(value);
        }

        object ISerialize.ReadValue(Segment stream)
        {
            return stream.ReadList<T>();
        }
    }

    /// <summary>
    /// 字典绑定
    /// </summary>
    public struct DictionaryBind<TKey, TValue>
	{
		public void Write(Dictionary<TKey, TValue> value, Segment stream, ISerialize<TValue> bind)
		{
			int count = value.Count;
			stream.Write(count);
			if (count == 0) return;
			foreach (var value1 in value)
			{
				stream.WriteValue(value1.Key);
				bind.Write(value1.Value, stream);
			}
		}

		public Dictionary<TKey, TValue> Read(Segment stream, ISerialize<TValue> bind)
		{
			var count = stream.ReadInt32();
			var value = new Dictionary<TKey, TValue>();
			if (count == 0) return value;
			for (int i = 0; i < count; i++)
			{
				var key = stream.ReadValue<TKey>();
				var tvalue = bind.Read(stream);
				value.Add(key, tvalue);
			}
			return value;
		}
	}

    /// <summary>
    /// My字典绑定
    /// </summary>
    public struct MyDictionaryBind<TKey, TValue>
    {
        public void Write(MyDictionary<TKey, TValue> value, Segment stream, ISerialize<TValue> bind)
        {
            int count = value.Count;
            stream.Write(count);
            if (count == 0) return;
            foreach (var value1 in value)
            {
                stream.WriteValue(value1.Key);
                bind.Write(value1.Value, stream);
            }
        }

        public MyDictionary<TKey, TValue> Read(Segment stream, ISerialize<TValue> bind)
        {
            var count = stream.ReadInt32();
            var value = new MyDictionary<TKey, TValue>();
            if (count == 0) return value;
            for (int i = 0; i < count; i++)
            {
                var key = stream.ReadValue<TKey>();
                var tvalue = bind.Read(stream);
                value.Add(key, tvalue);
            }
            return value;
        }
    }

    #region "基元类型绑定"
    public struct SystemByteBind : ISerialize<System.Byte>, ISerialize
    {
        public void Write(System.Byte value, Segment stream)
        {
            stream.Write(value);
        }
        public System.Byte Read(Segment stream)
        {
            return stream.ReadByte();
        }

        public void WriteValue(object value, Segment stream)
        {
            stream.Write((System.Byte)value);
        }

        object ISerialize.ReadValue(Segment stream)
        {
            return stream.ReadByte();
        }
    }
    public struct SystemSByteBind : ISerialize<System.SByte>, ISerialize
    {
        public void Write(System.SByte value, Segment stream)
        {
            stream.Write(value);
        }
        public System.SByte Read(Segment stream)
        {
            return stream.ReadSByte();
        }

        public void WriteValue(object value, Segment stream)
        {
            stream.Write((System.SByte)value);
        }

        object ISerialize.ReadValue(Segment stream)
        {
            return stream.ReadSByte();
        }
    }
    public struct SystemBooleanBind : ISerialize<System.Boolean>, ISerialize
    {
        public void Write(System.Boolean value, Segment stream)
        {
            stream.Write(value);
        }
        public System.Boolean Read(Segment stream)
        {
            return stream.ReadBoolean();
        }

        public void WriteValue(object value, Segment stream)
        {
            stream.Write((System.Boolean)value);
        }

        object ISerialize.ReadValue(Segment stream)
        {
            return stream.ReadBoolean();
        }
    }
    public struct SystemInt16Bind : ISerialize<System.Int16>, ISerialize
    {
        public void Write(System.Int16 value, Segment stream)
        {
            stream.Write(value);
        }
        public System.Int16 Read(Segment stream)
        {
            return stream.ReadInt16();
        }

        public void WriteValue(object value, Segment stream)
        {
            stream.Write((System.Int16)value);
        }

        object ISerialize.ReadValue(Segment stream)
        {
            return stream.ReadInt16();
        }
    }
    public struct SystemUInt16Bind : ISerialize<System.UInt16>, ISerialize
    {
        public void Write(System.UInt16 value, Segment stream)
        {
            stream.Write(value);
        }
        public System.UInt16 Read(Segment stream)
        {
            return stream.ReadUInt16();
        }

        public void WriteValue(object value, Segment stream)
        {
            stream.Write((System.UInt16)value);
        }

        object ISerialize.ReadValue(Segment stream)
        {
            return stream.ReadUInt16();
        }
    }
    public struct SystemCharBind : ISerialize<System.Char>, ISerialize
    {
        public void Write(System.Char value, Segment stream)
        {
            stream.Write(value);
        }
        public System.Char Read(Segment stream)
        {
            return stream.ReadChar();
        }

        public void WriteValue(object value, Segment stream)
        {
            stream.Write((System.Char)value);
        }

        object ISerialize.ReadValue(Segment stream)
        {
            return stream.ReadChar();
        }
    }
    public struct SystemInt32Bind : ISerialize<System.Int32>, ISerialize
    {
        public void Write(System.Int32 value, Segment stream)
        {
            stream.Write(value);
        }
        public System.Int32 Read(Segment stream)
        {
            return stream.ReadInt32();
        }

        public void WriteValue(object value, Segment stream)
        {
            stream.Write((System.Int32)value);
        }

        object ISerialize.ReadValue(Segment stream)
        {
            return stream.ReadInt32();
        }
    }
    public struct SystemUInt32Bind : ISerialize<System.UInt32>, ISerialize
    {
        public void Write(System.UInt32 value, Segment stream)
        {
            stream.Write(value);
        }
        public System.UInt32 Read(Segment stream)
        {
            return stream.ReadUInt32();
        }

        public void WriteValue(object value, Segment stream)
        {
            stream.Write((System.UInt32)value);
        }

        object ISerialize.ReadValue(Segment stream)
        {
            return stream.ReadUInt32();
        }
    }
    public struct SystemSingleBind : ISerialize<System.Single>, ISerialize
    {
        public void Write(System.Single value, Segment stream)
        {
            stream.Write(value);
        }
        public System.Single Read(Segment stream)
        {
            return stream.ReadSingle();
        }

        public void WriteValue(object value, Segment stream)
        {
            stream.Write((System.Single)value);
        }

        object ISerialize.ReadValue(Segment stream)
        {
            return stream.ReadSingle();
        }
    }
    public struct SystemInt64Bind : ISerialize<System.Int64>, ISerialize
    {
        public void Write(System.Int64 value, Segment stream)
        {
            stream.Write(value);
        }
        public System.Int64 Read(Segment stream)
        {
            return stream.ReadInt64();
        }

        public void WriteValue(object value, Segment stream)
        {
            stream.Write((System.Int64)value);
        }

        object ISerialize.ReadValue(Segment stream)
        {
            return stream.ReadInt64();
        }
    }
    public struct SystemUInt64Bind : ISerialize<System.UInt64>, ISerialize
    {
        public void Write(System.UInt64 value, Segment stream)
        {
            stream.Write(value);
        }
        public System.UInt64 Read(Segment stream)
        {
            return stream.ReadUInt64();
        }

        public void WriteValue(object value, Segment stream)
        {
            stream.Write((System.UInt64)value);
        }

        object ISerialize.ReadValue(Segment stream)
        {
            return stream.ReadUInt64();
        }
    }
    public struct SystemDoubleBind : ISerialize<System.Double>, ISerialize
    {
        public void Write(System.Double value, Segment stream)
        {
            stream.Write(value);
        }
        public System.Double Read(Segment stream)
        {
            return stream.ReadDouble();
        }

        public void WriteValue(object value, Segment stream)
        {
            stream.Write((System.Double)value);
        }

        object ISerialize.ReadValue(Segment stream)
        {
            return stream.ReadDouble();
        }
    }
    public struct SystemStringBind : ISerialize<System.String>, ISerialize
    {
        public void Write(System.String value, Segment stream)
        {
            stream.Write(value);
        }
        public System.String Read(Segment stream)
        {
            return stream.ReadString();
        }

        public void WriteValue(object value, Segment stream)
        {
            stream.Write((System.String)value);
        }

        object ISerialize.ReadValue(Segment stream)
        {
            return stream.ReadString();
        }
    }
    public struct SystemDecimalBind : ISerialize<System.Decimal>, ISerialize
    {
        public void Write(System.Decimal value, Segment stream)
        {
            stream.Write(value);
        }
        public System.Decimal Read(Segment stream)
        {
            return stream.ReadDecimal();
        }

        public void WriteValue(object value, Segment stream)
        {
            stream.Write((System.Decimal)value);
        }

        object ISerialize.ReadValue(Segment stream)
        {
            return stream.ReadDecimal();
        }
    }
#endregion
}