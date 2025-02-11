﻿using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
#if SERVICE
using Microsoft.CSharp.RuntimeBinder;
using Binder = Microsoft.CSharp.RuntimeBinder.Binder;
#endif
using Net.Share;
using Net.System;
using Net.Helper;
using Net.Event;

namespace Net.Serialize
{
    public class CollectionConvertBase
    {
        public virtual void Convert(object obj, IList array) { }
    }
    public class CollectionConvert<T> : CollectionConvertBase
    {
        public override void Convert(object obj, IList array)
        {
            if (obj is ICollection<T> collection)
                for (int i = 0; i < array.Count; i++)
                    collection.Add((T)array[i]);
        }
    }

    /// <summary>
    /// 二进制序列化类型
    /// </summary>
    public class BinarySerialize
    {
        private readonly MyDictionary<ushort, Type> HashToTypeDict = new();
        private readonly MyDictionary<Type, ushort> TypeToHashDict = new();
        private readonly MyDictionary<Type, string[]> serializeOnly = new();
        private readonly MyDictionary<Type, string[]> serializeIgnore = new();
        private readonly MyDictionary<Type, Member[]> map = new()
        {
            { typeof(byte), new Member[] { new() { Type = typeof(byte), IsPrimitive = true, TypeCode = TypeCode.Byte } } },
            { typeof(sbyte), new Member[] { new() { Type = typeof(sbyte), IsPrimitive = true, TypeCode = TypeCode.SByte } } },
            { typeof(bool), new Member[] { new() { Type = typeof(bool), IsPrimitive = true, TypeCode = TypeCode.Boolean } } },
            { typeof(short), new Member[] { new() { Type = typeof(short), IsPrimitive = true, TypeCode = TypeCode.Int16 } } },
            { typeof(ushort), new Member[] { new() { Type = typeof(ushort), IsPrimitive = true, TypeCode = TypeCode.UInt16 } } },
            { typeof(char), new Member[] { new() { Type = typeof(char), IsPrimitive = true, TypeCode = TypeCode.Char } } },
            { typeof(int), new Member[] { new() { Type = typeof(int), IsPrimitive = true, TypeCode = TypeCode.Int32 } } },
            { typeof(uint), new Member[] { new() { Type = typeof(uint), IsPrimitive = true, TypeCode = TypeCode.UInt32 } } },
            { typeof(long), new Member[] { new() { Type = typeof(long), IsPrimitive = true, TypeCode = TypeCode.Int64 } } },
            { typeof(ulong), new Member[] { new() { Type = typeof(ulong), IsPrimitive = true, TypeCode = TypeCode.UInt64 } } },
            { typeof(float), new Member[] { new() { Type = typeof(float), IsPrimitive = true, TypeCode = TypeCode.Single } } },
            { typeof(double), new Member[] { new() { Type = typeof(double), IsPrimitive = true, TypeCode = TypeCode.Double } } },
            { typeof(DateTime), new Member[] { new() { Type = typeof(DateTime), IsPrimitive = true, TypeCode = TypeCode.DateTime } } },
            { typeof(decimal), new Member[] { new() { Type = typeof(decimal), IsPrimitive = true, TypeCode = TypeCode.Decimal } } },
            { typeof(string), new Member[] { new() { Type = typeof(string), IsPrimitive = true, TypeCode = TypeCode.String } } },
        };
        private Type nonSerialized = typeof(NonSerialized);
        private Type serialized = typeof(Serialized);

        /// <summary>
        /// 初始化网络转换类型
        /// </summary>
        public bool Init()
        {
            HashToTypeDict.Clear();
            TypeToHashDict.Clear();
            serializeOnly.Clear();
            serializeIgnore.Clear();
            AddSerializeBaseType();
            MakeNonSerializedAttribute<NonSerialized>();
            return true;
        }

        public void MakeSerializedAttribute<T>() where T : Attribute
        {
            serialized = typeof(T);
        }

        public void MakeNonSerializedAttribute<T>() where T : Attribute
        {
            nonSerialized = typeof(T);
        }

