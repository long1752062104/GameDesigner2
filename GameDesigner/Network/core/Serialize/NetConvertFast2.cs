using System;
using System.Reflection;
using System.Collections.Generic;
using Net.Event;
using Net.Helper;
using Net.Share;
using Net.System;

namespace Net.Serialize
{
    /// <summary>
    /// 快速序列化2接口--动态匹配
    /// </summary>
    public interface ISerialize
    {
        /// <summary>
        /// 双端一致性的哈希协议码 --解决子项目和父项目融合问题
        /// </summary>
        ushort HashCode { get; }
        /// <summary>
        /// 绑定静态序列化缓存
        /// </summary>
        void Bind();
        /// <summary>
        /// 序列化写入
        /// </summary>
        /// <param name="value"></param>
        /// <param name="stream"></param>
        void WriteValue(object value, ISegment stream);
        /// <summary>
        /// 反序列化读取
        /// </summary>
        /// <param name="stream"></param>
        object ReadValue(ISegment stream);
    }

    /// <summary>
    /// 快速序列化2接口---类型匹配
    /// </summary>
    public interface ISerialize<T> : ISerialize
    {
        /// <summary>
        /// 序列化写入
        /// </summary>
        /// <param name="value"></param>
        /// <param name="stream"></param>
        void Write(T value, ISegment stream);
        /// <summary>
        /// 反序列化读取
        /// </summary>
        /// <param name="stream"></param>
        T Read(ISegment stream);
    }

    /// <summary>
    /// 类型绑定查找收集接口
    /// </summary>
    public interface IBindingType
    {
        /// <summary>
        /// 收集序列化类型的顺序 -!!!!- 如果有多个项目继承绑定类型时, 必须设置顺序, 否则会出现, 后端和前端收集的传输类型不一样的问题
        /// </summary>
        int SortingOrder { get; }
        /// <summary>
        /// 收集的绑定类型列表
        /// </summary>
        Dictionary<Type, Type> BindTypes { get; }
    }

    /// <summary>
    /// 绑定入口类型，Unity编辑器工具使用
    /// </summary>
    public interface IBindingEntryType
    {
        /// <summary>
        /// 要绑定的类型列表
        /// </summary>
        List<Type> BindTypes { get; }
    }

    /// <summary>
    /// 序列化缓存
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class SerializeCache<T>
    {
        /// <summary>
        /// 序列化绑定实例
        /// </summary>
        public static ISerialize<T> Serialize;
    }

    /// <summary>
    /// 极速序列化2版本
    /// </summary>
    public class NetConvertFast2 : NetConvertBase
    {
        private static readonly MyDictionary<ushort, Type> HashToTypeDict = new();
        private static readonly MyDictionary<Type, ushort> TypeToHashDict = new();
        private static readonly MyDictionary<Type, ISerialize> TypeToSerializeDict = new();
        private static readonly MyDictionary<Type, Type> BindTypes = new();

        static NetConvertFast2()
        {
            Init();
        }

        /// <summary>
        /// 初始化网络转换类型
        /// </summary>
        public static bool Init()
        {
            HashToTypeDict.Clear();
            TypeToHashDict.Clear();
            TypeToSerializeDict.Clear();
            BindTypes.Clear();
            AddBaseType();
            InitBindInterfaces();
            return true;
        }

        /// <summary>
        /// 添加网络基本类型， int，float，bool，string......
        /// </summary>
        public static void AddBaseType()
        {
            AddBaseType3<byte>();
            AddBaseType3<sbyte>();
            AddBaseType3<bool>();
            AddBaseType3<short>();
            AddBaseType3<ushort>();
            AddBaseType3<char>();
            AddBaseType3<int>();
            AddBaseType3<uint>();
            AddBaseType3<float>();
            AddBaseType3<long>();
            AddBaseType3<ulong>();
            AddBaseType3<double>();
            AddBaseType3<string>();
            AddBaseType3<decimal>();
            AddBaseType3<DateTime>();
            AddBaseType3<TimeSpan>();
            AddBaseType3<DateTimeOffset>();
            AddBaseType3<DBNull>();
        }

