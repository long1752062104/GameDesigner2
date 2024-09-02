#if UNITY_EDITOR
using System;
using System.Collections;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;
using System.Reflection;
using System.Text.RegularExpressions;
using Net.Helper;

namespace Unity.Editor
{
    public class MetadataValue
    {
        public static MetadataValue Create(TypeCode typeCode, string data)
        {
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return new MetadataValue<bool>(ObjectConverter.AsBool(data));
                case TypeCode.Char:
                    return new MetadataValue<char>(ObjectConverter.AsChar(data));
                case TypeCode.SByte:
                    return new MetadataValue<sbyte>(ObjectConverter.AsSbyte(data));
                case TypeCode.Byte:
                    return new MetadataValue<byte>(ObjectConverter.AsByte(data));
                case TypeCode.Int16:
                    return new MetadataValue<short>(ObjectConverter.AsShort(data));
                case TypeCode.UInt16:
                    return new MetadataValue<ushort>(ObjectConverter.AsUshort(data));
                case TypeCode.Int32:
                    return new MetadataValue<int>(ObjectConverter.AsInt(data));
                case TypeCode.UInt32:
                    return new MetadataValue<uint>(ObjectConverter.AsUint(data));
                case TypeCode.Int64:
                    return new MetadataValue<long>(ObjectConverter.AsLong(data));
                case TypeCode.UInt64:
                    return new MetadataValue<ulong>(ObjectConverter.AsUlong(data));
                case TypeCode.Single:
                    return new MetadataValue<float>(ObjectConverter.AsFloat(data));
                case TypeCode.Double:
                    return new MetadataValue<double>(ObjectConverter.AsDouble(data));
                case TypeCode.Decimal:
                    return new MetadataValue<decimal>(ObjectConverter.AsDecimal(data));
                case TypeCode.DateTime:
                    return new MetadataValue<DateTime>(ObjectConverter.AsDateTime(data));
                case TypeCode.String:
                    return new MetadataValue<string>(ObjectConverter.AsString(data));
                case TypeCode.Vector2:
                    return new MetadataValue<Vector2>(ObjectConverter.AsVector2(data));
                case TypeCode.Vector3:
                    return new MetadataValue<Vector3>(ObjectConverter.AsVector3(data));
                case TypeCode.Vector4:
                    return new MetadataValue<Vector4>(ObjectConverter.AsVector4(data));
                case TypeCode.Rect:
                    return new MetadataValue<Rect>(ObjectConverter.AsRect(data));
                case TypeCode.Color:
                    return new MetadataValue<Color>(ObjectConverter.AsColor(data));
                case TypeCode.Color32:
                    return new MetadataValue<Color32>(ObjectConverter.AsColor32(data));
                case TypeCode.Quaternion:
                    return new MetadataValue<Quaternion>(ObjectConverter.AsQuaternion(data));
                case TypeCode.AnimationCurve:
                    return new MetadataValue<AnimationCurve>(new AnimationCurve());
            }
            return new MetadataValue<DBNull>(DBNull.Value);
        }
    }

    public class MetadataValue<T> : MetadataValue
    {
        public T value;

        public MetadataValue(T value)
        {
            this.value = value;
        }
    }

    [CustomPropertyDrawer(typeof(Metadata))]
    public class MetadataDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (GetMetadata(property) is not Metadata metadata)
                return;
            if (metadata.target == null)
            {
                var value = MetadataValue.Create(metadata.type, metadata.data);
                metadata.target = value;
                metadata.field = value.GetType().GetField("value");
            }
            EditorGUI.BeginChangeCheck();
            var width = position.width;
            position.x = 0;
            position.width = width / 3.5f;
            metadata.name = EditorGUI.TextField(position, metadata.name, EditorStyles.label);
            position.x += position.width;
            position.width = 90;
            var typeCode = metadata.type;
            metadata.type = (TypeCode)EditorGUI.EnumPopup(position, typeCode);
            position.x += 110;
            position.width = width - position.x;
            if (metadata.type != typeCode)
            {
                metadata.target = null;
                metadata.field = null;
                metadata.data = string.Empty;
            }
            else
            {
                try { PropertyField(position, string.Empty, metadata); } catch { }
            }
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(property.serializedObject.targetObject);
        }

        private object GetMetadata(SerializedProperty property)
        {
            var path = property.propertyPath;
            path = Regex.Replace(path, "\\.Array\\.data", ".___ArrayElement___");
            object target = property.serializedObject.targetObject;
            var type = target.GetType();
            var strArray = path.Split('.', StringSplitOptions.None);
            for (int index = 0; index < strArray.Length; ++index)
            {
                string name = strArray[index];
                FieldInfo fieldInfo = null;
                for (Type type1 = type; fieldInfo == null && type1 != null; type1 = type1.BaseType)
                    fieldInfo = type1.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (fieldInfo == null)
                    break;
                type = fieldInfo.FieldType;
                target = fieldInfo.GetValue(target);
                if (index < strArray.Length - 1 && strArray[index + 1].StartsWith("___ArrayElement___") && type.IsArrayOrList())
                {
                    var arrayElement = strArray[index + 1];
                    arrayElement = arrayElement.Replace("___ArrayElement___[", "").Replace("]", "");
                    var array = target as IList; //IList数组或者索引才能获取
                    target = array[int.Parse(arrayElement)];

                    type = type.GetArrayOrListElementType();
                    index++;
                }
            }
            return target;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 18f;
        }

        public static void PropertyField(Rect position, Metadata metadata, float width = 40f, int arrayBeginSpace = 3, int arrayEndSpace = 2)
        {
            PropertyField(position, metadata.name, metadata, width, arrayBeginSpace, arrayEndSpace);
        }

        public static void PropertyField(Rect position, string label, Metadata metadata, float width = 40f, int arrayBeginSpace = 3, int arrayEndSpace = 2)
        {
            switch (metadata.type)
            {
                case TypeCode.Empty:
                    break;
                case TypeCode.Object:
                    metadata.value = EditorGUI.ObjectField(position, label, (Object)metadata.value, metadata.Type, true);
                    break;
                case TypeCode.DBNull:
                    break;
                case TypeCode.Boolean:
                    metadata.value = EditorGUI.Toggle(position, label, (bool)metadata.value);
                    break;
                case TypeCode.Char:
                    metadata.value = EditorGUI.TextField(position, label, metadata.value.ToString())[0];
                    break;
                case TypeCode.SByte:
                    metadata.value = (sbyte)EditorGUI.IntField(position, label, (sbyte)metadata.value);
                    break;
                case TypeCode.Byte:
                    metadata.value = (byte)EditorGUI.IntField(position, label, (byte)metadata.value);
                    break;
                case TypeCode.Int16:
                    metadata.value = (short)EditorGUI.IntField(position, label, (short)metadata.value);
                    break;
                case TypeCode.UInt16:
                    metadata.value = (ushort)EditorGUI.IntField(position, label, (ushort)metadata.value);
                    break;
                case TypeCode.Int32:
                    metadata.value = EditorGUI.IntField(position, label, (int)metadata.value);
                    break;
                case TypeCode.UInt32:
                    metadata.value = (uint)EditorGUI.IntField(position, label, (int)metadata.value);
                    break;
                case TypeCode.Int64:
                    metadata.value = EditorGUI.LongField(position, label, (long)metadata.value);
                    break;
                case TypeCode.UInt64:
                    metadata.value = (ulong)EditorGUI.LongField(position, label, (long)metadata.value);
                    break;
                case TypeCode.Single:
                    metadata.value = EditorGUI.FloatField(position, label, (float)metadata.value);
                    break;
                case TypeCode.Double:
                    metadata.value = EditorGUI.DoubleField(position, label, (double)metadata.value);
                    break;
                case TypeCode.Decimal:
                    break;
                case TypeCode.DateTime:
                    break;
                case TypeCode.String:
                    metadata.value = EditorGUI.TextField(position, label, metadata.value.ToString());
                    break;
                case TypeCode.Vector2:
                    metadata.value = EditorGUI.Vector2Field(position, label, (Vector2)metadata.value);
                    break;
                case TypeCode.Vector3:
                    metadata.value = EditorGUI.Vector3Field(position, label, (Vector3)metadata.value);
                    break;
                case TypeCode.Vector4:
                    metadata.value = EditorGUI.Vector4Field(position, label, (Vector4)metadata.value);
                    break;
                case TypeCode.Rect:
                    metadata.value = EditorGUI.RectField(position, label, (Rect)metadata.value);
                    break;
                case TypeCode.Color:
                    metadata.value = EditorGUI.ColorField(position, label, (Color)metadata.value);
                    break;
                case TypeCode.Color32:
                    metadata.value = (Color32)EditorGUI.ColorField(position, label, (Color32)metadata.value);
                    break;
                case TypeCode.Quaternion:
                    {
                        var q = (Quaternion)metadata.value;
                        var value = EditorGUI.Vector4Field(position, label, new Vector4(q.x, q.y, q.z, q.w));
                        var q1 = new Quaternion(value.x, value.y, value.z, value.w);
                        metadata.value = q1;
                    }
                    break;
                case TypeCode.AnimationCurve:
                    metadata.value = EditorGUI.CurveField(position, label, (AnimationCurve)metadata.value);
                    break;
                case TypeCode.Enum:
                    metadata.value = EditorGUI.EnumPopup(position, label, (Enum)metadata.value);
                    break;
                default: //这里是状态机行为组件才显示
                    var rect = EditorGUILayout.GetControlRect();
                    rect.x += width;
                    metadata.foldout = EditorGUI.BeginFoldoutHeaderGroup(rect, metadata.foldout, label);
                    if (metadata.foldout)
                    {
                        EditorGUI.indentLevel = arrayBeginSpace;
                        EditorGUI.BeginChangeCheck();
                        var arraySize = EditorGUILayout.DelayedIntField("Size", metadata.arraySize);
                        var flag8 = EditorGUI.EndChangeCheck();
                        var list = (IList)metadata.value;
                        if (flag8 | list.Count != metadata.arraySize)
                        {
                            metadata.arraySize = arraySize;
                            var list1 = (IList)Array.CreateInstance(metadata.itemType, arraySize);
                            for (int i = 0; i < list1.Count; i++)
                                if (i < list.Count)
                                    list1[i] = list[i];
                            if (metadata.type == TypeCode.GenericType)
                            {
                                var list2 = (IList)Activator.CreateInstance(metadata.Type);
                                for (int i = 0; i < list1.Count; i++)
                                    list2.Add(list1[i]);
                                list = list2;
                            }
                            else list = list1;
                        }
                        for (int i = 0; i < list.Count; i++)
                        {
                            rect = EditorGUILayout.GetControlRect();
                            list[i] = PropertyField(rect, "Element " + i, list[i], metadata.itemType);
                        }
                        metadata.value = list;
                        EditorGUI.indentLevel = arrayEndSpace;
                    }
                    EditorGUI.EndFoldoutHeaderGroup();
                    break;
            }
        }

        public static object PropertyField(Rect position, string name, object obj, Type type)
        {
            var typeCode = (TypeCode)Type.GetTypeCode(type);
            switch (typeCode)
            {
                case TypeCode.Empty:
                    break;
                case TypeCode.Object:
                    if (type.IsSubclassOf(typeof(Object)) | type == typeof(Object))
                        obj = EditorGUI.ObjectField(position, name, (Object)obj, type, true);
                    break;
                case TypeCode.DBNull:
                    break;
                case TypeCode.Boolean:
                    obj = EditorGUI.Toggle(position, name, (bool)obj);
                    break;
                case TypeCode.Char:
                    obj = EditorGUI.TextField(position, name, (string)obj)[0];
                    break;
                case TypeCode.SByte:
                    obj = (sbyte)EditorGUI.IntField(position, name, (sbyte)obj);
                    break;
                case TypeCode.Byte:
                    obj = (byte)EditorGUI.IntField(position, name, (byte)obj);
                    break;
                case TypeCode.Int16:
                    obj = (short)EditorGUI.IntField(position, name, (short)obj);
                    break;
                case TypeCode.UInt16:
                    obj = (ushort)EditorGUI.IntField(position, name, (ushort)obj);
                    break;
                case TypeCode.Int32:
                    obj = EditorGUI.IntField(position, name, (int)obj);
                    break;
                case TypeCode.UInt32:
                    obj = (uint)EditorGUI.IntField(position, name, (int)obj);
                    break;
                case TypeCode.Int64:
                    obj = EditorGUI.LongField(position, name, (long)obj);
                    break;
                case TypeCode.UInt64:
                    obj = (ulong)EditorGUI.LongField(position, name, (long)obj);
                    break;
                case TypeCode.Single:
                    obj = EditorGUI.FloatField(position, name, (float)obj);
                    break;
                case TypeCode.Double:
                    obj = EditorGUI.DoubleField(position, name, (double)obj);
                    break;
                case TypeCode.Decimal:
                    break;
                case TypeCode.DateTime:
                    break;
                case TypeCode.String:
                    obj = EditorGUI.TextField(position, name, (string)obj);
                    break;
                case TypeCode.Vector2:
                    obj = EditorGUI.Vector2Field(position, name, (Vector2)obj);
                    break;
                case TypeCode.Vector3:
                    obj = EditorGUI.Vector3Field(position, name, (Vector3)obj);
                    break;
                case TypeCode.Vector4:
                    obj = EditorGUI.Vector4Field(position, name, (Vector4)obj);
                    break;
                case TypeCode.Rect:
                    obj = EditorGUI.RectField(position, name, (Rect)obj);
                    break;
                case TypeCode.Color:
                    obj = EditorGUI.ColorField(position, name, (Color)obj);
                    break;
                case TypeCode.Color32:
                    obj = EditorGUI.ColorField(position, name, (Color32)obj);
                    break;
                case TypeCode.Quaternion:
                    {
                        var value = EditorGUI.Vector4Field(position, name, (Vector4)obj);
                        var quaternion = new Quaternion(value.x, value.y, value.z, value.w);
                        obj = quaternion;
                    }
                    break;
                case TypeCode.AnimationCurve:
                    obj = EditorGUI.CurveField(position, name, (AnimationCurve)obj);
                    break;
                case TypeCode.GenericType:
                    break;
                case TypeCode.Array:
                    break;
                case TypeCode.Enum:
                    obj = EditorGUI.EnumPopup(position, name, (Enum)obj);
                    break;
            }
            return obj;
        }
    }
}
#endif