        /// <summary>
        /// 添加网络基本类型， int，float，bool，string......
        /// </summary>
        public void AddSerializeBaseType()
        {
            AddBaseType<short>();
            AddBaseType<int>();
            AddBaseType<long>();
            AddBaseType<ushort>();
            AddBaseType<uint>();
            AddBaseType<ulong>();
            AddBaseType<float>();
            AddBaseType<double>();
            AddBaseType<bool>();
            AddBaseType<char>();
            AddBaseType<string>();
            AddBaseType<byte>();
            AddBaseType<sbyte>();
            AddBaseType<DateTime>();
            AddBaseType<decimal>();
            AddBaseType<DBNull>();
            AddBaseType<Type>();
            //基础序列化数组
            AddBaseType<short[]>();
            AddBaseType<int[]>();
            AddBaseType<long[]>();
            AddBaseType<ushort[]>();
            AddBaseType<uint[]>();
            AddBaseType<ulong[]>();
            AddBaseType<float[]>();
            AddBaseType<double[]>();
            AddBaseType<bool[]>();
            AddBaseType<char[]>();
            AddBaseType<string[]>();
            AddBaseType<byte[]>();
            AddBaseType<sbyte[]>();
            AddBaseType<DateTime[]>();
            AddBaseType<decimal[]>();
            //基础序列化List
            AddBaseType<List<short>>();
            AddBaseType<List<int>>();
            AddBaseType<List<long>>();
            AddBaseType<List<ushort>>();
            AddBaseType<List<uint>>();
            AddBaseType<List<ulong>>();
            AddBaseType<List<float>>();
            AddBaseType<List<double>>();
            AddBaseType<List<bool>>();
            AddBaseType<List<char>>();
            AddBaseType<List<string>>();
            AddBaseType<List<byte>>();
            AddBaseType<List<sbyte>>();
            AddBaseType<List<DateTime>>();
            AddBaseType<List<decimal>>();
            //基础序列化List
            AddBaseType<List<short[]>>();
            AddBaseType<List<int[]>>();
            AddBaseType<List<long[]>>();
            AddBaseType<List<ushort[]>>();
            AddBaseType<List<uint[]>>();
            AddBaseType<List<ulong[]>>();
            AddBaseType<List<float[]>>();
            AddBaseType<List<double[]>>();
            AddBaseType<List<bool[]>>();
            AddBaseType<List<char[]>>();
            AddBaseType<List<string[]>>();
            AddBaseType<List<byte[]>>();
            AddBaseType<List<sbyte[]>>();
            AddBaseType<List<DateTime[]>>();
            AddBaseType<List<decimal[]>>();
            //其他可能用到的
            AddSerializeType<Vector2>();
            AddSerializeType<Vector3>();
            AddSerializeType<Vector4>();
            AddSerializeType<Quaternion>(null, "eulerAngles");
            AddSerializeType<Rect>(new string[] { "x", "y", "width", "height" });
            AddSerializeType<Color>(null, "hex");
            AddSerializeType<Color32>(null, "hex");
            AddSerializeType<UnityEngine.Vector2>();
            AddSerializeType<UnityEngine.Vector3>();
            AddSerializeType<UnityEngine.Vector4>();
            AddSerializeType<UnityEngine.Quaternion>(null, "eulerAngles");
            AddSerializeType<UnityEngine.Rect>(new string[] { "x", "y", "width", "height" });
            AddSerializeType<UnityEngine.Color>(null, "hex");
            AddSerializeType<UnityEngine.Color32>(null, "hex");
            //框架操作同步用到
            AddSerializeType<Operation>();
            AddSerializeType<Operation[]>();
            AddSerializeType<OperationList>();
        }

        /// <summary>
        /// 添加可序列化的参数类型, 网络参数类型 如果不进行添加将不会被序列化,反序列化
        /// </summary>
        /// <typeparam name="T">序列化的类型</typeparam>
        /// <param name="onlyFields">只序列化的字段名称列表</param>
        /// <param name="ignoreFields">不序列化的字段名称列表</param>
        public void AddSerializeType<T>(string[] onlyFields = default, params string[] ignoreFields)
        {
            AddSerializeType(typeof(T), onlyFields, ignoreFields);
        }

        /// <summary>
        /// 添加可序列化的参数类型, 网络参数类型 如果不进行添加将不会被序列化,反序列化
        /// </summary>
        /// <param name="type">序列化的类型</param>
        /// <param name="onlyFields">只序列化的字段名称列表</param>
        /// <param name="ignoreFields">不序列化的字段名称列表</param>
        public void AddSerializeType(Type type, string[] onlyFields = default, params string[] ignoreFields)
        {
            var typeHash = (ushort)HashToTypeDict.Count;
            AddSerializeType(type, typeHash, onlyFields, ignoreFields);
        }

        /// <summary>
        /// 添加可序列化的参数类型, 网络参数类型 如果不进行添加将不会被序列化,反序列化
        /// </summary>
        /// <param name="type">序列化的类型</param>
        /// <param name="typeHash">序列化的类型哈希码,用于反序列化识别</param>
        /// <param name="onlyFields">只序列化的字段名称列表</param>
        /// <param name="ignoreFields">不序列化的字段名称列表</param>
        public void AddSerializeType(Type type, ushort typeHash, string[] onlyFields = default, params string[] ignoreFields)
        {
            if (TypeToHashDict.ContainsKey(type))
                throw new Exception($"已经添加{type}键，不需要添加了!");
            HashToTypeDict.Add(typeHash, type);
            TypeToHashDict.Add(type, typeHash);
            serializeOnly.Add(type, onlyFields);
            serializeIgnore.Add(type, ignoreFields);
            GetMembers(type);
        }