        /// <summary>
        /// 添加可序列化的参数类型, 网络参数类型 如果不进行添加将不会被序列化,反序列化
        /// </summary>
        /// <typeparam name="T">要添加的网络类型</typeparam>
        public static void AddSerializeType<T>()
        {
            AddSerializeType(typeof(T));
        }

        /// <summary>
        /// 添加所有可序列化的类型, 网络参数类型 如果不进行添加将不会被序列化,反序列化
        /// </summary>
        /// <param name="types"></param>
        public static void AddSerializeType3s(Type[] types)
        {
            foreach (var type in types)
            {
                if (type.IsGenericType)
                    AddSerializeType(type);
                else
                    AddSerializeType3(type);
            }
        }

        /// <summary>
        /// 添加可序列化的3个参数类型(T类,T类数组,T类List泛型), 网络参数类型 如果不进行添加将不会被序列化,反序列化
        /// </summary>
        /// <typeparam name="T">要添加的网络类型</typeparam>
        public static void AddSerializeType3<T>()
        {
            AddSerializeType(typeof(T));
            AddSerializeType(typeof(T[]));
            AddSerializeType(typeof(List<T>));
        }

        /// <summary>
        /// 添加可序列化的3个参数类型(T类,T类数组,T类List泛型), 网络参数类型 如果不进行添加将不会被序列化,反序列化
        /// </summary>
        public static void AddSerializeType3(Type type)
        {
            AddSerializeType(type);
            AddSerializeType(Array.CreateInstance(type, 0).GetType());
            AddSerializeType(typeof(List<>).MakeGenericType(type));
        }

        /// <summary>
        /// 添加可序列化的参数类型, 网络参数类型 如果不进行添加将不会被序列化,反序列化
        /// </summary>
        /// <param name="type">要添加的网络类型</param>
        public static void AddSerializeType(Type type)
        {
            if (TypeToSerializeDict.ContainsKey(type))
                throw new Exception($"已经添加{type}键，不需要添加了!");
            if (type.IsArray)
            {
                var itemType = type.GetArrayItemType();
                if (itemType.IsEnum)
                {
                    AddBaseArrayType(type, itemType);
                    return;
                }
            }
            else if (type.IsGenericType)
            {
                var arguments = type.GenericTypeArguments;
                if (arguments.Length == 1)
                {
                    var itemType = arguments[0];
                    if (itemType.IsEnum)
                    {
                        AddBaseListType(type, itemType);
                        return;
                    }
                }
            }
            else if (type.IsEnum)
            {
                AddBaseType(type);
                return;
            }
            if (!BindTypes.TryGetValue(type, out Type bindType))
                throw new Exception($"类型{type}尚未实现绑定类型,请使用工具生成绑定类型!");
            var serialize = Activator.CreateInstance(bindType) as ISerialize;
            serialize.Bind();
            var hashType = serialize.HashCode;
            HashToTypeDict.Add(hashType, type);
            TypeToHashDict.Add(type, hashType);
            TypeToSerializeDict.Add(type, serialize);
        }

        private static void AddBaseType3<T>()
        {
            AddBaseType(typeof(T));
            AddBaseArrayType(typeof(T[]), typeof(T));
            AddBaseListType(typeof(List<T>), typeof(T));
        }

        private static void AddBaseType(Type type)
        {
            if (TypeToSerializeDict.ContainsKey(type))
                return;
            Type bindType;
            if (type.IsEnum)
            {
                bindType = typeof(Binding.SystemEnumBind<>).MakeGenericType(type);
            }
            else
            {
                var typeName = "Binding." + type.ToString().Replace(".", "") + "Bind";
                bindType = AssemblyHelper.GetTypeNotOptimized(typeName);
            }
            var serialize = Activator.CreateInstance(bindType) as ISerialize;
            serialize.Bind();
            var hashType = serialize.HashCode;
            HashToTypeDict.Add(hashType, type);
            TypeToHashDict.Add(type, hashType);
            TypeToSerializeDict.Add(type, serialize);
        }

