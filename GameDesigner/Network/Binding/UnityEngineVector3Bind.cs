using Net.Serialize;
using Net.System;
using System;
using System.Collections.Generic;

namespace Binding
{
    public readonly struct UnityEngineVector3Bind : ISerialize<UnityEngine.Vector3>, ISerialize
    {
        public ushort HashCode { get { return 112; } }

        public void Write(UnityEngine.Vector3 value, ISegment stream)
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
		
        public UnityEngine.Vector3 Read(ISegment stream) 
        {
            var value = new UnityEngine.Vector3();
            Read(ref value, stream);
            return value;
        }

		public void Read(ref UnityEngine.Vector3 value, ISegment stream)
		{
			var bits = stream.Read(1);

			if(NetConvertBase.GetBit(bits[0], 1))
				value.x = stream.ReadSingle();

			if(NetConvertBase.GetBit(bits[0], 2))
				value.y = stream.ReadSingle();

			if(NetConvertBase.GetBit(bits[0], 3))
				value.z = stream.ReadSingle();

		}

        public void WriteValue(object value, ISegment stream)
        {
            Write((UnityEngine.Vector3)value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }
    }
}

namespace Binding
{
	public readonly struct UnityEngineVector3ArrayBind : ISerialize<UnityEngine.Vector3[]>, ISerialize
	{
        public ushort HashCode { get { return 113; } }

		public void Write(UnityEngine.Vector3[] value, ISegment stream)
		{
			int count = value.Length;
			stream.Write(count);
			if (count == 0) return;
			var bind = new UnityEngineVector3Bind();
			foreach (var value1 in value)
				bind.Write(value1, stream);
		}

		public UnityEngine.Vector3[] Read(ISegment stream)
		{
			var count = stream.ReadInt32();
			var value = new UnityEngine.Vector3[count];
			if (count == 0) return value;
			var bind = new UnityEngineVector3Bind();
			for (int i = 0; i < count; i++)
				value[i] = bind.Read(stream);
			return value;
		}

		public void WriteValue(object value, ISegment stream)
		{
			Write((UnityEngine.Vector3[])value, stream);
		}

		public object ReadValue(ISegment stream)
		{
			return Read(stream);
		}
	}
}

namespace Binding
{
	public readonly struct SystemCollectionsGenericListUnityEngineVector3Bind : ISerialize<System.Collections.Generic.List<UnityEngine.Vector3>>, ISerialize
	{
        public ushort HashCode { get { return 114; } }

		public void Write(System.Collections.Generic.List<UnityEngine.Vector3> value, ISegment stream)
		{
			int count = value.Count;
			stream.Write(count);
			if (count == 0) return;
			var bind = new UnityEngineVector3Bind();
			foreach (var value1 in value)
				bind.Write(value1, stream);
		}

		public System.Collections.Generic.List<UnityEngine.Vector3> Read(ISegment stream)
		{
			var count = stream.ReadInt32();
			var value = new System.Collections.Generic.List<UnityEngine.Vector3>(count);
			if (count == 0) return value;
			var bind = new UnityEngineVector3Bind();
			for (int i = 0; i < count; i++)
				value.Add(bind.Read(stream));
			return value;
		}

		public void WriteValue(object value, ISegment stream)
		{
			Write((System.Collections.Generic.List<UnityEngine.Vector3>)value, stream);
		}

		public object ReadValue(ISegment stream)
		{
			return Read(stream);
		}
	}
}