        private void AddBaseType<T>()
        {
            var type = typeof(T);
            if (TypeToHashDict.ContainsKey(type))
                return;
            var typeHash = (ushort)HashToTypeDict.Count;
            HashToTypeDict.Add(typeHash, type);
            TypeToHashDict.Add(type, typeHash);
        }

        /// <summary>
        /// 读取索引取类型
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public virtual Type ReadIndexToType(ISegment segment)
        {
            var typeIndex = segment.ReadUInt16();
            if (HashToTypeDict.TryGetValue(typeIndex, out Type type))
                return type;
            return null;
        }

        /// <summary>
        /// 写入类型取索引
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="type"></param>
        /// <exception cref="KeyNotFoundException"></exception>
        public virtual void WriteTypeToIndex(ISegment segment, Type type)
        {
            if (TypeToHashDict.TryGetValue(type, out ushort typeHash))
                segment.Write(typeHash);
            else
                throw new KeyNotFoundException($"没有注册[{type}]类为序列化对象, 请使用NetConvertBinary.AddSerializeType<{type}>()进行注册类型!");
        }

        /// <summary>
        /// 序列化数组实体
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="array"></param>
        /// <param name="itemType"></param>
        /// <param name="recordType"></param>
        /// <param name="ignore"></param>
        private unsafe void WriteArray(ISegment stream, IList array, Type itemType, bool recordType, bool ignore)
        {
            int len = array.Count;
            stream.Write(len); //必须记录长度 因为它的值不是null 而是 XX[0] 或者 List<XX>()
            if (len == 0)
                return;
            var bitLen = ((len - 1) / 8) + 1;
            var bits = new byte[bitLen];
            int strPos = stream.Position;
            stream.Write(bits, 0, bitLen);
            for (int i = 0; i < len; i++)
            {
                var arr1 = array[i];
                int bitInx1 = i % 8;
                int bitPos = i / 8;
                if (arr1 == null)
                    continue;
                SetBit(ref bits[bitPos], bitInx1 + 1, true);
                if (recordType)
                {
                    itemType = arr1.GetType();
                    WriteTypeToIndex(stream, itemType);
                }
                WriteObject(stream, itemType, arr1, recordType, ignore);
            }
            int strLen = stream.Position;
            stream.Position = strPos;
            stream.Write(bits, 0, bitLen);
            stream.Position = strLen;
        }

        /// <summary>
        /// 反序列化数组
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="array"></param>
        /// <param name="itemType"></param>
        /// <param name="recordType"></param>
        /// <param name="ignore"></param>
        private void ReadArray(ISegment segment, ref IList array, Type itemType, bool recordType, bool ignore)
        {
            if (array.Count == 0) //如果长度是0就不需要读取字段位字节了
                return;
            var bitLen = ((array.Count - 1) / 8) + 1;
            byte[] bits = segment.Read(bitLen);
            for (int i = 0; i < array.Count; i++)
            {
                int bitInx1 = i % 8;
                int bitPos = i / 8;
                if (!GetBit(bits[bitPos], (byte)(++bitInx1)))
                    continue;
                if (recordType)
                    itemType = ReadIndexToType(segment);
                array[i] = ReadObject(segment, itemType, recordType, ignore);
            }
        }

        public bool SerializeModel(ISegment segment, RPCModel model, bool recordType = false)
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
                        WriteTypeToIndex(segment, type);
                        continue;
                    }
                    type = obj.GetType();
                    WriteTypeToIndex(segment, type);
                    WriteObject(segment, type, obj, recordType, false);
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

        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="recordType">是否记录类型</param>
        /// <param name="ignore"></param>
        /// <returns></returns>
        public ISegment Serialize(object obj, bool recordType = false, bool ignore = false)
        {
            var stream = BufferPool.Take();
            try
            {
                if (obj == null)
                    return default;
                var type = obj.GetType();
                WriteTypeToIndex(stream, type);
                WriteObject(stream, type, obj, recordType, ignore);
            }
            catch (Exception ex)
            {
                NDebug.LogError("序列化:" + obj + "出错 详细信息:" + ex);
            }
            finally
            {
                stream.Count = stream.Position;
            }
            return stream;
        }

        /// <summary>
        /// 序列化对象, 不记录反序列化类型
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="recordType"></param>
        /// <param name="ignore"></param>
        /// <returns></returns>
        public ISegment SerializeObject(object obj, bool recordType = false, bool ignore = false)
        {
            var stream = BufferPool.Take();
            SerializeObject(stream, obj, recordType, ignore);
            stream.Position = 0;
            return stream;
        }

        /// <summary>
        /// 序列化对象, 不记录反序列化类型
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="obj"></param>
        /// <param name="recordType"></param>
        /// <param name="ignore"></param>
        /// <returns></returns>
        public bool SerializeObject(ISegment stream, object obj, bool recordType = false, bool ignore = false)
        {
            try
            {
                if (obj == null)
                    return false;
                var type = obj.GetType();
                if (recordType)
                    WriteTypeToIndex(stream, type);
                WriteObject(stream, type, obj, recordType, ignore);
                return true;
            }
            catch (Exception ex)
            {
                NDebug.LogError("序列化:" + obj + "出错 详细信息:" + ex);
                return false;
            }
            finally
            {
                stream.Count = stream.Position;
            }
        }