        private static void AddBaseArrayType(Type type, Type itemType)
        {
            if (TypeToSerializeDict.ContainsKey(type))
                return;
            Type bindType;
            if (itemType.IsEnum)
            {
                bindType = typeof(Binding.SystemEnumArrayBind<>).MakeGenericType(itemType);
            }
            else
            {
                var typeName = "Binding." + type.ToString().Replace(".", "").Replace("[]", "") + "ArrayBind";
                bindType = AssemblyHelper.GetTypeNotOptimized(typeName);
            }
            var serialize = Activator.CreateInstance(bindType) as ISerialize;
            serialize.Bind();
            var hashType = serialize.HashCode;
            HashToTypeDict.Add(hashType, type);
            TypeToHashDict.Add(type, hashType);
            TypeToSerializeDict.Add(type, serialize);
        }

        private static void AddBaseListType(Type type, Type itemType)
        {
            if (TypeToSerializeDict.ContainsKey(type))
                return;
            Type bindType;
            if (itemType.IsEnum)
            {
                bindType = typeof(Binding.SystemCollectionsGenericListSystemEnumBind<>).MakeGenericType(itemType);
            }
            else
            {
                var typeName = AssemblyHelper.GetCodeTypeName(type.ToString());
                typeName = typeName.Replace(".", "").Replace("+", "").Replace("<", "").Replace(">", "");
                typeName = "Binding." + typeName + "Bind";
                bindType = AssemblyHelper.GetTypeNotOptimized(typeName);
            }
            var serialize = Activator.CreateInstance(bindType) as ISerialize;
            serialize.Bind();
            var hashType = serialize.HashCode;
            HashToTypeDict.Add(hashType, type);
            TypeToHashDict.Add(type, hashType);
            TypeToSerializeDict.Add(type, serialize);
        }

        public static void InitBindInterfaces()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var bindTypes = new List<IBindingType>();
            foreach (Assembly assembly in assemblies)
            {
                var type = assembly.GetType("Binding.BindingType");
                if (type != null)
                {
                    var bindObj = (IBindingType)Activator.CreateInstance(type);
                    bindTypes.Add(bindObj);
                }
            }
            bindTypes.Sort((a, b) => a.SortingOrder.CompareTo(b.SortingOrder));
            foreach (var bindObj in bindTypes)
            {
                foreach (var bindType in bindObj.BindTypes)
                {
                    BindTypes.Add(bindType.Key, bindType.Value);
                    AddSerializeType(bindType.Key);
                }
            }
        }

        /// <summary>
        /// 添加可序列化的参数类型, 网络参数类型 如果不进行添加将不会被序列化,反序列化
        /// </summary>
        /// <param name="types"></param>
        public static void AddSerializeType(params Type[] types)
        {
            foreach (Type type in types)
            {
                AddSerializeType(type);
            }
        }

        public static ISegment SerializeObject<T>(in T value)
        {
            var stream = BufferPool.Take();
            SerializeObject(value, stream);
            stream.Flush();
            return stream;
        }

        public static void SerializeObject<T>(in T value, ISegment stream)
        {
            try
            {
                var serialize = SerializeCache<T>.Serialize;
                if (serialize == null)
                    throw new Exception($"请注册或绑定:{typeof(T)}类型后才能序列化!");
                serialize.Write(value, stream);
            }
            catch (Exception ex)
            {
                stream.Position = 0;
                NDebug.LogError("序列化:" + value + "出错 详细信息:" + ex);
            }
        }

        public static ISegment SerializeObject(object value)
        {
            var stream = BufferPool.Take();
            SerializeObject(value, stream);
            stream.Count = stream.Position;
            return stream;
        }

