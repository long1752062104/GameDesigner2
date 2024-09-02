using Net.Helper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity
{
    public enum TypeCode
    {
        Empty = 0,
        Object = 1,
        DBNull = 2,
        Boolean = 3,
        Char = 4,
        SByte = 5,
        Byte = 6,
        Int16 = 7,
        UInt16 = 8,
        Int32 = 9,
        UInt32 = 10,
        Int64 = 11,
        UInt64 = 12,
        Single = 13,
        Double = 14,
        Decimal = 0xF,
        DateTime = 0x10,
        String = 18,
        Vector2,
        Vector3,
        Vector4,
        Rect,
        Color,
        Color32,
        Quaternion,
        AnimationCurve,
        GenericType,
        Array,
        Enum,
    }

    [Serializable]
    public class Metadata
    {
        public string name;
        public TypeCode type;

        [Newtonsoft_X.Json.JsonIgnore]
        [Net.Serialize.NonSerialized]
        public string typeName;

        public string data;

        [Newtonsoft_X.Json.JsonIgnore]
        [Net.Serialize.NonSerialized]
        public object target;

        [Newtonsoft_X.Json.JsonIgnore]
        [Net.Serialize.NonSerialized]
        public FieldInfo field;

        [Newtonsoft_X.Json.JsonIgnore]
        [Net.Serialize.NonSerialized]
        public Object Value;

        [Newtonsoft_X.Json.JsonIgnore]
        [Net.Serialize.NonSerialized]
        public List<Object> values;

        [Newtonsoft_X.Json.JsonIgnore]
        [Net.Serialize.NonSerialized]
        public List<Object> Values
        {
            get
            {
                values ??= new List<Object>();
                return values;
            }
            set { values = value; }
        }

        private object _value;
        [Newtonsoft_X.Json.JsonIgnore]
        [Net.Serialize.NonSerialized]
        public object value
        {
            get
            {
                if (target != null & field != null)
                    _value = field.GetValue(target);
                if (_value == null)
                    _value = Read();
                return _value;
            }
            set
            {
                _value = value;
                if (target != null & field != null)
                    field.SetValue(target, _value);
                Write(_value);
            }
        }

        private Type _type;
        [Newtonsoft_X.Json.JsonIgnore]
        [Net.Serialize.NonSerialized]
        public Type Type
        {
            get
            {
                if (_type == null)
                    _type = AssemblyHelper.GetType(typeName);
                return _type;
            }
        }

        public Type _itemType;
        [Newtonsoft_X.Json.JsonIgnore]
        [Net.Serialize.NonSerialized]
        public Type itemType
        {
            get
            {
                if (_itemType == null)
                    if (!GenericTypeArguments.TryGetValue(Type, out _itemType))
                        GenericTypeArguments.Add(Type, _itemType = Type.GetInterface(typeof(IList<>).FullName).GenericTypeArguments[0]);
                return _itemType;
            }
        }

#if UNITY_EDITOR
        [Newtonsoft_X.Json.JsonIgnore]
        [Net.Serialize.NonSerialized]
        public int arraySize;

        [Newtonsoft_X.Json.JsonIgnore]
        [Net.Serialize.NonSerialized]
        public bool foldout;
#endif

        public static readonly Dictionary<Type, Type> GenericTypeArguments = new Dictionary<Type, Type>();

        public Metadata() { }

        public Metadata(string name, string fullName, TypeCode type, object target, FieldInfo field)
        {
            this.name = name;
            typeName = fullName;
            this.type = type;
            this.field = field;
            this.target = target;
            Write(value);
        }

        public object Read()
        {
            switch (type)
            {
                case TypeCode.Byte:
                    return ObjectConverter.AsByte(data);
                case TypeCode.SByte:
                    return ObjectConverter.AsSbyte(data);
                case TypeCode.Boolean:
                    return ObjectConverter.AsBool(data);
                case TypeCode.Int16:
                    return ObjectConverter.AsShort(data);
                case TypeCode.UInt16:
                    return ObjectConverter.AsUshort(data);
                case TypeCode.Char:
                    return ObjectConverter.AsChar(data);
                case TypeCode.Int32:
                    return ObjectConverter.AsInt(data);
                case TypeCode.UInt32:
                    return ObjectConverter.AsUint(data);
                case TypeCode.Single:
                    return ObjectConverter.AsFloat(data);
                case TypeCode.Int64:
                    return ObjectConverter.AsLong(data);
                case TypeCode.UInt64:
                    return ObjectConverter.AsUlong(data);
                case TypeCode.Double:
                    return ObjectConverter.AsDouble(data);
                case TypeCode.String:
                    return data;
                case TypeCode.Enum:
                    return ObjectConverter.AsEnum(data, Type);
                case TypeCode.Object:
                    return Value == null ? null : Value;
                case TypeCode.Vector2:
                    return ObjectConverter.AsVector2(data);
                case TypeCode.Vector3:
                    return ObjectConverter.AsVector3(data);
                case TypeCode.Vector4:
                    return ObjectConverter.AsVector4(data);
                case TypeCode.Quaternion:
                    return ObjectConverter.AsQuaternion(data);
                case TypeCode.Rect:
                    return ObjectConverter.AsRect(data);
                case TypeCode.Color:
                    return ObjectConverter.AsColor(data);
                case TypeCode.Color32:
                    return ObjectConverter.AsColor32(data);
                case TypeCode.GenericType:
                    if (itemType == typeof(Object) | itemType.IsSubclassOf(typeof(Object)))
                    {
                        IList list = (IList)Activator.CreateInstance(Type);
                        for (int i = 0; i < Values.Count; i++)
                        {
                            if (Values[i] == null)
                                list.Add(null);
                            else
                                list.Add(Values[i]);
                        }
                        return list;
                    }
                    else return Newtonsoft_X.Json.JsonConvert.DeserializeObject(data, Type);
                case TypeCode.Array:
                    if (itemType == typeof(Object) | itemType.IsSubclassOf(typeof(Object)))
                    {
                        IList list = Array.CreateInstance(itemType, Values.Count);
                        for (int i = 0; i < Values.Count; i++)
                        {
                            if (Values[i] == null) continue;
                            list[i] = Values[i];
                        }
                        return list;
                    }
                    else return Newtonsoft_X.Json.JsonConvert.DeserializeObject(data, Type);
            }
            return null;
        }

        public void Write(object value)
        {
            if (type == TypeCode.Object)
            {
                Value = (Object)value;
            }
            else if (value != null)
            {
                if (type == TypeCode.Vector2)
                {
                    Vector2 v2 = (Vector2)value;
                    data = $"{v2.x},{v2.y}";
                }
                else if (type == TypeCode.Vector3)
                {
                    Vector3 v = (Vector3)value;
                    data = $"{v.x},{v.y},{v.z}";
                }
                else if (type == TypeCode.Vector4)
                {
                    Vector4 v = (Vector4)value;
                    data = $"{v.x},{v.y},{v.z},{v.w}";
                }
                else if (type == TypeCode.Quaternion)
                {
                    Quaternion v = (Quaternion)value;
                    data = $"{v.x},{v.y},{v.z},{v.w}";
                }
                else if (type == TypeCode.Rect)
                {
                    Rect v = (Rect)value;
                    data = $"{v.x},{v.y},{v.width},{v.height}";
                }
                else if (type == TypeCode.Color)
                {
                    Color v = (Color)value;
                    data = $"{v.r},{v.g},{v.b},{v.a}";
                }
                else if (type == TypeCode.Color32)
                {
                    Color32 v = (Color32)value;
                    data = $"{v.r},{v.g},{v.b},{v.a}";
                }
                else if (type == TypeCode.GenericType | type == TypeCode.Array)
                {
                    if (itemType == typeof(Object) | itemType.IsSubclassOf(typeof(Object)))
                    {
                        Values.Clear();
                        IList list = (IList)value;
                        for (int i = 0; i < list.Count; i++)
                            Values.Add(list[i] as Object);
                    }
                    else data = Newtonsoft_X.Json.JsonConvert.SerializeObject(value);
                }
                else data = value.ToString();
            }
            else
            {
                data = null;
            }
        }
    }
}
