using System;
using Net.Share;
using Net.System;

namespace Net.Serialize
{
    /// <summary>
    /// 快速解析类型, 使用此类需要使用AddSerializeType先添加序列化类型, 类型是固定, 并且双端统一
    /// </summary>
    public class NetConvertFast : NetConvertBase
    {
        private static readonly BinarySerialize binarySerialize = new();

        static NetConvertFast()
        {
            binarySerialize.Init();
        }

        /// <summary>
        /// 添加可序列化的参数类型, 网络参数类型 如果不进行添加将不会被序列化,反序列化
        /// </summary>
        public static void AddSerializeType<T>()
        {
            AddSerializeType(typeof(T));
        }

        /// <summary>
        /// 添加经过网络传送的类型
        /// </summary>
        /// <param name="type"></param>
        public static void AddSerializeType(Type type) => binarySerialize.AddSerializeType(type);

        /// <summary>
        /// 添加经过网络传送的类型
        /// </summary>
        /// <param name="type">序列化的类型</param>
        /// <param name="typeHash">序列化的类型反序列化的哈希识别码</param>
        public static void AddSerializeType(Type type, ushort typeHash) => binarySerialize.AddSerializeType(type, typeHash);

        public static bool Serialize(ISegment segment, RPCModel model, bool recordType = false) => binarySerialize.SerializeModel(segment, model, recordType);

        public static bool Deserialize(ISegment segment, RPCModel model, bool recordType = false) => binarySerialize.DeserializeModel(segment, model, recordType);
    }
}