        private class Member
        {
            internal string name;
            internal bool IsPrimitive;
            internal bool IsEnum;
            internal bool IsArray;
            internal bool IsGenericType;
            internal Type Type;
            internal TypeCode TypeCode;
            internal Type ItemType;
            internal Type ItemType1;
            internal bool IsPrimitive1, IsPrimitive2;
            internal Type[] ItemTypes;
            internal object defaultV;
            internal bool Intricate;
#if !SERVICE
            internal Func<object, object> getValue;
            internal Action<object, object> setValue;
#endif
            internal CollectionConvertBase convertMethod;

            internal virtual object GetValue(object obj)
            {
                return obj;
            }

            internal virtual void SetValue(ref object obj, object v)
            {
                obj = v;
            }

            internal virtual void GetValueCall(object callSite)
            {
            }

            internal virtual void SetValueCall(object callSite)
            {
            }

            public override string ToString()
            {
                return $"{name} {Type}";
            }

            internal void ConvertValue(object array1, IList array)
            {
                if (convertMethod == null)
                    convertMethod = (CollectionConvertBase)Activator.CreateInstance(typeof(CollectionConvert<>).MakeGenericType(ItemType)); //如果在il2cpp报错,需要自己hook类型
                convertMethod.Convert(array1, array);
            }
        }
#if SERVICE
        private class FPMember<T> : Member
        {
            internal CallSite<Func<CallSite, object, object>> getValueCall;
            internal CallSite<Func<CallSite, object, T, object>> setValueCall;
            internal override object GetValue(object obj)
            {
                return getValueCall.Target(getValueCall, obj);
            }
            internal override void SetValue(ref object obj, object v)
            {
                setValueCall.Target(setValueCall, obj, (T)v);
            }
            internal override void GetValueCall(object callSite)
            {
                getValueCall = callSite as CallSite<Func<CallSite, object, object>>;
            }
            internal override void SetValueCall(object callSite)
            {
                setValueCall = callSite as CallSite<Func<CallSite, object, T, object>>;
            }
        }
        private class FPArrayMember<T> : Member
        {
            internal CallSite<Func<CallSite, object, object>> getValueCall;
            internal CallSite<Func<CallSite, object, T[], object>> setValueCall;
            internal override object GetValue(object obj)
            {
                return getValueCall.Target(getValueCall, obj);
            }
            internal override void SetValue(ref object obj, object v)
            {
                setValueCall.Target(setValueCall, obj, (T[])v);
            }
            internal override void GetValueCall(object callSite)
            {
                getValueCall = callSite as CallSite<Func<CallSite, object, object>>;
            }
            internal override void SetValueCall(object callSite)
            {
                setValueCall = callSite as CallSite<Func<CallSite, object, T[], object>>;
            }
        }
#else
        private class FPMember : Member
        {
            internal override object GetValue(object obj)
            {
                return getValue(obj);
            }
            internal override void SetValue(ref object obj, object v)
            {
                setValue(obj, v);
            }
        }
#endif
        private Member[] GetMembers(Type type)
        {
            if (!map.TryGetValue(type, out Member[] members2))
            {
                var members1 = new List<Member>();
                if (type.IsArray | type.IsGenericType)
                {
                    var member1 = GetFPMember(null, type, type.FullName, false);
                    members1.Add(member1);
                }
                else
                {
                    var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
                    var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    var members = new List<MemberInfo>(fields);
                    members.AddRange(properties);
                    if (!serializeOnly.TryGetValue(type, out var onlys))
                        onlys = new string[0];
                    else if (onlys == null)
                        onlys = new string[0];
                    if (!serializeIgnore.TryGetValue(type, out var ignores))
                        ignores = new string[0];
                    foreach (var member in members)
                    {
                        if (member.GetCustomAttribute(nonSerialized) != null)
                            continue;
                        if (onlys.Length != 0 & !onlys.Contains(member.Name))
                            continue;
                        if (ignores.Contains(member.Name))
                            continue;
                        Member member1;
                        if (member.MemberType == MemberTypes.Field)
                        {
                            var field = member as FieldInfo;
                            if (field.IsInitOnly) //只读字段不进行序列化
                                continue;
                            var fType = field.FieldType;
                            if (member.GetCustomAttribute(serialized) != null)
                                goto J;
                            if (fType.IsArray)
                            {
                                var arrItemType = fType.GetArrayItemType();
                                if (arrItemType == null)
                                    continue;
                                if (!CanSerialized(arrItemType))
                                    continue;
                            }
                            else if (fType.IsGenericType)
                            {
                                if (fType.GenericTypeArguments.Length == 1)
                                {
                                    var listType = fType.GenericTypeArguments[0];
                                    if (!CanSerialized(listType))
                                        continue;
                                }
                                else if (fType.GenericTypeArguments.Length == 2)
                                {
                                    var keyType = fType.GenericTypeArguments[0];
                                    var valueType = fType.GenericTypeArguments[1];
                                    if (!CanSerialized(keyType))
                                        continue;
                                    if (!CanSerialized(valueType))
                                        continue;
                                }
                            }
                            else
                            {
                                if (!CanSerialized(fType))
                                    continue;
                            }
                        J:
                            member1 = GetFPMember(type, fType, field.Name, true);
#if !SERVICE
                            member1.getValue = field.GetValue;
                            member1.setValue = field.SetValue;
#endif
                            members1.Add(member1);
                        }
                        else if (member.MemberType == MemberTypes.Property)
                        {
                            var property = member as PropertyInfo;
                            if (!property.CanRead | !property.CanWrite)
                                continue;
                            if (property.GetIndexParameters().Length > 0)
                                continue;
                            var pType = property.PropertyType;
                            if (member.GetCustomAttribute(serialized) != null)
                                goto J;
                            if (pType.IsArray)
                            {
                                var arrItemType = pType.GetArrayItemType();
                                if (arrItemType == null)
                                    continue;
                                if (!CanSerialized(arrItemType))
                                    continue;
                            }
                            else if (pType.IsGenericType)
                            {
                                if (pType.GenericTypeArguments.Length == 1)
                                {
                                    var listType = pType.GenericTypeArguments[0];
                                    if (!CanSerialized(listType))
                                        continue;
                                }
                                else if (pType.GenericTypeArguments.Length == 2)
                                {
                                    var keyType = pType.GenericTypeArguments[0];
                                    var valueType = pType.GenericTypeArguments[1];
                                    if (!CanSerialized(keyType))
                                        continue;
                                    if (!CanSerialized(valueType))
                                        continue;
                                }
                            }
                            else
                            {
                                if (!CanSerialized(pType))
                                    continue;
                            }
                        J:
                            member1 = GetFPMember(type, pType, property.Name, true);
#if !SERVICE
                            member1.getValue = property.GetValue;
                            member1.setValue = property.SetValue;
#endif
                            members1.Add(member1);
                        }
                    }
                }
                map.TryAdd(type, members2 = members1.ToArray());
            }
            return members2;
        }

