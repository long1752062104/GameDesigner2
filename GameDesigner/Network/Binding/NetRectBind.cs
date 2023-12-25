using Net.Serialize;
using Net.System;
using System;
using System.Collections.Generic;

namespace Binding
{
    public readonly struct NetRectBind : ISerialize<Net.Rect>, ISerialize
    {
        public ushort HashCode { get { return 130; } }

        public void Write(Net.Rect value, ISegment stream)
        {
            int pos = stream.Position;
            stream.Position += 1;
            var bits = new byte[1];

            if (value.x != 0)
            {
                NetConvertBase.SetBit(ref bits[0], 1, true);
                stream.Write(value.x);
            }

            if (value.y != 0)
            {
                NetConvertBase.SetBit(ref bits[0], 2, true);
                stream.Write(value.y);
            }

            if (value.width != 0)
            {
                NetConvertBase.SetBit(ref bits[0], 3, true);
                stream.Write(value.width);
            }

            if (value.height != 0)
            {
                NetConvertBase.SetBit(ref bits[0], 4, true);
                stream.Write(value.height);
            }

            int pos1 = stream.Position;
            stream.Position = pos;
            stream.Write(bits, 0, 1);
            stream.Position = pos1;
        }
		
        public Net.Rect Read(ISegment stream) 
        {
            var value = new Net.Rect();
            Read(ref value, stream);
            return value;
        }

		public void Read(ref Net.Rect value, ISegment stream)
		{
			var bits = stream.Read(1);

			if(NetConvertBase.GetBit(bits[0], 1))
				value.x = stream.ReadSingle();

			if(NetConvertBase.GetBit(bits[0], 2))
				value.y = stream.ReadSingle();

			if(NetConvertBase.GetBit(bits[0], 3))
				value.width = stream.ReadSingle();

			if(NetConvertBase.GetBit(bits[0], 4))
				value.height = stream.ReadSingle();

		}

        public void WriteValue(object value, ISegment stream)
        {
            Write((Net.Rect)value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }
    }
}

namespace Binding
{
	public readonly struct NetRectArrayBind : ISerialize<Net.Rect[]>, ISerialize
	{
        public ushort HashCode { get { return 131; } }

		public void Write(Net.Rect[] value, ISegment stream)
		{
			int count = value.Length;
			stream.Write(count);
			if (count == 0) return;
			var bind = new NetRectBind();
			foreach (var value1 in value)
				bind.Write(value1, stream);
		}

		public Net.Rect[] Read(ISegment stream)
		{
			var count = stream.ReadInt32();
			var value = new Net.Rect[count];
			if (count == 0) return value;
			var bind = new NetRectBind();
			for (int i = 0; i < count; i++)
				value[i] = bind.Read(stream);
			return value;
		}

		public void WriteValue(object value, ISegment stream)
		{
			Write((Net.Rect[])value, stream);
		}

		public object ReadValue(ISegment stream)
		{
			return Read(stream);
		}
	}
}

namespace Binding
{
	public readonly struct SystemCollectionsGenericListNetRectBind : ISerialize<System.Collections.Generic.List<Net.Rect>>, ISerialize
	{
        public ushort HashCode { get { return 132; } }

		public void Write(System.Collections.Generic.List<Net.Rect> value, ISegment stream)
		{
			int count = value.Count;
			stream.Write(count);
			if (count == 0) return;
			var bind = new NetRectBind();
			foreach (var value1 in value)
				bind.Write(value1, stream);
		}

		public System.Collections.Generic.List<Net.Rect> Read(ISegment stream)
		{
			var count = stream.ReadInt32();
			var value = new System.Collections.Generic.List<Net.Rect>(count);
			if (count == 0) return value;
			var bind = new NetRectBind();
			for (int i = 0; i < count; i++)
				value.Add(bind.Read(stream));
			return value;
		}

		public void WriteValue(object value, ISegment stream)
		{
			Write((System.Collections.Generic.List<Net.Rect>)value, stream);
		}

		public object ReadValue(ISegment stream)
		{
			return Read(stream);
		}
	}
}
