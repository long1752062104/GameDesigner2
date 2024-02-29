using Net.Serialize;
using Net.System;
using System;
using System.Collections.Generic;

namespace Binding
{
    public readonly struct NetShareOperationBind : ISerialize<Net.Share.Operation>, ISerialize
    {
        public ushort HashCode { get { return 151; } }

        public void Write(Net.Share.Operation value, ISegment stream)
        {
            int pos = stream.Position;
            stream.Position += 2;
            var bits = new byte[2];

            if (value.cmd != 0)
            {
                NetConvertBase.SetBit(ref bits[0], 1, true);
                stream.Write(value.cmd);
            }

            if (value.cmd1 != 0)
            {
                NetConvertBase.SetBit(ref bits[0], 2, true);
                stream.Write(value.cmd1);
            }

            if (value.cmd2 != 0)
            {
                NetConvertBase.SetBit(ref bits[0], 3, true);
                stream.Write(value.cmd2);
            }

            if (!string.IsNullOrEmpty(value.name))
            {
                NetConvertBase.SetBit(ref bits[0], 4, true);
                stream.Write(value.name);
            }

            if (value.position != default(Net.Vector3))
            {
                NetConvertBase.SetBit(ref bits[0], 5, true);
                SerializeCache<Net.Vector3>.Serialize.Write(value.position, stream);
            }

            if (value.rotation != default(Net.Quaternion))
            {
                NetConvertBase.SetBit(ref bits[0], 6, true);
                SerializeCache<Net.Quaternion>.Serialize.Write(value.rotation, stream);
            }

            if (value.direction != default(Net.Vector3))
            {
                NetConvertBase.SetBit(ref bits[0], 7, true);
                SerializeCache<Net.Vector3>.Serialize.Write(value.direction, stream);
            }

            if (value.identity != 0)
            {
                NetConvertBase.SetBit(ref bits[0], 8, true);
                stream.Write(value.identity);
            }

            if (value.uid != 0)
            {
                NetConvertBase.SetBit(ref bits[1], 1, true);
                stream.Write(value.uid);
            }

            if (value.index != 0)
            {
                NetConvertBase.SetBit(ref bits[1], 2, true);
                stream.Write(value.index);
            }

            if (value.index1 != 0)
            {
                NetConvertBase.SetBit(ref bits[1], 3, true);
                stream.Write(value.index1);
            }

            if (value.index2 != 0)
            {
                NetConvertBase.SetBit(ref bits[1], 4, true);
                stream.Write(value.index2);
            }

            if (value.index3 != 0)
            {
                NetConvertBase.SetBit(ref bits[1], 5, true);
                stream.Write(value.index3);
            }

            if (value.buffer != null)
            {
                NetConvertBase.SetBit(ref bits[1], 6, true);
                stream.Write(value.buffer);
            }

            if (!string.IsNullOrEmpty(value.name1))
            {
                NetConvertBase.SetBit(ref bits[1], 7, true);
                stream.Write(value.name1);
            }

            if (!string.IsNullOrEmpty(value.name2))
            {
                NetConvertBase.SetBit(ref bits[1], 8, true);
                stream.Write(value.name2);
            }

            int pos1 = stream.Position;
            stream.Position = pos;
            stream.Write(bits, 0, 2);
            stream.Position = pos1;
        }

        public Net.Share.Operation Read(ISegment stream)
        {
            var value = new Net.Share.Operation();
            Read(ref value, stream);
            return value;
        }

        public void Read(ref Net.Share.Operation value, ISegment stream)
        {
            var bits = stream.Read(2);

            if (NetConvertBase.GetBit(bits[0], 1))
                value.cmd = stream.ReadByte();

            if (NetConvertBase.GetBit(bits[0], 2))
                value.cmd1 = stream.ReadByte();

            if (NetConvertBase.GetBit(bits[0], 3))
                value.cmd2 = stream.ReadByte();

            if (NetConvertBase.GetBit(bits[0], 4))
                value.name = stream.ReadString();

            if (NetConvertBase.GetBit(bits[0], 5))
            {
                value.position = SerializeCache<Net.Vector3>.Serialize.Read(stream);
            }

            if (NetConvertBase.GetBit(bits[0], 6))
            {
                value.rotation = SerializeCache<Net.Quaternion>.Serialize.Read(stream);
            }

            if (NetConvertBase.GetBit(bits[0], 7))
            {
                value.direction = SerializeCache<Net.Vector3>.Serialize.Read(stream);
            }

            if (NetConvertBase.GetBit(bits[0], 8))
                value.identity = stream.ReadInt32();

            if (NetConvertBase.GetBit(bits[1], 1))
                value.uid = stream.ReadInt32();

            if (NetConvertBase.GetBit(bits[1], 2))
                value.index = stream.ReadInt32();

            if (NetConvertBase.GetBit(bits[1], 3))
                value.index1 = stream.ReadInt32();

            if (NetConvertBase.GetBit(bits[1], 4))
                value.index2 = stream.ReadInt32();

            if (NetConvertBase.GetBit(bits[1], 5))
                value.index3 = stream.ReadInt32();

            if (NetConvertBase.GetBit(bits[1], 6))
                value.buffer = stream.ReadByteArray();

            if (NetConvertBase.GetBit(bits[1], 7))
                value.name1 = stream.ReadString();

            if (NetConvertBase.GetBit(bits[1], 8))
                value.name2 = stream.ReadString();

        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((Net.Share.Operation)value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<Net.Share.Operation>.Serialize = this;
        }
    }
}

namespace Binding
{
    public readonly struct NetShareOperationArrayBind : ISerialize<Net.Share.Operation[]>, ISerialize
    {
        public ushort HashCode { get { return 152; } }

        public void Write(Net.Share.Operation[] value, ISegment stream)
        {
            int count = value.Length;
            stream.Write(count);
            if (count == 0) return;
            var serialize = SerializeCache<Net.Share.Operation>.Serialize;
            for (int i = 0; i < value.Length; i++)
                serialize.Write(value[i], stream);
        }

        public Net.Share.Operation[] Read(ISegment stream)
        {
            var count = stream.ReadInt32();
            var value = new Net.Share.Operation[count];
            if (count == 0) return value;
            var serialize = SerializeCache<Net.Share.Operation>.Serialize;
            for (int i = 0; i < count; i++)
                value[i] = serialize.Read(stream);
            return value;
        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((Net.Share.Operation[])value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<Net.Share.Operation[]>.Serialize = this;
        }
    }
}

namespace Binding
{
    public readonly struct SystemCollectionsGenericListNetShareOperationBind : ISerialize<System.Collections.Generic.List<Net.Share.Operation>>, ISerialize
    {
        public ushort HashCode { get { return 153; } }

        public void Write(System.Collections.Generic.List<Net.Share.Operation> value, ISegment stream)
        {
            int count = value.Count;
            stream.Write(count);
            if (count == 0) return;
            var serialize = SerializeCache<Net.Share.Operation>.Serialize;
            for (int i = 0; i < value.Count; i++)
                serialize.Write(value[i], stream);
        }

        public System.Collections.Generic.List<Net.Share.Operation> Read(ISegment stream)
        {
            var count = stream.ReadInt32();
            var value = new System.Collections.Generic.List<Net.Share.Operation>(count);
            if (count == 0) return value;
            var serialize = SerializeCache<Net.Share.Operation>.Serialize;
            for (int i = 0; i < count; i++)
                value.Add(serialize.Read(stream));
            return value;
        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((System.Collections.Generic.List<Net.Share.Operation>)value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<System.Collections.Generic.List<Net.Share.Operation>>.Serialize = this;
        }
    }
}
