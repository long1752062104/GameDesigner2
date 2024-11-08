using Net.System;
using Net.Serialize;

namespace Net.Table
{
    public class TableBinarySerialize
    {
        public static byte[] Serialize(DataSetInfo dataSet)
        {
            var binarySerialize = new BinarySerialize();
            Initialize(binarySerialize);
            var segment = new Segment(new byte[1024 * 1024 * 50]);
            binarySerialize.SerializeObject(segment, dataSet, true, true);
            return segment.ToArray();
        }

        public static DataSetInfo Deserialize(byte[] buffer)
        {
            var binarySerialize = new BinarySerialize();
            Initialize(binarySerialize);
            return binarySerialize.DeserializeObject<DataSetInfo>(new Segment(buffer), false, true, true);
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