        private bool CanSerialized(Type type)
        {
            if (type == typeof(Type) | type == typeof(object))
                return false;
            if (type.IsClass & type != typeof(string))
            {
                if (type.IsArray)
                {
                    var arrItemType = type.GetArrayItemType();
                    if (arrItemType == null)
                        return false;
                    return CanSerialized(arrItemType);
                }
                else if (type.IsGenericType)
                {
                    if (type.GenericTypeArguments.Length == 1)
                    {
                        var listType = type.GenericTypeArguments[0];
                        return CanSerialized(listType);
                    }
                    else if (type.GenericTypeArguments.Length == 2)
                    {
                        Type keyType = type.GenericTypeArguments[0];
                        Type valueType = type.GenericTypeArguments[1];
                        if (!CanSerialized(keyType))
                            return false;
                        if (!CanSerialized(valueType))
                            return false;
                    }
                }
                var constructors = type.GetConstructors();
                bool hasConstructor = false;
                foreach (var constructor in constructors)
                {
                    if (constructor.GetParameters().Length == 0)
                    {
                        hasConstructor = true;
                        break;
                    }
                }
                if (!hasConstructor)
                    return false;
            }
#if !SERVICE
            if (type.IsSubclassOf(typeof(UnityEngine.Object)) | type == typeof(UnityEngine.Object))
                return false;
#endif
            return true;
        }

        private Member GetFPMember(Type type, Type fpType, string Name, bool isClassField)
        {
#if SERVICE
            var getValueCall = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(0, Name, type, new CSharpArgumentInfo[]
            {
                CSharpArgumentInfo.Create(0, null)
            }));
            var csType = typeof(Func<,,,>).MakeGenericType(typeof(CallSite), typeof(object), fpType, typeof(object));
            var setValueCall = CallSite.Create(csType, Binder.SetMember(0, Name, type, new CSharpArgumentInfo[]
            {
                CSharpArgumentInfo.Create(0, null),
                CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null)
            }));
            var pm = typeof(FPMember<>).MakeGenericType(new Type[] { fpType });
            var member1 = (Member)Activator.CreateInstance(pm);
#else
            Member member1;
            if (!isClassField)
                member1 = new Member();
            else
                member1 = new FPMember();
