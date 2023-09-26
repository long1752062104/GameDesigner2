using Net.Serialize;
using Net.System;
using System;
using System.Collections.Generic;

namespace Binding
{
    public readonly struct UnityEngineVector3IntBind : ISerialize<UnityEngine.Vector3Int>, ISerialize
    {
        public ushort HashCode { get { return 115; } }

        public void Write(UnityEngine.Vector3Int value, ISegment stream)
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

            if (value.z != 0)
            {
                NetConvertBase.SetBit(ref bits[0], 3, true);
                stream.Write(value.z);
            }

            int pos1 = stream.Position;
            stream.Position = pos;
            stream.Write(bits, 0, 1);
            stream.Position = pos1;
        }
		
        public UnityEngine.Vector3Int Read(ISegment stream) 
        {
            var value = new UnityEngine.Vector3Int();
            Read(ref value, stream);
            return value;
        }

		public void Read(ref UnityEngine.Vector3Int value, ISegment stream)
		{
			var bits = stream.Read(1);

			if(NetConvertBase.GetBit(bits[0], 1))
				value.x = stream.ReadInt32();

			if(NetConvertBase.GetBit(bits[0], 2))
				value.y = stream.ReadInt32();

			if(NetConvertBase.GetBit(bits[0], 3))
				value.z = stream.ReadInt32();

		}

        public void WriteValue(object value, ISegment stream)
        {
            Write((UnityEngine.Vector3Int)value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }
    }
}

namespace Binding
{
	public readonly struct UnityEngineVector3IntArrayBind : ISerialize<UnityEngine.Vector3Int[]>, ISerialize
	{
        public ushort HashCode { get { return 116; } }

		public void Write(UnityEngine.Vector3Int[] value, ISegment stream)
		{
			int count = value.Length;
			stream.Write(count);
			if (count == 0) return;
			var bind = new UnityEngineVector3IntBind();
			foreach (var value1 in value)
				bind.Write(value1, stream);
		}

		public UnityEngine.Vector3Int[] Read(ISegment stream)
		{
			var count = stream.ReadInt32();
			var value = new UnityEngine.Vector3Int[count];
			if (count == 0) return value;
			var bind = new UnityEngineVector3IntBind();
			for (int i = 0; i < count; i++)
				value[i] = bind.Read(stream);
			return value;
		}

		public void WriteValue(object value, ISegment stream)
		{
			Write((UnityEngine.Vector3Int[])value, stream);
		}

		public object ReadValue(ISegment stream)
		{
			return Read(stream);
		}
	}
}

namespace Binding
{
	public readonly struct SystemCollectionsGenericListUnityEngineVector3IntBind : ISerialize<System.Collections.Generic.List<UnityEngine.Vector3Int>>, ISerialize
	{
        public ushort HashCode { get { return 117; } }

		public void Write(System.Collections.Generic.List<UnityEngine.Vector3Int> value, ISegment stream)
		{
			int count = value.Count;
			stream.Write(count);
			if (count == 0) return;
			var bind = new UnityEngineVector3IntBind();
			foreach (var value1 in value)
				bind.Write(value1, stream);
		}

		public System.Collections.Generic.List<UnityEngine.Vector3Int> Read(ISegment stream)
		{
			var count = stream.ReadInt32();
			var value = new System.Collections.Generic.List<UnityEngine.Vector3Int>(count);
			if (count == 0) return value;
			var bind = new UnityEngineVector3IntBind();
			for (int i = 0; i < count; i++)
				value.Add(bind.Read(stream));
			return value;
		}

		public void WriteValue(object value, ISegment stream)
		{
			Write((System.Collections.Generic.List<UnityEngine.Vector3Int>)value, stream);
		}

		public object ReadValue(ISegment stream)
		{
			return Read(stream);
		}
	}
}
