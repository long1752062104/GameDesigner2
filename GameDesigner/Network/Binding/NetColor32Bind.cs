using Net.Serialize;
using Net.System;
using System;
using System.Collections.Generic;

namespace Binding
{
    public readonly struct NetColor32Bind : ISerialize<Net.Color32>, ISerialize
    {
        public ushort HashCode { get { return 142; } }

        public void Write(Net.Color32 value, ISegment stream)
        {
            int pos = stream.Position;
            stream.Position += 1;
            var bits = new byte[1];

            if (value.r != 0)
            {
                NetConvertBase.SetBit(ref bits[0], 1, true);
                stream.Write(value.r);
            }

            if (value.g != 0)
            {
                NetConvertBase.SetBit(ref bits[0], 2, true);
                stream.Write(value.g);
            }

            if (value.b != 0)
            {
                NetConvertBase.SetBit(ref bits[0], 3, true);
                stream.Write(value.b);
            }

            if (value.a != 0)
            {
                NetConvertBase.SetBit(ref bits[0], 4, true);
                stream.Write(value.a);
            }

            int pos1 = stream.Position;
            stream.Position = pos;
            stream.Write(bits, 0, 1);
            stream.Position = pos1;
        }
		
        public Net.Color32 Read(ISegment stream) 
        {
            var value = new Net.Color32();
            Read(ref value, stream);
            return value;
        }

		public void Read(ref Net.Color32 value, ISegment stream)
		{
			var bits = stream.Read(1);

			if(NetConvertBase.GetBit(bits[0], 1))
				value.r = stream.ReadByte();

			if(NetConvertBase.GetBit(bits[0], 2))
				value.g = stream.ReadByte();

			if(NetConvertBase.GetBit(bits[0], 3))
				value.b = stream.ReadByte();

			if(NetConvertBase.GetBit(bits[0], 4))
				value.a = stream.ReadByte();

		}

        public void WriteValue(object value, ISegment stream)
        {
            Write((Net.Color32)value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<Net.Color32>.Serialize = this;
        }
    }
}

namespace Binding
{
	public readonly struct NetColor32ArrayBind : ISerialize<Net.Color32[]>, ISerialize
	{
        public ushort HashCode { get { return 143; } }

		public void Write(Net.Color32[] value, ISegment stream)
		{
			int count = value.Length;
			stream.Write(count);
			if (count == 0) return;
			var bind = new NetColor32Bind();
			foreach (var value1 in value)
				bind.Write(value1, stream);
		}

		public Net.Color32[] Read(ISegment stream)
		{
			var count = stream.ReadInt32();
			var value = new Net.Color32[count];
			if (count == 0) return value;
			var bind = new NetColor32Bind();
			for (int i = 0; i < count; i++)
				value[i] = bind.Read(stream);
			return value;
		}

		public void WriteValue(object value, ISegment stream)
		{
			Write((Net.Color32[])value, stream);
		}

		public object ReadValue(ISegment stream)
		{
			return Read(stream);
		}

        public void Bind()
        {
			SerializeCache<Net.Color32[]>.Serialize = this;
        }
    }
}

namespace Binding
{
	public readonly struct SystemCollectionsGenericListNetColor32Bind : ISerialize<System.Collections.Generic.List<Net.Color32>>, ISerialize
	{
        public ushort HashCode { get { return 144; } }

		public void Write(System.Collections.Generic.List<Net.Color32> value, ISegment stream)
		{
			int count = value.Count;
			stream.Write(count);
			if (count == 0) return;
			var bind = new NetColor32Bind();
			foreach (var value1 in value)
				bind.Write(value1, stream);
		}

		public System.Collections.Generic.List<Net.Color32> Read(ISegment stream)
		{
			var count = stream.ReadInt32();
			var value = new System.Collections.Generic.List<Net.Color32>(count);
			if (count == 0) return value;
			var bind = new NetColor32Bind();
			for (int i = 0; i < count; i++)
				value.Add(bind.Read(stream));
			return value;
		}

		public void WriteValue(object value, ISegment stream)
		{
			Write((System.Collections.Generic.List<Net.Color32>)value, stream);
		}

		public object ReadValue(ISegment stream)
		{
			return Read(stream);
		}

        public void Bind()
        {
            SerializeCache<System.Collections.Generic.List<Net.Color32>>.Serialize = this;
        }
    }
}