#endif
            if (fpType.IsArray)
            {
                var itemType = fpType.GetArrayItemType();
#if SERVICE
                if (isClassField)
                {
                    var pm1 = typeof(FPArrayMember<>).MakeGenericType(new Type[] { itemType });
                    member1 = (Member)Activator.CreateInstance(pm1);
                    csType = typeof(Func<,,,>).MakeGenericType(typeof(CallSite), typeof(object), fpType, typeof(object));
                    setValueCall = CallSite.Create(csType, Binder.SetMember(0, Name, type, new CSharpArgumentInfo[]
                    {
                        CSharpArgumentInfo.Create(0, null),
                        CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null)
                    }));
                }
                else
                {
                    member1 = new Member();
                    getValueCall = null;
                    setValueCall = null;
                }
#endif
                member1.ItemType = itemType;
                member1.IsPrimitive1 = Type.GetTypeCode(itemType) != TypeCode.Object;
            }
            else if (fpType.IsGenericType)
            {
#if SERVICE
                if (!isClassField)
                {
                    member1 = new Member();
                    getValueCall = null;
                    setValueCall = null;
                }
#endif
                if (fpType.GenericTypeArguments.Length == 1)
                {
                    var itemType = fpType.GenericTypeArguments[0];
                    member1.ItemType = itemType;
                    member1.Intricate = !fpType.IsInterfaceType(typeof(IList));
                    member1.IsPrimitive1 = Type.GetTypeCode(itemType) != TypeCode.Object;
                }
                else if (fpType.GenericTypeArguments.Length == 2)
                {
                    var keyType = fpType.GenericTypeArguments[0];
                    var valueType = fpType.GenericTypeArguments[1];
                    member1.ItemType = keyType;
                    member1.ItemType1 = valueType;
                    member1.IsPrimitive1 = Type.GetTypeCode(keyType) != TypeCode.Object;
                    if (valueType.IsArray)
                    {
                        var arrItemType = valueType.GetArrayItemType();
                        member1.IsPrimitive2 = Type.GetTypeCode(arrItemType) != TypeCode.Object;
                        member1.ItemTypes = new Type[] { arrItemType };
                    }
                    else if (valueType.IsGenericType & valueType.GenericTypeArguments.Length == 1)
                    {
                        var arrItemType = valueType.GenericTypeArguments[0];
                        member1.IsPrimitive2 = Type.GetTypeCode(arrItemType) != TypeCode.Object;
                        member1.ItemTypes = new Type[] { arrItemType };
                    }
                    else
                    {
                        member1.IsPrimitive2 = Type.GetTypeCode(valueType) != TypeCode.Object;
                        member1.ItemTypes = new Type[] { valueType };
                    }
                }
            }
#if SERVICE
            if (setValueCall != null & getValueCall != null)
            {
                member1.GetValueCall(getValueCall);
                member1.SetValueCall(setValueCall);
            }
