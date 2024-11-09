using Net.System;
using Net.Serialize;
using Net.Event;

namespace Net.Table
{
    public class TableBinarySerialize
    {
        private const int Version = 1;

        public static byte[] Serialize(DataSetInfo dataSet)
        {
            var binarySerialize = new BinarySerialize();
            Initialize(binarySerialize);
            var segment = new Segment(new byte[1024 * 1024 * 50]);
            segment.Write(Version);
            binarySerialize.SerializeObject(segment, dataSet, true, true);
            return segment.ToArray();
        }

        public static DataSetInfo Deserialize(byte[] buffer)
        {
            var binarySerialize = new BinarySerialize();
            Initialize(binarySerialize);
            var segment = new Segment(buffer);
            var version = segment.ReadInt32();
            if (version != Version)
            {
                NDebug.LogError("配置表版本不一致, 请重新生成配置数据!");
                return null;
            }
            return binarySerialize.DeserializeObject<DataSetInfo>(segment, false, true, true);
        }

        private static void Initialize(BinarySerialize binarySerialize)
        {
            binarySerialize.Init();
            binarySerialize.AddSerializeType<DataSetInfo>();
            binarySerialize.AddSerializeType<DataTableInfo>();
            binarySerialize.AddSerializeType<DataRowInfo>();
            binarySerialize.AddSerializeType<DataColumnInfo>();
        }
    }
}