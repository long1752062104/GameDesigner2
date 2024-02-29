namespace Net.Serialize
{
    using Net.Event;
    using global::System;
    using global::System.Collections.Generic;
    using global::System.IO;
    using global::System.Linq;
    using global::System.Reflection;
    using Net.Share;
    using Net.System;

    /// <summary>
    /// 快速解析类型, 使用此类需要使用AddSerializeType先添加序列化类型, 类型是固定, 并且双端统一
    /// </summary>
    public class NetConvertFast : NetConvertBase
    {
        private static readonly Dictionary<ushort, Type> HashToTypeDict = new Dictionary<ushort, Type>();
        private static readonly Dictionary<Type, ushort> TypeToHashDict = new Dictionary<Type, ushort>();

        static NetConvertFast()
        {
            Init();
        }

        public static void Init()
        {
            HashToTypeDict.Clear();
            TypeToHashDict.Clear();
            AddNetworkBaseType();
        }

        /// <summary>
        /// 添加网络基本类型， int，float，bool，string......
        /// </summary>
        public static void AddNetworkBaseType()
        {
            AddSerializeType<short>();
            AddSerializeType<int>();
            AddSerializeType<long>();
            AddSerializeType<ushort>();
            AddSerializeType<uint>();
            AddSerializeType<ulong>();
            AddSerializeType<float>();
            AddSerializeType<double>();
            AddSerializeType<bool>();
            AddSerializeType<char>();
            AddSerializeType<string>();
            AddSerializeType<byte>();
            AddSerializeType<sbyte>();
            AddSerializeType<DateTime>();
            AddSerializeType<decimal>();
            AddSerializeType<DBNull>();
            AddSerializeType<Type>();
            //基础序列化数组
            AddSerializeType<short[]>();
            AddSerializeType<int[]>();
            AddSerializeType<long[]>();
            AddSerializeType<ushort[]>();
            AddSerializeType<uint[]>();
            AddSerializeType<ulong[]>();
            AddSerializeType<float[]>();
            AddSerializeType<double[]>();
            AddSerializeType<bool[]>();
            AddSerializeType<char[]>();
            AddSerializeType<string[]>();
            AddSerializeType<byte[]>();
            AddSerializeType<sbyte[]>();
            AddSerializeType<DateTime[]>();
            AddSerializeType<decimal[]>();
            //基础序列化List
            AddSerializeType<List<short>>();
            AddSerializeType<List<int>>();
            AddSerializeType<List<long>>();
            AddSerializeType<List<ushort>>();
            AddSerializeType<List<uint>>();
            AddSerializeType<List<ulong>>();
            AddSerializeType<List<float>>();
            AddSerializeType<List<double>>();
            AddSerializeType<List<bool>>();
            AddSerializeType<List<char>>();
            AddSerializeType<List<string>>();
            AddSerializeType<List<byte>>();
            AddSerializeType<List<sbyte>>();
            AddSerializeType<List<DateTime>>();
            AddSerializeType<List<decimal>>();
            //基础序列化List
            AddSerializeType<List<short[]>>();
            AddSerializeType<List<int[]>>();
            AddSerializeType<List<long[]>>();
            AddSerializeType<List<ushort[]>>();
            AddSerializeType<List<uint[]>>();
            AddSerializeType<List<ulong[]>>();
            AddSerializeType<List<float[]>>();
            AddSerializeType<List<double[]>>();
            AddSerializeType<List<bool[]>>();
            AddSerializeType<List<char[]>>();
            AddSerializeType<List<string[]>>();
            AddSerializeType<List<byte[]>>();
            AddSerializeType<List<sbyte[]>>();
            AddSerializeType<List<DateTime[]>>();
            AddSerializeType<List<decimal[]>>();
            //其他可能用到的
            AddSerializeType<Vector2>();
            AddSerializeType<Vector3>();
            AddSerializeType<Vector4>();
            AddSerializeType<Quaternion>();
            AddSerializeType<Rect>();
            AddSerializeType<Color>();
            AddSerializeType<Color32>();
            AddSerializeType<UnityEngine.Vector2>();
            AddSerializeType<UnityEngine.Vector3>();
            AddSerializeType<UnityEngine.Vector4>();
            AddSerializeType<UnityEngine.Quaternion>();
            AddSerializeType<UnityEngine.Rect>();
            AddSerializeType<UnityEngine.Color>();
            AddSerializeType<UnityEngine.Color32>();
            //框架操作同步用到
            AddSerializeType<Operation>();
            AddSerializeType<Operation[]>();
            AddSerializeType<OperationList>();
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
        public static void AddSerializeType(Type type)
        {
            var typeHash = (ushort)HashToTypeDict.Count;
            AddSerializeType(type, typeHash);
        }

        /// <summary>
        /// 添加经过网络传送的类型
        /// </summary>
        /// <param name="type">序列化的类型</param>
        /// <param name="typeHash">序列化的类型反序列化的哈希识别码</param>
        public static void AddSerializeType(Type type, ushort typeHash)
        {
            if (TypeToHashDict.ContainsKey(type))
                throw new Exception($"已经添加{type}键，不需要添加了!");
            HashToTypeDict.Add(typeHash, type);
            TypeToHashDict.Add(type, typeHash);
        }

        public static byte[] Serialize(RPCModel model, bool recordType = false)
        {
            var segment = BufferPool.Take();
            segment.Write(model.protocol);
            foreach (object obj in model.pars)
            {
                Type type;
                if (obj == null)
                {
                    type = typeof(DBNull);
                    segment.Write(GetTypeHash(type));
                    continue;
                }
                type = obj.GetType();
                segment.Write(GetTypeHash(type));
                NetConvertBinary.WriteObject(segment, type, obj, recordType, true);
            }
            return segment.ToArray(true);
        }

        private static ushort GetTypeHash(Type type)
        {
            if (TypeToHashDict.TryGetValue(type, out var typeHash))
                return typeHash;
            throw new IOException($"参数类型:[{type}]没有被注册! 请使用NetConvertFast.AddSerializeType<{type}>()添加序列化类型! 双端都要添加");
        }

        public static Type GetTypeHash(ushort hashCode)
        {
            if (HashToTypeDict.TryGetValue(hashCode, out Type type))
                return type;
            NDebug.LogError($"找不到哈希代码类型:{hashCode}, 类型太复杂时需要使用 NetConvertOld.AddSerializeType(type) 添加类型后再进行系列化!");
            return null;
        }

        public static FuncData Deserialize(byte[] buffer, int index, int count, bool recordType = false)
        {
            FuncData fdata = default;
            try
            {
                var segment = BufferPool.NewSegment(buffer, index, count, false);
                fdata.protocol = segment.ReadInt32();
                var list = new List<object>();
                while (segment.Position < segment.Offset + segment.Count)
                {
                    var typeName = segment.ReadUInt16();
                    var type = GetTypeHash(typeName);
                    if (type == null)
                    {
                        fdata.error = true;
                        break;
                    }
                    if (type == typeof(DBNull))
                    {
                        list.Add(null);
                        continue;
                    }
                    var obj = NetConvertBinary.ReadObject(segment, type, recordType, true);
                    list.Add(obj);
                }
                fdata.pars = list.ToArray();
            }
            catch (Exception ex)
            {
                fdata.error = true;
                NDebug.LogError("反序列化:" + ex.ToString());
            }
            return fdata;
        }
    }
}