#endif
            member1.name = Name;
            member1.IsPrimitive = Type.GetTypeCode(fpType) != TypeCode.Object;
            member1.IsEnum = fpType.IsEnum;
            member1.IsArray = fpType.IsArray;
            member1.IsGenericType = fpType.IsGenericType;
            member1.Type = fpType;
            member1.TypeCode = Type.GetTypeCode(fpType);
            if (member1.IsPrimitive)
            {
                if (fpType == typeof(string))
                    member1.defaultV = null;
                else
                    member1.defaultV = Activator.CreateInstance(member1.Type);
            }
            return member1;
        }

        /// <summary>
        /// 序列化对象
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="type"></param>
        /// <param name="target"></param>
        /// <param name="recordType"></param>
        /// <param name="ignore">忽略不使用<see cref="AddBaseType"/>方法也会被序列化</param>
        public void WriteObject(ISegment segment, Type type, object target, bool recordType, bool ignore)
        {
            var members = GetMembers(type);
            var bitLen = ((members.Length - 1) / 8) + 1;
            var bits = new byte[bitLen];
            var strPos = segment.Position;
            segment.Position += bitLen;
            for (byte i = 0; i < members.Length; i++)
            {
                var member = members[i];
                object value = member.GetValue(target);
                if (value == null)
                    continue;
                int bitInx1 = i % 8;
                int bitPos = i / 8;
                if (member.IsPrimitive)
                {
                    if (!value.Equals(member.defaultV))
                    {
                        segment.WriteValue(value);
                        SetBit(ref bits[bitPos], bitInx1 + 1, true);
                    }
                }
                else if (member.IsEnum)
                {
                    var enumValue = value.GetHashCode();
                    if (enumValue == 0)
                        continue;
                    segment.Write(enumValue);
                    SetBit(ref bits[bitPos], bitInx1 + 1, true);
                }
                else if (member.IsArray | member.IsGenericType)
                {
                    if (member.ItemType1 == null)
                    {
                        if (value is not IList array)
                        {
                            array = Activator.CreateInstance(typeof(List<>).MakeGenericType(member.ItemType)) as IList;
                            var array1 = value as IEnumerable;
                            var enumerator = array1.GetEnumerator();
                            while (enumerator.MoveNext())
                            {
                                array.Add(enumerator.Current);
                            }
                        }
                        SetBit(ref bits[bitPos], bitInx1 + 1, true); //Count = 0也得记录, 因为它不是null, 而是XX[0] 或者 List<XX>(0)
                        if (member.IsPrimitive1)
                        {
                            if (array.Count == 0) //如果长度是0, 写入会报错!
                                segment.Write(0);
                            else if (member.IsArray)
                                segment.WriteArray(array);
                            else
                                segment.WriteList(array);
                        }
                        else WriteArray(segment, array, member.ItemType, recordType, ignore);
                    }
                    else
                    {
                        var dict = (IDictionary)value;
                        SetBit(ref bits[bitPos], bitInx1 + 1, true); //Count = 0也得记录, 因为它不是null, 而是XX[0] 或者 List<XX>(0)
                        if (!member.IsPrimitive1)
                            throw new Exception("字典Key必须是基础类型！");
                        segment.Write(dict.Count);
                        var enumerator = dict.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            segment.WriteValue(enumerator.Key);
                            if (member.ItemType1.IsArray & member.IsPrimitive2)
                            {
                                segment.WriteArray(enumerator.Value);
                            }
                            else if (member.ItemType1.IsGenericType & member.IsPrimitive2)
                            {
                                segment.WriteList(enumerator.Value);
                            }
                            else if (member.IsPrimitive2)
                            {
                                segment.WriteValue(enumerator.Value);
                            }
                            else
                            {
                                Type memberType;
                                if (recordType)
                                {
                                    memberType = enumerator.Value.GetType();
                                    WriteTypeToIndex(segment, memberType);
                                }
                                else memberType = member.ItemType1;
                                WriteObject(segment, memberType, enumerator.Value, recordType, ignore);
                            }
                        }
                    }
                }
                else if (TypeToHashDict.ContainsKey(member.Type) | ignore)
                {
                    SetBit(ref bits[bitPos], bitInx1 + 1, true);
                    Type memberType;
                    if (recordType)
                    {
                        memberType = value.GetType();
                        WriteTypeToIndex(segment, memberType);
                    }
                    else memberType = member.Type;
                    WriteObject(segment, memberType, value, recordType, ignore);
                }
                else throw new Exception($"你没有标记此类[{member.Type}]为可序列化! 请使用NetConvertBinary.AddNetworkType<T>()方法进行添加此类为可序列化类型!");
            }
            var strLen = segment.Position;
            segment.Position = strPos;
            segment.Write(bits, 0, bitLen);
            segment.Position = strLen;
        }

        public bool DeserializeModel(ISegment segment, RPCModel model, bool recordType = false, bool ignore = false)
        {
            try
            {
                model.protocol = segment.ReadUInt32();
                var list = new List<object>();
                while (segment.Position < segment.Offset + segment.Count)
                {
                    var type = ReadIndexToType(segment);
                    if (type == null)
                        return false;
                    if (type == typeof(DBNull))
                    {
                        list.Add(null);
                        continue;
                    }
                    var obj1 = ReadObject(segment, type, recordType, ignore);
                    list.Add(obj1);
                }
                model.pars = list.ToArray();
                return true;
            }
            catch (Exception ex)
            {
                var func = RPCExtensions.GetFunc(model.protocol);
                NDebug.LogError($"反序列化:{func}出错:{ex}");
                return false;
            }
        }

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
        public T DeserializeObject<T>(byte[] buffer, int index, int count, bool recordType = false, bool ignore = false)
        {
            var segment = BufferPool.NewSegment(buffer, index, count, false);
            return DeserializeObject<T>(segment, default, recordType, ignore);
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="segment"></param>
        /// <param name="isPush"></param>
        /// <param name="recordType"></param>
        /// <param name="ignore">忽略不使用<see cref="AddBaseType"/>方法也会被序列化</param>
        /// <returns></returns>
        public T DeserializeObject<T>(ISegment segment, bool isPush = true, bool recordType = false, bool ignore = false)
        {
            return (T)DeserializeObject(segment, typeof(T), isPush, recordType, ignore);
        }

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
        public T DeserializeObject<T>(ISegment segment, Type type, bool isPush = true, bool recordType = false, bool ignore = false)
        {
            return (T)DeserializeObject(segment, type, isPush, recordType, ignore);
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="type"></param>
        /// <param name="isPush"></param>
        /// <param name="recordType"></param>
        /// <param name="ignore">忽略不使用<see cref="AddBaseType"/>方法也会被序列化</param>
        /// <returns></returns>
        public object DeserializeObject(ISegment segment, Type type, bool isPush = true, bool recordType = false, bool ignore = false)
        {
            if (recordType) type = ReadIndexToType(segment);
            var obj = ReadObject(segment, type, recordType, ignore);
            if (isPush) BufferPool.Push(segment);
            return obj;
        }

        public object Deserialize(ISegment segment, bool isPush = true, bool recordType = false, bool ignore = false)
        {
            object obj = null;
            if (segment.Position < segment.Offset + segment.Count)
            {
                var type = ReadIndexToType(segment);
                if (type == null)
                    return obj;
                obj = ReadObject(segment, type, recordType, ignore);
            }
            if (isPush) BufferPool.Push(segment);
            return obj;
        }

        /// <summary>
        /// 反序列化实体对象
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="type"></param>
        /// <param name="recordType"></param>
        /// <param name="ignore"></param>
        /// <returns></returns>
        public object ReadObject(ISegment segment, Type type, bool recordType, bool ignore)
        {
            object obj;
            if (type == typeof(string)) obj = string.Empty;
            else if (type.IsArray) obj = default;
            else obj = Activator.CreateInstance(type);
            var members = GetMembers(type);
            var bitLen = ((members.Length - 1) / 8) + 1;
            var bits = segment.Read(bitLen);
            for (int i = 0; i < members.Length; i++)
            {
                int bitInx1 = i % 8;
                int bitPos = i / 8;
                if (!GetBit(bits[bitPos], (byte)(++bitInx1)))
                    continue;
                var member = members[i];
                if (member.IsPrimitive)//如果是基础类型
                {
                    member.SetValue(ref obj, segment.ReadValue(member.TypeCode));
                }
                else if (member.IsEnum)//如果是枚举类型
                {
                    member.SetValue(ref obj, segment.ReadEnum(member.Type));
                }
                else if (member.IsArray | member.IsGenericType)
                {
                    if (member.ItemType1 == null)//如果itemType1是空的话，说明是List类型，否则是字典
                    {
                        IList array;
                        if (member.IsPrimitive1)
                        {
                            if (member.IsArray)
                                array = (IList)segment.ReadArray(member.ItemType);
                            else
                                array = (IList)segment.ReadList(member.ItemType);
                        }
                        else
                        {
                            if (member.IsArray)
                            {
                                int arrCount = segment.ReadInt32();
                                array = (IList)Activator.CreateInstance(member.Type, arrCount);
                                ReadArray(segment, ref array, member.ItemType, recordType, ignore);
                            }
                            else
                            {
                                int arrCount = segment.ReadInt32();
                                var array1 = Array.CreateInstance(member.ItemType, arrCount);
                                array = (IList)Activator.CreateInstance(member.Type, array1);
                                ReadArray(segment, ref array, member.ItemType, recordType, ignore);
                            }
                        }
                        if (!member.Intricate)
                        {
                            member.SetValue(ref obj, array);
                        }
                        else
                        {
                            var array1 = Activator.CreateInstance(member.Type);
                            member.ConvertValue(array1, array);
                            member.SetValue(ref obj, array1);
                        }
                    }
                    else
                    {
                        var dictCount = segment.ReadInt32();
                        var dict = (IDictionary)Activator.CreateInstance(member.Type);
                        for (int a = 0; a < dictCount; a++)
                        {
                            var key = segment.ReadValue(member.ItemType);
                            object value;
                            if (member.ItemType1.IsArray & member.IsPrimitive2)
                            {
                                value = segment.ReadArray(member.ItemTypes[0]);
                            }
                            else if (member.ItemType1.IsGenericType & member.IsPrimitive2)
                            {
                                value = segment.ReadList(member.ItemTypes[0]);
                            }
                            else if (member.IsPrimitive2)
                            {
                                value = segment.ReadValue(member.ItemType1);
                            }
                            else
                            {
                                Type memberType;
                                if (recordType)
                                    memberType = ReadIndexToType(segment);
                                else
                                    memberType = member.ItemType1;
                                value = ReadObject(segment, memberType, recordType, ignore);
                            }
                            dict.Add(key, value);
                        }
                        member.SetValue(ref obj, dict);
                    }
                }
                else if (TypeToHashDict.ContainsKey(member.Type) | ignore)//如果是序列化类型
                {
                    Type memberType;
                    if (recordType)
                        memberType = ReadIndexToType(segment);
                    else
                        memberType = member.Type;
                    member.SetValue(ref obj, ReadObject(segment, memberType, recordType, ignore));
                }
                else throw new Exception($"你没有标记此类[{member.Type}]为可序列化! 请使用NetConvertBinary.AddNetworkType<T>()方法进行添加此类为可序列化类型!");
            }
            return obj;
        }

        /// <summary>
        /// 设置二进制值
        /// </summary>
        /// <param name="data">要修改的数据</param>
        /// <param name="index">索引从1-8</param>
        /// <param name="flag">填二进制的0或1</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBit(ref byte data, int index, bool flag)
        {
            if (flag)
                data |= (byte)(1 << (8 - index));
            else
                data &= (byte)~(1 << (8 - index));
        }

        /// <summary>
        /// 获取二进制值
        /// </summary>
        /// <param name="data">要获取的数据</param>
        /// <param name="index">索引从1-8</param>
        /// <returns>返回二进制的0或1</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetBit(byte data, byte index)
        {
            return ((data >> (8 - index)) & 1) == 1;
        }
    }
}