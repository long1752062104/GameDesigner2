using Net.Serialize;
using Net.System;
using System;
using System.Collections.Generic;

namespace Binding
{
    public readonly struct UnityEngineVector2IntBind : ISerialize<UnityEngine.Vector2Int>, ISerialize
    {
        public ushort HashCode { get { return 106; } }

        public void Write(UnityEngine.Vector2Int value, ISegment stream)
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

            int pos1 = stream.Position;
            stream.Position = pos;
            stream.Write(bits, 0, 1);
            stream.Position = pos1;
        }
		
        public UnityEngine.Vector2Int Read(ISegment stream) 
        {
            var value = new UnityEngine.Vector2Int();
            Read(ref value, stream);
            return value;
        }

		public void Read(ref UnityEngine.Vector2Int value, ISegment stream)
		{
			var bits = stream.Read(1);

			if(NetConvertBase.GetBit(bits[0], 1))
				value.x = stream.ReadInt32();

			if(NetConvertBase.GetBit(bits[0], 2))
				value.y = stream.ReadInt32();

		}

        public void WriteValue(object value, ISegment stream)
        {
            Write((UnityEngine.Vector2Int)value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<UnityEngine.Vector2Int>.Serialize = this;
        }
    }
}

namespace Binding
{
    public readonly struct UnityEngineVector2IntArrayBind : ISerialize<UnityEngine.Vector2Int[]>, ISerialize
	{
        public ushort HashCode { get { return 107; } }

		public void Write(UnityEngine.Vector2Int[] value, ISegment stream)
		{
			int count = value.Length;
			stream.Write(count);
			if (count == 0) return;
			var bind = new UnityEngineVector2IntBind();
			foreach (var value1 in value)
				bind.Write(value1, stream);
		}

		public UnityEngine.Vector2Int[] Read(ISegment stream)
		{
			var count = stream.ReadInt32();
			var value = new UnityEngine.Vector2Int[count];
			if (count == 0) return value;
			var bind = new UnityEngineVector2IntBind();
			for (int i = 0; i < count; i++)
				value[i] = bind.Read(stream);
			return value;
		}

		public void WriteValue(object value, ISegment stream)
		{
			Write((UnityEngine.Vector2Int[])value, stream);
		}

		public object ReadValue(ISegment stream)
		{
			return Read(stream);
		}

        public void Bind()
        {
			SerializeCache<UnityEngine.Vector2Int[]>.Serialize = this;
        }
    }
}

namespace Binding
{
	public readonly struct SystemCollectionsGenericListUnityEngineVector2IntBind : ISerialize<System.Collections.Generic.List<UnityEngine.Vector2Int>>, ISerialize
	{
        public ushort HashCode { get { return 108; } }

		public void Write(System.Collections.Generic.List<UnityEngine.Vector2Int> value, ISegment stream)
		{
			int count = value.Count;
			stream.Write(count);
			if (count == 0) return;
			var bind = new UnityEngineVector2IntBind();
			foreach (var value1 in value)
				bind.Write(value1, stream);
		}

		public System.Collections.Generic.List<UnityEngine.Vector2Int> Read(ISegment stream)
		{
			var count = stream.ReadInt32();
			var value = new System.Collections.Generic.List<UnityEngine.Vector2Int>(count);
			if (count == 0) return value;
			var bind = new UnityEngineVector2IntBind();
			for (int i = 0; i < count; i++)
				value.Add(bind.Read(stream));
			return value;
		}

		public void WriteValue(object value, ISegment stream)
		{
			Write((System.Collections.Generic.List<UnityEngine.Vector2Int>)value, stream);
		}

		public object ReadValue(ISegment stream)
		{
			return Read(stream);
		}

        public void Bind()
        {
            SerializeCache<System.Collections.Generic.List<UnityEngine.Vector2Int>>.Serialize = this;
        }
    }
}
