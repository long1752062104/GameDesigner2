﻿using Net.Event;
using Net.Serialize;
using Net.Share;
using Net.System;
using System;
using System.Collections;
using System.Reflection;

namespace Net.Helper
{
    public class SyncVarHelper
    {
        public static void InitSyncVar(MemberInfo info, object target, Action<SyncVarInfo> onSyncVarCollect)
        {
            var syncVar = info.GetCustomAttribute<SyncVar>();
            if (syncVar == null)
                return;
            Type type1 = null;
            SyncVarInfo syncVarInfo = null;
            if (info is FieldInfo field)
            {
                type1 = field.FieldType;
                syncVarInfo = new SyncVarFieldInfo();
            }
            else if (info is PropertyInfo property)
            {
                if (!property.CanRead | !property.CanWrite)
                {
                    NDebug.LogError($"错误! {target.GetType().Name}类的{property.Name}属性不能完全读写!");
                    return;
                }
                type1 = property.PropertyType;
                syncVarInfo = new SyncVarPropertyInfo();
            }
            var code = Type.GetTypeCode(type1);
            var isClass = false;
            if (code == TypeCode.Object & type1.IsValueType)
            {
                var fields1 = type1.GetFields(BindingFlags.Public | BindingFlags.Instance);
                foreach (var field1 in fields1)
                {
                    var code1 = Type.GetTypeCode(field1.FieldType);
                    if (code1 == TypeCode.Object)
                    {
                        if (field1.FieldType.IsClass)
                        {
                            isClass = true;
                            break;
                        }
                        int layer = 0;
                        isClass = CheckIsClass(field1.FieldType, ref layer);
                        if (isClass)
                            break;
                    }
                }
            }
            else if (code == TypeCode.Object & type1.IsClass)//解决string, string也是类
                isClass = true;
#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA
            var isUnityObject = type1.IsSubclassOf(typeof(UnityEngine.Object)) | type1 == typeof(UnityEngine.Object);
#else
            var isUnityObject = false;
#endif
            syncVarInfo.id = syncVar.id;
            syncVarInfo.type = type1;
            syncVarInfo.target = target;
            syncVarInfo.authorize = syncVar.authorize;
            syncVarInfo.isEnum = type1.IsEnum;
            syncVarInfo.baseType = code != TypeCode.Object;
            syncVarInfo.isClass = isClass;
            syncVarInfo.isList = type1.IsGenericType | type1.IsArray;
            syncVarInfo.isUnityObject = isUnityObject;
            syncVarInfo.member = info;
            syncVarInfo.Init();
            syncVarInfo.value = isClass & !isUnityObject ? Clone.Instance(syncVarInfo.GetValue()) : syncVarInfo.GetValue();
            if (!string.IsNullOrEmpty(syncVar.hook))
                syncVarInfo.OnValueChanged = target.GetType().GetMethod(syncVar.hook, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            onSyncVarCollect(syncVarInfo);
        }

        private static bool CheckIsClass(Type type, ref int layer, bool root = true)
        {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in fields)
            {
                var code = Type.GetTypeCode(field.FieldType);
                if (code == TypeCode.Object)
                {
                    if (field.FieldType.IsClass)
                        return true;
                    if (root)
                        layer = 0;
                    if (layer++ < 5)
                    {
                        var isClass = CheckIsClass(field.FieldType, ref layer, false);
                        if (isClass)
                            return true;
                    }
                }
            }
            return false;
        }

        private static bool SyncListEquals(IList a, IList b)
        {
            if (a == null | b == null)
                return false;
            if (a.Count != b.Count)
                return false;
            for (int i = 0; i < a.Count; i++)
                if (!a[i].Equals(b[i]))
                    return false;
            return true;
        }

        public static void CheckSyncVar(bool isLocal, MyDictionary<ushort, SyncVarInfo> syncVarInfos, Action<byte[]> OnBuffer)
        {
            Segment segment = null;
            foreach (var syncVar in syncVarInfos.Values)
            {
                if ((!isLocal & !syncVar.authorize) | syncVar.isDispose)
                    continue;
                var value = syncVar.GetValue();
                if (value == null)
                    continue;
                if (syncVar.isList)
                {
                    var a = value as IList;
                    var b = syncVar.value as IList;
                    if (SyncListEquals(a, b))
                        continue;
                }
                else if (value.Equals(syncVar.value))
                    continue;
                if (syncVar.isUnityObject)
                {
                    syncVar.value = value;
#if UNITY_EDITOR
                    string path = UnityEditor.AssetDatabase.GetAssetPath((UnityEngine.Object)value);
                    if (segment == null)
                        segment = BufferPool.Take();
                    segment.Write(syncVar.id);
                    segment.Write(path);
#endif
                    continue;
                }
                if (syncVar.isClass)
                    syncVar.value = Clone.Instance(value);
                else
                    syncVar.value = value;
                if (segment == null)
                    segment = BufferPool.Take();
                segment.Write(syncVar.id);
                if (syncVar.baseType)
                    segment.WriteValue(value);
                else
                    NetConvertBinary.SerializeObject(segment, value, false, true);
            }
            if (segment != null)
            {
                var buffer = segment.ToArray(true);
                OnBuffer(buffer);
            }
        }

        public static void SyncVarHandler(MyDictionary<ushort, SyncVarInfo> syncVarDic, byte[] buffer)
        {
            var segment1 = new Segment(buffer, false);
            while (segment1.Position < segment1.Offset + segment1.Count)
            {
                var index = segment1.ReadUInt16();
                if (!syncVarDic.TryGetValue(index, out var syncVar))
                    break;
                if (syncVar == null)
                    break;
                var oldValue = syncVar.value;
                object value;
                if (syncVar.baseType)
                    value = segment1.ReadValue(syncVar.type);
                else if (syncVar.isUnityObject)
                {
                    var path = segment1.ReadString();
#if UNITY_EDITOR
                    value = UnityEditor.AssetDatabase.LoadAssetAtPath(path, syncVar.type);
                    syncVar.SetValue(value);
                    syncVar.value = value;
#endif
                    continue;
                }
                else
                    value = NetConvertBinary.DeserializeObject(segment1, syncVar.type, false, false, true);
                if (syncVar.isEnum)
                    value = Enum.ToObject(syncVar.type, value);
                if (syncVar.isDispose)
                    continue;
                if (syncVar.isClass)
                {
                    syncVar.SetValue(value);
                    syncVar.value = Clone.Instance(value);
                }
                else
                {
                    syncVar.SetValue(value);
                    syncVar.value = value;
                }
                syncVar.OnValueChanged?.Invoke(syncVar.target, new object[] { oldValue, value });
            }
        }

        public static void RemoveSyncVar(MyDictionary<ushort, SyncVarInfo> syncVarList, object target)
        {
            foreach (var item in syncVarList)
            {
                var syncVar = item.Value;
                if (target.Equals(syncVar.target))
                    syncVar.isDispose = true;
            }
        }
    }
}
