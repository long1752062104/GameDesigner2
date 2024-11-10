using Net.System;
using Net.Serialize;
using Net.Event;
using System;
using Net.Helper;

namespace Net.Table
{
    public class TableBinarySerialize : BinarySerialize
    {
        private const int Version = 2;

        public override void WriteTypeToIndex(ISegment segment, Type type)
        {
            segment.Write(type.ToString());
        }

        public override Type ReadIndexToType(ISegment segment)
        {
            var typeName = segment.ReadString();
            return AssemblyHelper.GetType(typeName);
        }

        public static byte[] Serialize(DataSetInfo dataSet)
        {
            var binarySerialize = new TableBinarySerialize();
            var segment = new Segment(new byte[1024 * 1024 * 50]);
            segment.Write(Version);
            binarySerialize.SerializeObject(segment, dataSet, true, true);
            return segment.ToArray();
        }

        public static DataSetInfo Deserialize(byte[] buffer)
        {
            var binarySerialize = new TableBinarySerialize();
            var segment = new Segment(buffer);
            var version = segment.ReadInt32();
            if (version != Version)
            {
                NDebug.LogError("配置表版本不一致, 请重新生成配置数据!");
                return null;
            }
            return binarySerialize.DeserializeObject<DataSetInfo>(segment, false, true, true);
        }
    }
}