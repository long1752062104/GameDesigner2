using Net.Serialize;
using Net.System;
using System;
using System.Collections.Generic;

namespace Binding
{
    public readonly struct NetShareOperationListBind : ISerialize<Net.Share.OperationList>, ISerialize
    {
        public ushort HashCode { get { return 154; } }

        public void Write(Net.Share.OperationList value, ISegment stream)
        {
            int pos = stream.Position;
            stream.Position += 1;
            var bits = new byte[1];

            if (value.frame != 0)
            {
                NetConvertBase.SetBit(ref bits[0], 1, true);
                stream.Write(value.frame);
            }

            if (value.operations != null)
            {
                NetConvertBase.SetBit(ref bits[0], 2, true);
                SerializeCache<Net.Share.Operation[]>.Serialize.Write(value.operations, stream);
            }

            int pos1 = stream.Position;
            stream.Position = pos;
            stream.Write(bits, 0, 1);
            stream.Position = pos1;
        }

        public unsafe Net.Share.OperationList Read(ISegment stream)
        {
            var bits = stream.ReadPtr(1);

            uint frame = default;
            if (NetConvertBase.GetBit(bits[0], 1))
                frame = stream.ReadUInt32();

            Net.Share.Operation[] operations = default;
            if (NetConvertBase.GetBit(bits[0], 2))
                operations = SerializeCache<Net.Share.Operation[]>.Serialize.Read(stream);

            return new Net.Share.OperationList(frame, operations);
        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((Net.Share.OperationList)value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<Net.Share.OperationList>.Serialize = this;
        }
    }
}

namespace Binding
{
    public readonly struct NetShareOperationListArrayBind : ISerialize<Net.Share.OperationList[]>, ISerialize
    {
        public ushort HashCode { get { return 155; } }

        public void Write(Net.Share.OperationList[] value, ISegment stream)
        {
            int count = value.Length;
            stream.Write(count);
            if (count == 0) return;
            var bind = new NetShareOperationListBind();
            foreach (var value1 in value)
                bind.Write(value1, stream);
        }

        public Net.Share.OperationList[] Read(ISegment stream)
        {
            var count = stream.ReadInt32();
            var value = new Net.Share.OperationList[count];
            if (count == 0) return value;
            var bind = new NetShareOperationListBind();
            for (int i = 0; i < count; i++)
                value[i] = bind.Read(stream);
            return value;
        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((Net.Share.OperationList[])value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<Net.Share.OperationList[]>.Serialize = this;
        }
    }
}

namespace Binding
{
    public readonly struct SystemCollectionsGenericListNetShareOperationListBind : ISerialize<System.Collections.Generic.List<Net.Share.OperationList>>, ISerialize
    {
        public ushort HashCode { get { return 156; } }

        public void Write(System.Collections.Generic.List<Net.Share.OperationList> value, ISegment stream)
        {
            int count = value.Count;
            stream.Write(count);
            if (count == 0) return;
            var bind = new NetShareOperationListBind();
            foreach (var value1 in value)
                bind.Write(value1, stream);
        }

        public System.Collections.Generic.List<Net.Share.OperationList> Read(ISegment stream)
        {
            var count = stream.ReadInt32();
            var value = new System.Collections.Generic.List<Net.Share.OperationList>(count);
            if (count == 0) return value;
            var bind = new NetShareOperationListBind();
            for (int i = 0; i < count; i++)
                value.Add(bind.Read(stream));
            return value;
        }

        public void WriteValue(object value, ISegment stream)
        {
            Write((System.Collections.Generic.List<Net.Share.OperationList>)value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }

        public void Bind()
        {
            SerializeCache<System.Collections.Generic.List<Net.Share.OperationList>>.Serialize = this;
        }
    }
}