        public static void SerializeObject(object value, ISegment stream)
        {
            try
            {
                var type = value.GetType();
                if (TypeToSerializeDict.TryGetValue(type, out ISerialize bind))
                    bind.WriteValue(value, stream);
                else throw new Exception($"请注册或绑定:{type}类型后才能序列化!");
            }
            catch (Exception ex)
            {
                stream.Position = 0;
                NDebug.LogError("序列化:" + value + "出错 详细信息:" + ex);
            }
        }

        public static T DeserializeObject<T>(ISegment segment, bool isPush = true)
        {
            var serialize = SerializeCache<T>.Serialize;
            if (serialize == null)
                throw new Exception($"请注册或绑定:{typeof(T)}类型后才能序列化!");
            T value = serialize.Read(segment);
            if (isPush) BufferPool.Push(segment);
            return value;
        }

        public static object DeserializeObject(Type type, ISegment segment, bool isPush = true)
        {
            if (TypeToSerializeDict.TryGetValue(type, out ISerialize bind))
            {
                object value = bind.ReadValue(segment);
                if (isPush) BufferPool.Push(segment);
                return value;
            }
            throw new Exception($"请注册或绑定:{type}类型后才能反序列化!");
        }

        /// <summary>
        /// 索引取类型
        /// </summary>
        /// <param name="typeIndex"></param>
        /// <returns></returns>
        private static Type IndexToType(ushort typeIndex)
        {
            if (HashToTypeDict.TryGetValue(typeIndex, out Type type))
                return type;
            return null;
        }

        /// <summary>
        /// 类型取索引
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static ushort TypeToIndex(Type type)
        {
            if (TypeToHashDict.TryGetValue(type, out ushort typeHash))
                return typeHash;
            throw new KeyNotFoundException($"没有注册[{type}]类为序列化对象, 请使用序列化生成工具生成{type}绑定类! (如果是基类,请联系作者修复!谢谢)");
        }

        public static byte[] SerializeModel(RPCModel model)
        {
            var segment = BufferPool.Take();
            SerializeModel(segment, model);
            return segment.ToArray(true);
        }

        public static bool SerializeModel(ISegment segment, RPCModel model)
        {
            try
            {
                segment.Write(model.protocol);
                foreach (var obj in model.pars)
                {
                    Type type;
                    if (obj == null)
                    {
                        type = typeof(DBNull);
                        segment.Write(TypeToIndex(type));
                        continue;
                    }
                    type = obj.GetType();
                    segment.Write(TypeToIndex(type));
                    if (TypeToSerializeDict.TryGetValue(type, out ISerialize bind))
                        bind.WriteValue(obj, segment);
                    else throw new Exception($"请注册或绑定:{type}类型后才能序列化!");
                }
                return true;
            }
            catch (Exception ex)
            {
                var func = RPCExtensions.GetFunc(model.protocol);
                NDebug.LogError($"序列化:{func}出错,如果提示为索引溢出,你可以在Call或者Response方法直接设置serialize参数为true 详情:{ex}");
                return false;
            }
        }

        public static bool DeserializeModel(ISegment segment, RPCModel model, bool isPush = true)
        {
            try
            {
                model.protocol = segment.ReadUInt32();
                var list = new List<object>();
                int count = segment.Offset + segment.Count;
                while (segment.Position < count)
                {
                    ushort typeIndex = segment.ReadUInt16();
                    var type = IndexToType(typeIndex);
                    if (type == null)
                        return false;
                    if (type == typeof(DBNull))
                    {
                        list.Add(null);
                        continue;
                    }
                    var obj1 = DeserializeObject(type, segment, false);
                    list.Add(obj1);
                }
                model.pars = list.ToArray();
                if (isPush)
                    BufferPool.Push(segment);
                return true;
            }
            catch (Exception ex)
            {
                var func = RPCExtensions.GetFunc(model.protocol);
                NDebug.LogError($"反序列化:{func}出错:{ex}");
                return false;
            }
        }
    }
}