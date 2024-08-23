using Net.Serialize;
using Net.System;
using System;
using System.Collections.Generic;

namespace Binding
{
    public readonly struct UnityEngineQuaternionBind : ISerialize<UnityEngine.Quaternion>, ISerialize
    {
        public ushort HashCode { get { return 127; } }

        public void Write(UnityEngine.Quaternion value, ISegment stream)
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

            if (value.w != 0)
            {
                NetConvertBase.SetBit(ref bits[0], 4, true);
                stream.Write(value.w);
            }

            int pos1 = stream.Position;
            stream.Position = pos;
            stream.Write(bits, 0, 1);
            stream.Position = pos1;
        }

        public UnityEngine.Quaternion Read(ISegment stream)
        {
            var value = new UnityEngine.Quaternion();
            Read(ref value, stream);
            return value;
        }

        public unsafe void Read(ref UnityEngine.Quaternion value, ISegment stream)
        {
            var bits = stream.ReadPtr(1);

            if (NetConvertBase.GetBit(bits[0], 1))
                value.x = stream.ReadSingle();

            if (NetConvertBase.GetBit(bits[0], 2))
                value.y = stream.ReadSingle();

            if (NetConvertBase.GetBit(bits[0], 3))
                value.z = stream.ReadSingle();

            if (NetConvertBase.GetBit(bits[0], 4))
                value.w = stream.ReadSingle();

        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((UnityEngine.Quaternion)value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<UnityEngine.Quaternion>.Serialize = this;
        }
    }
}

namespace Binding
{
    public readonly struct UnityEngineQuaternionArrayBind : ISerialize<UnityEngine.Quaternion[]>, ISerialize
    {
        public ushort HashCode { get { return 128; } }

        public void Write(UnityEngine.Quaternion[] value, ISegment stream)
        {
            int count = value.Length;
            stream.Write(count);
            if (count == 0) return;
            var bind = new UnityEngineQuaternionBind();
            foreach (var value1 in value)
                bind.Write(value1, stream);
        }

        public UnityEngine.Quaternion[] Read(ISegment stream)
        {
            var count = stream.ReadInt32();
            var value = new UnityEngine.Quaternion[count];
            if (count == 0) return value;
            var bind = new UnityEngineQuaternionBind();
            for (int i = 0; i < count; i++)
                value[i] = bind.Read(stream);
            return value;
        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((UnityEngine.Quaternion[])value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<UnityEngine.Quaternion[]>.Serialize = this;
        }
    }
}

namespace Binding
{
    public readonly struct SystemCollectionsGenericListUnityEngineQuaternionBind : ISerialize<System.Collections.Generic.List<UnityEngine.Quaternion>>, ISerialize
    {
        public ushort HashCode { get { return 129; } }

        public void Write(System.Collections.Generic.List<UnityEngine.Quaternion> value, ISegment stream)
        {
            int count = value.Count;
            stream.Write(count);
            if (count == 0) return;
            var bind = new UnityEngineQuaternionBind();
            foreach (var value1 in value)
                bind.Write(value1, stream);
        }

        public System.Collections.Generic.List<UnityEngine.Quaternion> Read(ISegment stream)
        {
            var count = stream.ReadInt32();
            var value = new System.Collections.Generic.List<UnityEngine.Quaternion>(count);
            if (count == 0) return value;
            var bind = new UnityEngineQuaternionBind();
            for (int i = 0; i < count; i++)
                value.Add(bind.Read(stream));
            return value;
        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((System.Collections.Generic.List<UnityEngine.Quaternion>)value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<System.Collections.Generic.List<UnityEngine.Quaternion>>.Serialize = this;
        }
    }
}
