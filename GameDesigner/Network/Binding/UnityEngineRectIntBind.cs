using Net.Serialize;
using Net.System;
using System;
using System.Collections.Generic;

namespace Binding
{
    public readonly struct UnityEngineRectIntBind : ISerialize<UnityEngine.RectInt>, ISerialize
    {
        public ushort HashCode { get { return 136; } }

        public void Write(UnityEngine.RectInt value, ISegment stream)
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

        public UnityEngine.RectInt Read(ISegment stream)
        {
            var value = new UnityEngine.RectInt();
            Read(ref value, stream);
            return value;
        }

        public unsafe void Read(ref UnityEngine.RectInt value, ISegment stream)
        {
            var bits = stream.ReadPtr(1);

            if (NetConvertBase.GetBit(bits[0], 1))
                value.x = stream.ReadInt32();

            if (NetConvertBase.GetBit(bits[0], 2))
                value.y = stream.ReadInt32();

            if (NetConvertBase.GetBit(bits[0], 3))
                value.width = stream.ReadInt32();

            if (NetConvertBase.GetBit(bits[0], 4))
                value.height = stream.ReadInt32();

        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((UnityEngine.RectInt)value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<UnityEngine.RectInt>.Serialize = this;
        }
    }
}

namespace Binding
{
    public readonly struct UnityEngineRectIntArrayBind : ISerialize<UnityEngine.RectInt[]>, ISerialize
    {
        public ushort HashCode { get { return 137; } }

        public void Write(UnityEngine.RectInt[] value, ISegment stream)
        {
            int count = value.Length;
            stream.Write(count);
            if (count == 0) return;
            var bind = new UnityEngineRectIntBind();
            foreach (var value1 in value)
                bind.Write(value1, stream);
        }

        public UnityEngine.RectInt[] Read(ISegment stream)
        {
            var count = stream.ReadInt32();
            var value = new UnityEngine.RectInt[count];
            if (count == 0) return value;
            var bind = new UnityEngineRectIntBind();
            for (int i = 0; i < count; i++)
                value[i] = bind.Read(stream);
            return value;
        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((UnityEngine.RectInt[])value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<UnityEngine.RectInt[]>.Serialize = this;
        }
    }
}

namespace Binding
{
    public readonly struct SystemCollectionsGenericListUnityEngineRectIntBind : ISerialize<System.Collections.Generic.List<UnityEngine.RectInt>>, ISerialize
    {
        public ushort HashCode { get { return 138; } }

        public void Write(System.Collections.Generic.List<UnityEngine.RectInt> value, ISegment stream)
        {
            int count = value.Count;
            stream.Write(count);
            if (count == 0) return;
            var bind = new UnityEngineRectIntBind();
            foreach (var value1 in value)
                bind.Write(value1, stream);
        }

        public System.Collections.Generic.List<UnityEngine.RectInt> Read(ISegment stream)
        {
            var count = stream.ReadInt32();
            var value = new System.Collections.Generic.List<UnityEngine.RectInt>(count);
            if (count == 0) return value;
            var bind = new UnityEngineRectIntBind();
            for (int i = 0; i < count; i++)
                value.Add(bind.Read(stream));
            return value;
        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((System.Collections.Generic.List<UnityEngine.RectInt>)value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<System.Collections.Generic.List<UnityEngine.RectInt>>.Serialize = this;
        }
    }
}
