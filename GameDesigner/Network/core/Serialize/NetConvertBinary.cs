using System;
using Net.Share;
using Net.System;

namespace Net.Serialize
{
    /// <summary>
    /// 提供序列化二进制类
    /// </summary>
    public class NetConvertBinary : NetConvertBase
    {
        private static readonly BinarySerialize binarySerialize = new();

        static NetConvertBinary()
        {
            Init();
        }

        /// <summary>
        /// 初始化网络转换类型
        /// </summary>
        public static bool Init() => binarySerialize.Init();

        public static void MakeNonSerializedAttribute<T>() where T : Attribute => binarySerialize.MakeNonSerializedAttribute<T>();

        /// <summary>
        /// 添加网络基本类型， int，float，bool，string......
        /// </summary>
        public static void AddSerializeBaseType() => binarySerialize.AddSerializeBaseType();

        /// <summary>
        /// 添加可序列化的参数类型, 网络参数类型 如果不进行添加将不会被序列化,反序列化
        /// </summary>
        /// <typeparam name="T">序列化的类型</typeparam>
        /// <param name="onlyFields">只序列化的字段名称列表</param>
        /// <param name="ignoreFields">不序列化的字段名称列表</param>
        public static void AddSerializeType<T>(string[] onlyFields = default, params string[] ignoreFields)
            => AddSerializeType(typeof(T), onlyFields, ignoreFields);

        /// <summary>
        /// 添加可序列化的参数类型, 网络参数类型 如果不进行添加将不会被序列化,反序列化
        /// </summary>
        /// <param name="type">序列化的类型</param>
        /// <param name="onlyFields">只序列化的字段名称列表</param>
        /// <param name="ignoreFields">不序列化的字段名称列表</param>
        public static void AddSerializeType(Type type, string[] onlyFields = default, params string[] ignoreFields)
            => binarySerialize.AddSerializeType(type, onlyFields, ignoreFields);

        /// <summary>
        /// 添加可序列化的参数类型, 网络参数类型 如果不进行添加将不会被序列化,反序列化
        /// </summary>
        /// <param name="type">序列化的类型</param>
        /// <param name="typeHash">序列化的类型哈希码,用于反序列化识别</param>
        /// <param name="onlyFields">只序列化的字段名称列表</param>
        /// <param name="ignoreFields">不序列化的字段名称列表</param>
        public static void AddSerializeType(Type type, ushort typeHash, string[] onlyFields = default, params string[] ignoreFields)
            => binarySerialize.AddSerializeType(type, typeHash, onlyFields, ignoreFields);

        /// <summary>
        /// 索引取类型
        /// </summary>
        /// <param name="typeIndex"></param>
        /// <returns></returns>
        public static Type IndexToType(ushort typeIndex) => binarySerialize.IndexToType(typeIndex);

        /// <summary>
        /// 类型取索引
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ushort TypeToIndex(Type type) => binarySerialize.TypeToIndex(type);

        public static bool SerializeModel(ISegment segment, RPCModel model, bool recordType = false)
            => binarySerialize.SerializeModel(segment, model, recordType);

        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="recordType">是否记录类型</param>
        /// <param name="ignore"></param>
        /// <returns></returns>
        public static ISegment Serialize(object obj, bool recordType = false, bool ignore = false)
            => binarySerialize.Serialize(obj, recordType, ignore);

        /// <summary>
        /// 序列化对象, 不记录反序列化类型
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="recordType"></param>
        /// <param name="ignore"></param>
        /// <returns></returns>
        public static ISegment SerializeObject(object obj, bool recordType = false, bool ignore = false)
            => binarySerialize.SerializeObject(obj, recordType, ignore);

        /// <summary>
        /// 序列化对象, 不记录反序列化类型
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="obj"></param>
        /// <param name="recordType"></param>
        /// <param name="ignore"></param>
        /// <returns></returns>
        public static bool SerializeObject(ISegment stream, object obj, bool recordType = false, bool ignore = false)
            => binarySerialize.SerializeObject(stream, obj, recordType, ignore);

        /// <summary>
        /// 序列化对象
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="type"></param>
        /// <param name="target"></param>
        /// <param name="recordType"></param>
        /// <param name="ignore">忽略不使用<see cref="AddBaseType"/>方法也会被序列化</param>
        public static void WriteObject(ISegment segment, Type type, object target, bool recordType, bool ignore)
            => binarySerialize.WriteObject(segment, type, target, recordType, ignore);

        public static bool DeserializeModel(ISegment segment, RPCModel model, bool recordType = false, bool ignore = false)
            => binarySerialize.DeserializeModel(segment, model, recordType, ignore);

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buffer"></param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <param name="recordType">序列化的类型字段是 object[]字段时, 可以帮你记录object的绝对类型</param>
        /// <param name="ignore">忽略不使用<see cref="AddBaseType"/>方法也会被序列化</param>
        /// <returns></returns>
        public static T DeserializeObject<T>(byte[] buffer, int index, int count, bool recordType = false, bool ignore = false)
            => binarySerialize.DeserializeObject<T>(buffer, index, count, recordType, ignore);

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="segment"></param>
        /// <param name="isPush"></param>
        /// <param name="recordType"></param>
        /// <param name="ignore">忽略不使用<see cref="AddBaseType"/>方法也会被序列化</param>
        /// <returns></returns>
        public static T DeserializeObject<T>(ISegment segment, bool isPush = true, bool recordType = false, bool ignore = false)
            => (T)DeserializeObject(segment, typeof(T), isPush, recordType, ignore);

        /// <summary>
        /// 反序列化 -- 记录类型时用到
        /// </summary>
        /// <typeparam name="T">基类或真实类对象</typeparam>
        /// <param name="segment"></param>
        /// <param name="type">派生类型</param>
        /// <param name="isPush"></param>
        /// <param name="recordType">记录类型?</param>
        /// <param name="ignore">忽略不使用<see cref="AddBaseType"/>方法也会被序列化</param>
        /// <returns></returns>
        public static T DeserializeObject<T>(ISegment segment, Type type, bool isPush = true, bool recordType = false, bool ignore = false)
            => (T)DeserializeObject(segment, type, isPush, recordType, ignore);

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="type"></param>
        /// <param name="isPush"></param>
        /// <param name="recordType"></param>
        /// <param name="ignore">忽略不使用<see cref="AddBaseType"/>方法也会被序列化</param>
        /// <returns></returns>
        public static object DeserializeObject(ISegment segment, Type type, bool isPush = true, bool recordType = false, bool ignore = false)
            => binarySerialize.DeserializeObject(segment, type, isPush, recordType, ignore);

        public static object Deserialize(ISegment segment, bool isPush = true, bool recordType = false, bool ignore = false)
            => binarySerialize.Deserialize(segment, isPush, recordType, ignore);

        /// <summary>
        /// 反序列化实体对象
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="type"></param>
        /// <param name="recordType"></param>
        /// <param name="ignore"></param>
        /// <returns></returns>
        public static object ReadObject(ISegment segment, Type type, bool recordType, bool ignore)
            => binarySerialize.ReadObject(segment, type, recordType, ignore);
    }
}