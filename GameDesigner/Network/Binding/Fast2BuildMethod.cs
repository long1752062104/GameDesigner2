using Net.Helper;
using System.Reflection;
using System.Text;
#if CORE
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
#else
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
#endif

namespace Binding
{
    /// <summary>
    /// 序列化模式
    /// </summary>
    public enum SerializeMode
    {
        /// <summary>
        /// 压缩模式, 如int类型的值是123, 只占用1个字节
        /// </summary>
        Compress,
        /// <summary>
        /// 不压缩模式, 如int类型的值是123, 也一样占用4个字节
        /// </summary>
        NoCompress,
        /// <summary>
        /// 内存直接复制模式, 取类的内存地址直接复制所有字段的数据, 使用内存复制模式, 尽量只使用基础类型, 这样速度飞起
        /// </summary>
        MemoryCopy
    }

    public static class Fast2BuildMethod
    {
        private class Member
        {
            internal string Name;
            internal bool IsPrimitive;
            internal bool IsEnum;
            internal bool IsArray;
            internal bool IsGenericType;
            internal Type Type;
            internal TypeCode TypeCode;
            internal Type ItemType;
            internal Type ItemType1;

            public override string ToString()
            {
                return $"Name:{Name} IsPrimitive:{IsPrimitive} IsEnum:{IsEnum} IsArray:{IsArray} IsGenericType:{IsGenericType}";
            }
        }

        /// <summary>
        /// 动态编译, 在unity开发过程中不需要生成绑定cs文件, 直接运行时编译使用, 当编译apk. app时才进行生成绑定cs文件
        /// </summary>
        /// <param name="serializeMode">true: 使用字节压缩模式生成代码 false: 不进行压缩</param>
        /// <param name="sortingOrder"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        public static bool DynamicBuild(SerializeMode serializeMode, int sortingOrder, params Type[] types)
        {
            var bindTypes = new HashSet<Type>();
            var codes = new Dictionary<string, string>();
            ushort orderId = (ushort)(sortingOrder * 1000);
            foreach (var type in types)
            {
                var genericCodes = new List<string>();
                StringBuilder code;
                orderId++;
                if (serializeMode == SerializeMode.Compress)
                    code = BuildNew(type, ref orderId, true, true, new List<string>(), string.Empty, bindTypes, genericCodes);
                else if (serializeMode == SerializeMode.NoCompress)
                    code = BuildNewFast(type, ref orderId, true, true, new List<string>(), string.Empty, bindTypes, genericCodes);
                else
                    code = BuildMemoryCopy(type, ref orderId, true, true, new List<string>(), string.Empty, bindTypes, genericCodes);
                orderId++;
                code.AppendLine(BuildArray(type, ref orderId).ToString());
                orderId++;
                code.AppendLine(BuildGeneric(typeof(List<>).MakeGenericType(type), ref orderId).ToString());
                foreach (var igenericCode in genericCodes)
                {
                    var index1 = igenericCode.IndexOf("struct") + 7;
                    var index2 = igenericCode.IndexOf(" ", index1);
                    var className = igenericCode.Substring(index1, index2 - index1);
                    codes.Add(className + ".cs", igenericCode);
                }
                codes.Add(type.ToString() + "Bind.cs", code.ToString());
                bindTypes.Add(type);
            }
            codes.Add("Binding.BindingType.cs", BuildBindingType(bindTypes));
            codes.Add("BindingExtension.cs", BuildBindingExtension(bindTypes));
            var assembly = AssemblyHelper.DynamicBuild("SerializeBinding.dll", codes, "Editor", "PackageCache");
            if (assembly != null)
                Net.Serialize.NetConvertFast2.Init();
            return true;
        }

        /// <summary>
        /// 生成所有完整的绑定类型
        /// </summary>
        /// <param name="savePath"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        public static void BuildAll(string savePath, SerializeMode serializeMode, int sortingOrder, params Type[] types)
        {
            var bindTypes = new HashSet<Type>();
            ushort orderId = (ushort)(sortingOrder * 1000);
            foreach (var type in types)
            {
                StringBuilder code;
                orderId++;
                if (serializeMode == SerializeMode.Compress)
                    code = BuildNew(type, ref orderId, true, true, new List<string>(), savePath, bindTypes);
                else if (serializeMode == SerializeMode.NoCompress)
                    code = BuildNewFast(type, ref orderId, true, true, new List<string>(), savePath, bindTypes);
                else
                    code = BuildMemoryCopy(type, ref orderId, true, true, new List<string>(), savePath, bindTypes);
                orderId++;
                code.AppendLine(BuildArray(type, ref orderId).ToString());
                orderId++;
                code.AppendLine(BuildGeneric(typeof(List<>).MakeGenericType(type), ref orderId).ToString());
                var className = type.ToString().Replace(".", "").Replace("+", "");
                File.WriteAllText(savePath + $"//{className}Bind.cs", code.ToString());
                bindTypes.Add(type);
            }
            BuildBindingType(new HashSet<Type>(bindTypes), savePath, 1);
            BuildBindingExtension(new HashSet<Type>(bindTypes), savePath);
        }

        private static List<Member> Filter(Type type, bool serField, bool serProperty, List<string> ignores)
        {
            var memberInfos = type.GetMembers(BindingFlags.Public | BindingFlags.Instance);
            var members = new List<Member>();
            foreach (var memberInfo in memberInfos)
            {
                Type fieldOrPropertyType;
                if (memberInfo is FieldInfo field)
                {
                    if (!serField)
                        continue;
                    if (field.GetCustomAttribute<Net.Serialize.NonSerialized>() != null)
                        continue;
                    if (ignores.Contains(field.Name))
                        continue;
                    fieldOrPropertyType = field.FieldType;
                }
                else if (memberInfo is PropertyInfo property)
                {
                    if (!serProperty)
                        continue;
                    if (property.GetCustomAttribute<Net.Serialize.NonSerialized>() != null)
                        continue;
                    if (!property.CanRead | !property.CanWrite)
                        continue;
                    if (property.GetIndexParameters().Length > 0)
                        continue;
                    if (ignores.Contains(property.Name))
                        continue;
                    fieldOrPropertyType = property.PropertyType;
                }
                else continue;
                var member = new Member()
                {
                    IsArray = fieldOrPropertyType.IsArray,
                    IsEnum = fieldOrPropertyType.IsEnum,
                    IsGenericType = fieldOrPropertyType.IsGenericType,
                    IsPrimitive = Type.GetTypeCode(fieldOrPropertyType) != TypeCode.Object,
                    Name = memberInfo.Name,
                    Type = fieldOrPropertyType,
                    TypeCode = Type.GetTypeCode(fieldOrPropertyType)
                };
                if (fieldOrPropertyType.IsArray)
                {
                    var itemType = fieldOrPropertyType.GetArrayItemType();
                    member.ItemType = itemType;
                }
                else if (fieldOrPropertyType.GenericTypeArguments.Length == 1)
                {
                    Type itemType = fieldOrPropertyType.GenericTypeArguments[0];
                    member.ItemType = itemType;
                }
                else if (fieldOrPropertyType.GenericTypeArguments.Length == 2)
                {
                    Type itemType = fieldOrPropertyType.GenericTypeArguments[0];
                    Type itemType1 = fieldOrPropertyType.GenericTypeArguments[1];
                    member.ItemType = itemType;
                    member.ItemType1 = itemType1;
                }
                members.Add(member);
            }
            return members;
        }

        public static StringBuilder BuildNew(Type type, ref ushort orderId, bool serField, bool serProperty, List<string> ignores, string savePath = null, HashSet<Type> types = null, List<string> genericCodes = null)
        {
            var sb = new StringBuilder();
            var sb1 = new StringBuilder();
            var members = Filter(type, serField, serProperty, ignores);

            var templateText = @"using Net.Serialize;
using Net.System;
using System;
using System.Collections.Generic;

namespace Binding
{
    public readonly struct {TYPENAME}Bind : ISerialize<{TYPE}>, ISerialize
    {
        public ushort HashCode { get { return {orderId}; } }
            
        public void Bind()
		{
			SerializeCache<{TYPE}>.Serialize = this;
		}

        public void Write({TYPE} value, ISegment stream)
        {
            int pos = stream.Position;
            stream.Position += {SIZE};
            var bits = new byte[{SIZE}];
{Split}
            if ({Condition})
            {
                NetConvertBase.SetBit(ref bits[{BITPOS}], {FIELDINDEX}, true);
                stream.Write(value.{FIELDNAME});
            }
{Split}
			if({Condition})
			{
				NetConvertBase.SetBit(ref bits[{BITPOS}], {FIELDINDEX}, true);
                SerializeCache<{FIELDTYPE}>.Serialize.Write(value.{FIELDNAME}, stream);
			}
{Split}
            int pos1 = stream.Position;
            stream.Position = pos;
            stream.Write(bits, 0, {SIZE});
            stream.Position = pos1;
        }
		
        public {TYPE} Read(ISegment stream) 
        {
            var value = new {TYPE}();
            Read(ref value, stream);
            return value;
        }

		public void Read(ref {TYPE} value, ISegment stream)
		{
			var bits = stream.Read({SIZE});
{Split}
			if(NetConvertBase.GetBit(bits[{BITPOS}], {FIELDINDEX}))
				value.{FIELDNAME} = stream.{READTYPE}();
{Split}
			if(NetConvertBase.GetBit(bits[{BITPOS}], {FIELDINDEX}))
				value.{FIELDNAME} = SerializeCache<{FIELDTYPE}>.Serialize.Read(stream);
{Split}
		}

        public void WriteValue(object value, ISegment stream)
        {
            Write(({TYPE})value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }
    }
}
";
            var typeName = type.ToString().Replace(".", "").Replace("+", "");
            var fullName = type.ToString();
            templateText = templateText.Replace("{TYPENAME}", typeName);
            templateText = templateText.Replace("{TYPE}", fullName);
            templateText = templateText.Replace("{SIZE}", $"{((members.Count - 1) / 8) + 1}");
            templateText = templateText.Replace("{orderId}", orderId.ToString());

            var templateTexts = templateText.Split(new string[] { "{Split}" }, 0);

            sb.Append(templateTexts[0]);

            for (int i = 0; i < members.Count; i++)
            {
                int bitInx1 = i % 8;
                int bitPos = i / 8;
                var typecode = Type.GetTypeCode(members[i].Type);
                if (typecode != TypeCode.Object)
                {
                    var templateText1 = templateTexts[1];
                    templateText1 = templateText1.Replace("{BITPOS}", $"{bitPos}");
                    templateText1 = templateText1.Replace("{FIELDINDEX}", $"{++bitInx1}");
                    templateText1 = templateText1.Replace("{FIELDNAME}", $"{members[i].Name}");
                    if (typecode == TypeCode.String)
                        templateText1 = templateText1.Replace("{Condition}", $"!string.IsNullOrEmpty(value.{members[i].Name})");
                    else if (typecode == TypeCode.Boolean)
                        templateText1 = templateText1.Replace("{Condition}", $"value.{members[i].Name} != false");
                    else if (typecode == TypeCode.DateTime)
                        templateText1 = templateText1.Replace("{Condition}", $"value.{members[i].Name} != default");
                    else
                        templateText1 = templateText1.Replace("{Condition}", $"value.{members[i].Name} != 0");
                    sb.Append(templateText1);

                    var templateText2 = templateTexts[4];
                    templateText2 = templateText2.Replace("{BITPOS}", $"{bitPos}");
                    templateText2 = templateText2.Replace("{FIELDINDEX}", $"{bitInx1}");
                    templateText2 = templateText2.Replace("{FIELDNAME}", $"{members[i].Name}");
                    if (members[i].IsEnum)
                        templateText2 = templateText2.Replace("{READTYPE}", $"ReadEnum<{members[i].Type.ToString().Replace("+", ".")}>");
                    else
                        templateText2 = templateText2.Replace("{READTYPE}", $"Read{typecode}");
                    sb1.Append(templateText2);
                }
                else if (members[i].IsArray)
                {
                    typecode = Type.GetTypeCode(members[i].ItemType);
                    if (typecode != TypeCode.Object)
                    {
                        var templateText1 = templateTexts[1];
                        templateText1 = templateText1.Replace("{BITPOS}", $"{bitPos}");
                        templateText1 = templateText1.Replace("{FIELDINDEX}", $"{++bitInx1}");
                        templateText1 = templateText1.Replace("{FIELDNAME}", $"{members[i].Name}");
                        templateText1 = templateText1.Replace("{Condition}", $"value.{members[i].Name} != null");
                        sb.Append(templateText1);

                        var templateText2 = templateTexts[4];
                        templateText2 = templateText2.Replace("{BITPOS}", $"{bitPos}");
                        templateText2 = templateText2.Replace("{FIELDINDEX}", $"{bitInx1}");
                        templateText2 = templateText2.Replace("{FIELDNAME}", $"{members[i].Name}");
                        templateText2 = templateText2.Replace("{READTYPE}", $"Read{typecode}Array");
                        sb1.Append(templateText2);
                    }
                    else
                    {
                        var templateText1 = templateTexts[2];
                        templateText1 = templateText1.Replace("{BITPOS}", $"{bitPos}");
                        templateText1 = templateText1.Replace("{FIELDINDEX}", $"{++bitInx1}");
                        templateText1 = templateText1.Replace("{FIELDNAME}", $"{members[i].Name}");
                        if (members[i].Type.IsValueType)
                            templateText1 = templateText1.Replace("{Condition}", $"value.{members[i].Name} != default");
                        else
                            templateText1 = templateText1.Replace("{Condition}", $"value.{members[i].Name} != null");
                        var local = AssemblyHelper.GetCodeTypeName(members[i].ItemType.ToString());
                        templateText1 = templateText1.Replace("{FIELDTYPE}", $"{local}");
                        sb.Append(templateText1);

                        var templateText2 = templateTexts[5];
                        templateText2 = templateText2.Replace("{BITPOS}", $"{bitPos}");
                        templateText2 = templateText2.Replace("{FIELDINDEX}", $"{bitInx1}");
                        templateText2 = templateText2.Replace("{FIELDNAME}", $"{members[i].Name}");
                        templateText2 = templateText2.Replace("{FIELDTYPE}", $"{local}");
                        sb1.Append(templateText2);
                    }
                }
                else if (members[i].IsGenericType)
                {
                    if (members[i].ItemType1 == null)//List<T>
                    {
                        orderId++;
                        var codeSB = BuildGenericAll(members[i].Type, ref orderId);
                        var local = AssemblyHelper.GetCodeTypeName(members[i].Type.ToString());
                        var className = local.Replace(".", "").Replace("+", "").Replace("<", "").Replace(">", "");
                        if (members[i].Type != typeof(List<>).MakeGenericType(members[i].ItemType))
                        {
                            if (!string.IsNullOrEmpty(savePath))
                                File.WriteAllText(savePath + $"//{className}Bind.cs", codeSB.ToString());
                            genericCodes?.Add(codeSB.ToString());
                        }

                        var templateText1 = templateTexts[2];
                        templateText1 = templateText1.Replace("{BITPOS}", $"{bitPos}");
                        templateText1 = templateText1.Replace("{FIELDINDEX}", $"{++bitInx1}");
                        templateText1 = templateText1.Replace("{FIELDNAME}", $"{members[i].Name}");
                        if (members[i].Type.IsValueType)
                            templateText1 = templateText1.Replace("{Condition}", $"value.{members[i].Name} != default");
                        else
                            templateText1 = templateText1.Replace("{Condition}", $"value.{members[i].Name} != null");

                        templateText1 = templateText1.Replace("{FIELDTYPE}", $"{local}");
                        sb.Append(templateText1);

                        var templateText2 = templateTexts[5];
                        templateText2 = templateText2.Replace("{BITPOS}", $"{bitPos}");
                        templateText2 = templateText2.Replace("{FIELDINDEX}", $"{bitInx1}");
                        templateText2 = templateText2.Replace("{FIELDNAME}", $"{members[i].Name}");
                        templateText2 = templateText2.Replace("{FIELDTYPE}", $"{local}");
                        sb1.Append(templateText2);
                    }
                    else //Dic
                    {
                        var templateText1 = templateTexts[2];
                        templateText1 = templateText1.Replace("{BITPOS}", $"{bitPos}");
                        templateText1 = templateText1.Replace("{FIELDINDEX}", $"{++bitInx1}");
                        templateText1 = templateText1.Replace("{FIELDNAME}", $"{members[i].Name}");
                        templateText1 = templateText1.Replace("{Condition}", $"value.{members[i].Name} != null");

                        var templateText2 = templateTexts[5];
                        templateText2 = templateText2.Replace("{BITPOS}", $"{bitPos}");
                        templateText2 = templateText2.Replace("{FIELDINDEX}", $"{bitInx1}");
                        templateText2 = templateText2.Replace("{FIELDNAME}", $"{members[i].Name}");

                        orderId++;
                        var text = BuildDictionary(members[i].Type, ref orderId, out var className1);
                        if (!string.IsNullOrEmpty(savePath))
                            File.WriteAllText(savePath + $"//{className1}.cs", text);
                        genericCodes?.Add(text);

                        var local = AssemblyHelper.GetCodeTypeName(members[i].Type.ToString());

                        templateText1 = templateText1.Replace("{FIELDTYPE}", $"{local}");
                        sb.Append(templateText1);

                        templateText2 = templateText2.Replace("{FIELDTYPE}", $"{local}");
                        sb1.Append(templateText2);

                        types?.Add(members[i].Type);
                    }
                }
                else
                {
                    var templateText1 = templateTexts[2];
                    templateText1 = templateText1.Replace("{BITPOS}", $"{bitPos}");
                    templateText1 = templateText1.Replace("{FIELDINDEX}", $"{++bitInx1}");
                    templateText1 = templateText1.Replace("{FIELDNAME}", $"{members[i].Name}");

                    if (members[i].Type.IsValueType)
                        templateText1 = templateText1.Replace("{Condition}", $"value.{members[i].Name} != default({members[i].Type})");
                    else
                        templateText1 = templateText1.Replace("{Condition}", $"value.{members[i].Name} != null");
                    var local = AssemblyHelper.GetCodeTypeName(members[i].Type.ToString());
                    templateText1 = templateText1.Replace("{FIELDTYPE}", $"{local}");
                    sb.Append(templateText1);

                    var templateText2 = templateTexts[5];
                    templateText2 = templateText2.Replace("{BITPOS}", $"{bitPos}");
                    templateText2 = templateText2.Replace("{FIELDINDEX}", $"{bitInx1}");
                    templateText2 = templateText2.Replace("{FIELDNAME}", $"{members[i].Name}");
                    templateText2 = templateText2.Replace("{FIELDTYPE}", $"{local}");
                    sb1.Append(templateText2);
                }
            }

            sb.Append(templateTexts[3]);
            sb.Append(sb1);
            sb.Append(templateTexts[6]);
            return sb;
        }

        public static StringBuilder BuildNewFast(Type type, ref ushort orderId, bool serField, bool serProperty, List<string> ignores, string savePath = null, HashSet<Type> types = null, List<string> genericCodes = null)
        {
            var sb = new StringBuilder();
            var sb1 = new StringBuilder();
            var members = Filter(type, serField, serProperty, ignores);

            var templateText = @"using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Net.System;
using Net.Serialize;

namespace Binding
{
    public readonly struct {TYPENAME}Bind : ISerialize<{TYPE}>, ISerialize
    {
        public ushort HashCode { get { return {orderId}; } }
        
        public void Bind()
		{
			SerializeCache<{TYPE}>.Serialize = this;
		}
        
        public unsafe void Write({TYPE} value, ISegment stream)
        {
            fixed (byte* ptr = &stream.Buffer[stream.Position]) 
            {
                int offset = 0;
                bool judge = false;
{Split} //1
                {WRITE}
{Split} //2
                stream.Position += offset;
{Split} //3
                judge = value.{FIELDNAME} != null;
                stream.Write(judge);
                if (judge)
                    SerializeCache<{FIELDTYPE}>.Serialize.Write(value.{FIELDNAME}, stream);
{Split} //4
            }
        }
		
        public {TYPE} Read(ISegment stream) 
        {
            var value = new {TYPE}();
            Read(ref value, stream);
            return value;
        }

		public unsafe void Read(ref {TYPE} value, ISegment stream)
		{
			fixed (byte* ptr = &stream.Buffer[stream.Position]) 
            {
                int offset = 0;
{Split} //5
                {READ}
{Split} //6
                stream.Position += offset;
{Split} //7
                if (stream.ReadBoolean())
                    value.{FIELDNAME} = SerializeCache<{FIELDTYPE}>.Serialize.Read(stream);
{Split} //8
            }
		}

        public void WriteValue(object value, ISegment stream)
        {
            Write(({TYPE})value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }
    }
}
";
            var typeName = type.ToString().Replace(".", "").Replace("+", "");
            var fullName = type.ToString();
            templateText = templateText.Replace("{TYPENAME}", typeName);
            templateText = templateText.Replace("{TYPE}", fullName);
            templateText = templateText.Replace("{SIZE}", $"{((members.Count - 1) / 8) + 1}");
            templateText = templateText.Replace("{orderId}", orderId.ToString());

            var templateTexts = templateText.Split(new string[] { "{Split}" }, 0);

            sb.Append(templateTexts[0]);

            var membersSort = new List<Member>();
            int basisIndex = 0;
            for (int i = 0; i < members.Count; i++)
            {
                var typecode = Type.GetTypeCode(members[i].Type);
                if (typecode != TypeCode.Object)
                {
                    membersSort.Add(members[i]);
                    members.RemoveAt(i);
                    i = -1;
                    basisIndex++;
                }
                else if (members[i].IsArray)
                {
                    typecode = Type.GetTypeCode(members[i].ItemType);
                    if (typecode != TypeCode.Object)
                    {
                        membersSort.Add(members[i]);
                        members.RemoveAt(i);
                        i = -1;
                        basisIndex++;
                    }
                }
                else if (members[i].IsGenericType)
                {
                    if (members[i].ItemType1 == null)//List<T>
                    {
                        typecode = Type.GetTypeCode(members[i].ItemType);
                        if (typecode != TypeCode.Object)
                        {
                            membersSort.Add(members[i]);
                            members.RemoveAt(i);
                            i = -1;
                            basisIndex++;
                        }
                    }
                    else //Dic
                    {
                    }
                }
            }
            membersSort.AddRange(members);
            members = membersSort;

            for (int i = 0; i < members.Count; i++)
            {
                if (basisIndex == i)
                {
                    sb.Append(templateTexts[2]);
                    sb1.Append(templateTexts[6]);
                }

                var typecode = Type.GetTypeCode(members[i].Type);
                if (typecode != TypeCode.Object)
                {
                    var templateText1 = templateTexts[1];
                    templateText1 = templateText1.Replace("{WRITE}", $"NetConvertHelper.Write(ptr, ref offset, value.{members[i].Name});");
                    sb.Append(templateText1);

                    var templateText2 = templateTexts[5];
                    templateText2 = templateText2.Replace("{READ}", $"value.{members[i].Name} = NetConvertHelper.Read{(typecode == TypeCode.String ? "" : $"<{members[i].Type}>")}(ptr, ref offset);");
                    sb1.Append(templateText2);
                }
                else if (members[i].IsArray)
                {
                    typecode = Type.GetTypeCode(members[i].ItemType);
                    if (typecode != TypeCode.Object)
                    {
                        var templateText1 = templateTexts[1];
                        templateText1 = templateText1.Replace("{WRITE}", $"NetConvertHelper.WriteArray(ptr, ref offset, value.{members[i].Name});");
                        sb.Append(templateText1);

                        var templateText2 = templateTexts[5];
                        templateText2 = templateText2.Replace("{READ}", $"value.{members[i].Name} = NetConvertHelper.ReadArray{(typecode == TypeCode.String ? "" : $"<{members[i].ItemType}>")}(ptr, ref offset);");
                        sb1.Append(templateText2);
                    }
                    else
                    {
                        var local = AssemblyHelper.GetCodeTypeName(members[i].ItemType.ToString());

                        var templateText1 = templateTexts[3];
                        templateText1 = templateText1.Replace("{FIELDNAME}", $"{members[i].Name}");
                        templateText1 = templateText1.Replace("{FIELDTYPE}", $"{local}");
                        sb.Append(templateText1);

                        var templateText2 = templateTexts[7];
                        templateText2 = templateText2.Replace("{FIELDNAME}", $"{members[i].Name}");
                        templateText2 = templateText2.Replace("{FIELDTYPE}", $"{local}");
                        sb1.Append(templateText2);
                    }
                }
                else if (members[i].IsGenericType)
                {
                    if (members[i].ItemType1 == null)//List<T>
                    {
                        var codeSB = BuildGenericAll(members[i].Type, ref orderId);
                        var className = AssemblyHelper.GetCodeTypeName(members[i].Type.ToString());
                        className = className.Replace(".", "").Replace("+", "").Replace("<", "").Replace(">", "");
                        if (members[i].Type != typeof(List<>).MakeGenericType(members[i].ItemType))
                        {
                            if (!string.IsNullOrEmpty(savePath))
                                File.WriteAllText(savePath + $"//{className}Bind.cs", codeSB.ToString());
                            genericCodes?.Add(codeSB.ToString());
                        }

                        typecode = Type.GetTypeCode(members[i].ItemType);
                        if (typecode != TypeCode.Object)
                        {
                            var templateText1 = templateTexts[1];
                            var typeName1 = AssemblyHelper.GetCodeTypeName(members[i].Type.ToString());
                            templateText1 = templateText1.Replace("{WRITE}", $"NetConvertHelper.WriteCollection(ptr, ref offset, value.{members[i].Name});");
                            sb.Append(templateText1);

                            var templateText2 = templateTexts[5];
                            templateText2 = templateText2.Replace("{READ}", $"value.{members[i].Name} = NetConvertHelper.ReadCollection<{(typecode == TypeCode.String ? $"{typeName1}" : $"{typeName1}, {members[i].ItemType}")}>(ptr, ref offset);");
                            sb1.Append(templateText2);
                        }
                        else
                        {
                            var local = AssemblyHelper.GetCodeTypeName(members[i].Type.ToString());

                            var templateText1 = templateTexts[3];
                            templateText1 = templateText1.Replace("{FIELDNAME}", $"{members[i].Name}");
                            templateText1 = templateText1.Replace("{FIELDTYPE}", $"{local}");
                            sb.Append(templateText1);

                            var templateText2 = templateTexts[7];
                            templateText2 = templateText2.Replace("{FIELDNAME}", $"{members[i].Name}");
                            templateText2 = templateText2.Replace("{FIELDTYPE}", $"{local}");
                            sb1.Append(templateText2);
                        }
                    }
                    else //Dic
                    {
                        orderId++;
                        var text = BuildDictionary(members[i].Type, ref orderId, out var className1);
                        if (!string.IsNullOrEmpty(savePath))
                            File.WriteAllText(savePath + $"//{className1}.cs", text);
                        genericCodes?.Add(text);

                        var local = AssemblyHelper.GetCodeTypeName(members[i].Type.ToString());

                        var templateText1 = templateTexts[3];
                        templateText1 = templateText1.Replace("{FIELDNAME}", $"{members[i].Name}");
                        templateText1 = templateText1.Replace("{FIELDTYPE}", $"{local}");
                        sb.Append(templateText1);

                        var templateText2 = templateTexts[7];
                        templateText2 = templateText2.Replace("{FIELDNAME}", $"{members[i].Name}");
                        templateText2 = templateText2.Replace("{FIELDTYPE}", $"{local}");
                        sb1.Append(templateText2);

                        types?.Add(members[i].Type);
                    }
                }
                else
                {
                    var local = AssemblyHelper.GetCodeTypeName(members[i].Type.ToString());

                    var templateText1 = templateTexts[3];
                    templateText1 = templateText1.Replace("{FIELDNAME}", $"{members[i].Name}");
                    templateText1 = templateText1.Replace("{FIELDTYPE}", $"{local}");
                    sb.Append(templateText1);

                    var templateText2 = templateTexts[7];
                    templateText2 = templateText2.Replace("{FIELDNAME}", $"{members[i].Name}");
                    templateText2 = templateText2.Replace("{FIELDTYPE}", $"{local}");
                    sb1.Append(templateText2);
                }
            }

            if (basisIndex == members.Count)
            {
                sb.Append(templateTexts[2]);
                sb1.Append(templateTexts[6]);
            }

            sb.Append(templateTexts[4]);
            sb.Append(sb1);
            sb.Append(templateTexts[8]);

            return sb;
        }

        public static StringBuilder BuildMemoryCopy(Type type, ref ushort orderId, bool serField, bool serProperty, List<string> ignores, string savePath = null, HashSet<Type> types = null, List<string> genericCodes = null)
        {
            var sb = new StringBuilder();
            var sb1 = new StringBuilder();
            var members = Filter(type, serField, serProperty, ignores);

            var templateText = @"using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Net.System;
using Net.Serialize;

namespace Binding
{
    public readonly struct {TYPENAME}Bind : ISerialize<{TYPE}>, ISerialize
    {
        public ushort HashCode { get { return {orderId}; } }

        public void Bind()
		{
			SerializeCache<{TYPE}>.Serialize = this;
		}

        public unsafe void Write({TYPE} value, ISegment stream)
        {
            {ReadMemoryAddress}
            fixed (byte* ptr = &stream.Buffer[stream.Position]) 
            {
                int offset = {OFFSET};
                Unsafe.CopyBlockUnaligned(ptr, address, (uint)offset);
                bool judge = false;
{Split} //1
                {WRITE}
{Split} //2
                stream.Position += offset;
{Split} //3
                judge = value.{FIELDNAME} != null;
                stream.Write(judge);
                if (judge)
                    SerializeCache<{FIELDTYPE}>.Serialize.Write(value.{FIELDNAME}, stream);
{Split} //4
            }
        }
		
        public {TYPE} Read(ISegment stream) 
        {
            var value = new {TYPE}();
            Read(ref value, stream);
            return value;
        }

		public unsafe void Read(ref {TYPE} value, ISegment stream)
		{
            {ReadMemoryAddress}
			fixed (byte* ptr = &stream.Buffer[stream.Position]) 
            {
                int offset = {OFFSET};
                Unsafe.CopyBlockUnaligned(address, ptr, (uint)offset);
{Split} //5
                {READ}
{Split} //6
                stream.Position += offset;
{Split} //7
                if (stream.ReadBoolean())
                    value.{FIELDNAME} = SerializeCache<{FIELDTYPE}>.Serialize.Read(stream);
{Split} //8
            }
		}

        public void WriteValue(object value, ISegment stream)
        {
            Write(({TYPE})value, stream);
        }

        public object ReadValue(ISegment stream)
        {
            return Read(stream);
        }
    }
}
";
            var typeName = type.ToString().Replace(".", "").Replace("+", "");
            var fullName = type.ToString();
            templateText = templateText.Replace("{TYPENAME}", typeName);
            templateText = templateText.Replace("{TYPE}", fullName);
            templateText = templateText.Replace("{orderId}", orderId.ToString());
            if (type.IsValueType)
                templateText = templateText.Replace("{ReadMemoryAddress}", "var address = Unsafe.AsPointer(ref value);");
            else
                templateText = templateText.Replace("{ReadMemoryAddress}", "var address = Unsafe.AsPointer(ref value);\r\n\t\t\taddress = (void*)(Unsafe.ReadUnaligned<long>(address) + 8);");

            var templateTexts = templateText.Split(new string[] { "{Split}" }, 0);
            sb.Append(templateTexts[0]);

            var membersSort = new List<Member>();
            int basisIndex = 0;
            for (int i = 0; i < members.Count; i++)
            {
                var typecode = Type.GetTypeCode(members[i].Type);
                if (typecode != TypeCode.Object)
                {
                    membersSort.Add(members[i]);
                    members.RemoveAt(i);
                    i = -1;
                    basisIndex++;
                }
                else if (members[i].IsArray)
                {
                    typecode = Type.GetTypeCode(members[i].ItemType);
                    if (typecode != TypeCode.Object)
                    {
                        membersSort.Add(members[i]);
                        members.RemoveAt(i);
                        i = -1;
                        basisIndex++;
                    }
                }
                else if (members[i].IsGenericType)
                {
                    if (members[i].ItemType1 == null)//List<T>
                    {
                        typecode = Type.GetTypeCode(members[i].ItemType);
                        if (typecode != TypeCode.Object)
                        {
                            membersSort.Add(members[i]);
                            members.RemoveAt(i);
                            i = -1;
                            basisIndex++;
                        }
                    }
                    else //Dic
                    {
                    }
                }
            }
            membersSort.AddRange(members);
            members = membersSort;
            int fieldSize = 0;

            for (int i = 0; i < members.Count; i++)
            {
                if (basisIndex == i)
                {
                    sb.Append(templateTexts[2]);
                    sb1.Append(templateTexts[6]);
                }

                var typecode = Type.GetTypeCode(members[i].Type);
                if (typecode != TypeCode.Object & typecode != TypeCode.String & typecode != TypeCode.DateTime & typecode != TypeCode.Decimal & typecode != TypeCode.DBNull)
                {
                    fieldSize += SizeOf(members[i].Type);
                    continue;
                }
                if (typecode != TypeCode.Object)
                {
                    fieldSize += SizeOf(members[i].Type);

                    var templateText1 = templateTexts[1];
                    templateText1 = templateText1.Replace("{WRITE}", $"NetConvertHelper.Write(ptr, ref offset, value.{members[i].Name});");
                    sb.Append(templateText1);

                    var templateText2 = templateTexts[5];
                    templateText2 = templateText2.Replace("{READ}", $"value.{members[i].Name} = NetConvertHelper.Read{(typecode == TypeCode.String ? "" : $"<{members[i].Type}>")}(ptr, ref offset);");
                    sb1.Append(templateText2);
                }
                else if (members[i].IsArray)
                {
                    fieldSize += 8;

                    typecode = Type.GetTypeCode(members[i].ItemType);
                    if (typecode != TypeCode.Object)
                    {
                        var templateText1 = templateTexts[1];
                        templateText1 = templateText1.Replace("{WRITE}", $"NetConvertHelper.WriteArray(ptr, ref offset, value.{members[i].Name});");
                        sb.Append(templateText1);

                        var templateText2 = templateTexts[5];
                        templateText2 = templateText2.Replace("{READ}", $"value.{members[i].Name} = NetConvertHelper.ReadArray{(typecode == TypeCode.String ? "" : $"<{members[i].ItemType}>")}(ptr, ref offset);");
                        sb1.Append(templateText2);
                    }
                    else
                    {
                        var local = AssemblyHelper.GetCodeTypeName(members[i].Type.ToString());

                        var templateText1 = templateTexts[3];
                        templateText1 = templateText1.Replace("{FIELDNAME}", $"{members[i].Name}");
                        templateText1 = templateText1.Replace("{FIELDTYPE}", $"{local}");
                        sb.Append(templateText1);

                        var templateText2 = templateTexts[7];
                        templateText2 = templateText2.Replace("{FIELDNAME}", $"{members[i].Name}");
                        templateText2 = templateText2.Replace("{FIELDTYPE}", $"{local}");
                        sb1.Append(templateText2);
                    }
                }
                else if (members[i].IsGenericType)
                {
                    fieldSize += 8;

                    if (members[i].ItemType1 == null)//List<T>
                    {
                        var codeSB = BuildGenericAll(members[i].Type, ref orderId);
                        var className = AssemblyHelper.GetCodeTypeName(members[i].Type.ToString());
                        className = className.Replace(".", "").Replace("+", "").Replace("<", "").Replace(">", "");
                        var gType = typeof(List<>).MakeGenericType(members[i].ItemType);
                        if (members[i].Type != gType)
                        {
                            if (!string.IsNullOrEmpty(savePath))
                                File.WriteAllText(savePath + $"//{className}Bind.cs", codeSB.ToString());
                            genericCodes?.Add(codeSB.ToString());
                        }

                        typecode = Type.GetTypeCode(members[i].ItemType);
                        if (typecode != TypeCode.Object)
                        {
                            var templateText1 = templateTexts[1];
                            var typeName1 = AssemblyHelper.GetCodeTypeName(members[i].Type.ToString());
                            templateText1 = templateText1.Replace("{WRITE}", $"NetConvertHelper.WriteCollection(ptr, ref offset, value.{members[i].Name});");
                            sb.Append(templateText1);

                            var templateText2 = templateTexts[5];
                            templateText2 = templateText2.Replace("{READ}", $"value.{members[i].Name} = NetConvertHelper.ReadCollection<{(typecode == TypeCode.String ? $"{typeName1}" : $"{typeName1}, {members[i].ItemType}")}>(ptr, ref offset);");
                            sb1.Append(templateText2);
                        }
                        else
                        {
                            var local = AssemblyHelper.GetCodeTypeName(members[i].Type.ToString());

                            var templateText1 = templateTexts[3];
                            templateText1 = templateText1.Replace("{FIELDNAME}", $"{members[i].Name}");
                            templateText1 = templateText1.Replace("{FIELDTYPE}", $"{local}");
                            sb.Append(templateText1);

                            var templateText2 = templateTexts[7];
                            templateText2 = templateText2.Replace("{FIELDNAME}", $"{members[i].Name}");
                            templateText2 = templateText2.Replace("{FIELDTYPE}", $"{local}");
                            sb1.Append(templateText2);
                        }
                    }
                    else //Dic
                    {
                        orderId++;
                        var text = BuildDictionary(members[i].Type, ref orderId, out var className1);
                        if (!string.IsNullOrEmpty(savePath))
                            File.WriteAllText(savePath + $"//{className1}.cs", text);
                        genericCodes?.Add(text);

                        var local = AssemblyHelper.GetCodeTypeName(members[i].Type.ToString());

                        var templateText1 = templateTexts[3];
                        templateText1 = templateText1.Replace("{FIELDNAME}", $"{members[i].Name}");
                        templateText1 = templateText1.Replace("{FIELDTYPE}", $"{local}");
                        sb.Append(templateText1);

                        var templateText2 = templateTexts[7];
                        templateText2 = templateText2.Replace("{FIELDNAME}", $"{members[i].Name}");
                        templateText2 = templateText2.Replace("{FIELDTYPE}", $"{local}");
                        sb1.Append(templateText2);

                        types?.Add(members[i].Type);
                    }
                }
                else
                {
                    fieldSize += 8;

                    var local = AssemblyHelper.GetCodeTypeName(members[i].Type.ToString());

                    var templateText1 = templateTexts[3];
                    templateText1 = templateText1.Replace("{FIELDNAME}", $"{members[i].Name}");
                    templateText1 = templateText1.Replace("{FIELDTYPE}", $"{local}");
                    sb.Append(templateText1);

                    var templateText2 = templateTexts[7];
                    templateText2 = templateText2.Replace("{FIELDNAME}", $"{members[i].Name}");
                    templateText2 = templateText2.Replace("{FIELDTYPE}", $"{local}");
                    sb1.Append(templateText2);
                }
            }

            if (basisIndex == members.Count)
            {
                sb.Append(templateTexts[2]);
                sb1.Append(templateTexts[6]);
            }

            sb.Append(templateTexts[4]);
            sb.Append(sb1);
            sb.Append(templateTexts[8]);

            sb.Replace("{OFFSET}", fieldSize.ToString());

            return sb;
        }

        public static int SizeOf(Type type)
        {
            var code = Type.GetTypeCode(type);
            switch (code)
            {
                case TypeCode.Empty:
                    return 0;
                case TypeCode.Object:
                    return 8;
                case TypeCode.DBNull:
                    return 8;
                case TypeCode.Boolean:
                    return 1;
                case TypeCode.Char:
                    return 2;
                case TypeCode.SByte:
                    return 1;
                case TypeCode.Byte:
                    return 1;
                case TypeCode.Int16:
                    return 2;
                case TypeCode.UInt16:
                    return 2;
                case TypeCode.Int32:
                    return 4;
                case TypeCode.UInt32:
                    return 4;
                case TypeCode.Int64:
                    return 8;
                case TypeCode.UInt64:
                    return 8;
                case TypeCode.Single:
                    return 4;
                case TypeCode.Double:
                    return 8;
                case TypeCode.Decimal:
                    return 16;
                case TypeCode.DateTime:
                    return 8;
                case TypeCode.String:
                    return 8;
            }
            return 8;
        }

        public static void BuildBindingType(HashSet<Type> types, string savePath, int sortingOrder = 1)
        {
            var code = BuildBindingType(types, sortingOrder);
            File.WriteAllText(savePath + $"//BindingType.cs", code);
        }

        public static string BuildBindingType(HashSet<Type> types, int sortingOrder = 1)
        {
            var str = new StringBuilder();
            str.AppendLine("using System;");
            str.AppendLine("using System.Collections.Generic;");
            str.AppendLine("using Net.Serialize;");
            str.AppendLine("");
            str.AppendLine("namespace Binding");
            str.AppendLine("{");
            str.AppendLine("    public class BindingType : IBindingType");
            str.AppendLine("    {");
            str.AppendLine("        public int SortingOrder { get; private set; }");
            str.AppendLine("        public Dictionary<Type, Type> BindTypes { get; private set; }");
            str.AppendLine("        public BindingType()");
            str.AppendLine("        {");
            str.AppendLine($"            SortingOrder = {sortingOrder};");
            str.AppendLine("            BindTypes = new Dictionary<Type, Type>");
            str.AppendLine("            {");
            foreach (var item in types)
            {
                if (item.IsGenericType & item.GenericTypeArguments.Length == 2)
                {
                    var typeName = AssemblyHelper.GetTypeName(item);
                    var bindType = GetDictionaryBindTypeName(item);
                    str.AppendLine($"\t\t\t\t{{ typeof({typeName}), typeof({bindType}) }},");
                }
                else
                {
                    str.AppendLine($"\t\t\t\t{{ typeof({item.ToString()}), typeof({item.ToString().Replace(".", "")}Bind) }},");
                    str.AppendLine($"\t\t\t\t{{ typeof({item.ToString()}[]), typeof({item.ToString().Replace(".", "")}ArrayBind) }},");
                    var typeName = AssemblyHelper.GetCodeTypeName(typeof(List<>).MakeGenericType(item).ToString());
                    typeName = typeName.Replace(".", "").Replace("+", "").Replace("<", "").Replace(">", "") + "Bind";
                    str.AppendLine($"\t\t\t\t{{ typeof(List<{item.ToString()}>), typeof({typeName}) }},");
                }
            }
            str.AppendLine("            };");
            str.AppendLine("        }");
            str.AppendLine("    }");
            str.AppendLine("}");
            return str.ToString();
        }

        public static void BuildBindingExtension(HashSet<Type> types, string savePath)
        {
            var code = BuildBindingExtension(types);
            File.WriteAllText(savePath + $"//BindingExtension.cs", code);
        }

        public static string BuildBindingExtension(HashSet<Type> types)
        {
            var codeTemplate = @"using Binding;
using Net.System;

public static class BindingExtension
{
{Space}
    public static ISegment SerializeObject(this {TYPE} value)
    {
        var segment = BufferPool.Take();
        var bind = new {TYPEBIND}();
        bind.Write(value, segment);
        return segment;
    }

    public static {TYPE} DeserializeObject(this {TYPE} value, ISegment segment, bool isPush = true)
    {
        var bind = new {TYPEBIND}();
        bind.Read(ref value, segment);
        if (isPush) BufferPool.Push(segment);
        return value;
    }
{Space}
}";

            var str = new StringBuilder();
            var codes = codeTemplate.Split(new string[] { "{Space}" }, 0);
            str.Append(codes[0]);
            foreach (var item in types)
            {
                if (item.IsGenericType)
                    continue;
                var code = codes[1];
                code = code.Replace("{TYPE}", item.ToString());
                code = code.Replace("{TYPEBIND}", $"{item.ToString().Replace(".", "")}Bind");
                str.Append(code);
            }
            str.Append(codes[2]);
            return str.ToString();
        }

        public static StringBuilder BuildArray(Type type, ref ushort orderId)
        {
            var sb = new StringBuilder();
            var templateText = @"
namespace Binding
{
	public readonly struct {TYPENAME}ArrayBind : ISerialize<{TYPE}[]>, ISerialize
	{
        public ushort HashCode { get { return {orderId}; } }

        public void Bind()
		{
			SerializeCache<{TYPE}[]>.Serialize = this;
		}

		public void Write({TYPE}[] value, ISegment stream)
		{
			int count = value.Length;
			stream.Write(count);
			if (count == 0) return;
			var bind = new {BINDTYPE}();
			foreach (var value1 in value)
				bind.Write(value1, stream);
		}

		public {TYPE}[] Read(ISegment stream)
		{
			var count = stream.ReadInt32();
			var value = new {TYPE}[count];
			if (count == 0) return value;
			var bind = new {BINDTYPE}();
			for (int i = 0; i < count; i++)
				value[i] = bind.Read(stream);
			return value;
		}

		public void WriteValue(object value, ISegment stream)
		{
			Write(({TYPE}[])value, stream);
		}

		public object ReadValue(ISegment stream)
		{
			return Read(stream);
		}
	}
}";
            var typeName = type.FullName.Replace(".", "").Replace("+", "");
            var fullName = type.FullName;
            templateText = templateText.Replace("{TYPENAME}", typeName);
            templateText = templateText.Replace("{TYPE}", fullName);
            templateText = templateText.Replace("{orderId}", orderId.ToString());

            var local = typeName + "Bind";
            templateText = templateText.Replace("{BINDTYPE}", $"{local}");

            sb.Append(templateText);
            return sb;
        }

        public static StringBuilder BuildGeneric(Type type, ref ushort orderId)
        {
            var sb = new StringBuilder();
            var templateText = @"
namespace Binding
{
	public readonly struct {TYPENAME}Bind : ISerialize<List<TYPE>>, ISerialize
	{
        public ushort HashCode { get { return {orderId}; } }

        public void Bind()
		{
			SerializeCache<List<TYPE>>.Serialize = this;
		}

		public void Write(List<TYPE> value, ISegment stream)
		{
			int count = value.Count;
			stream.Write(count);
			if (count == 0) return;
			var bind = new {BINDTYPE}();
			foreach (var value1 in value)
				bind.Write(value1, stream);
		}

		public List<TYPE> Read(ISegment stream)
		{
			var count = stream.ReadInt32();
			var value = new List<TYPE>({COUNT});
			if (count == 0) return value;
			var bind = new {BINDTYPE}();
			for (int i = 0; i < count; i++)
				value.Add(bind.Read(stream));
			return value;
		}

		public void WriteValue(object value, ISegment stream)
		{
			Write((List<TYPE>)value, stream);
		}

		public object ReadValue(ISegment stream)
		{
			return Read(stream);
		}
	}
}";
            var typeName = AssemblyHelper.GetCodeTypeName(type.ToString());
            var fullName = typeName;
            typeName = typeName.Replace(".", "").Replace("+", "").Replace("<", "").Replace(">", "");
            templateText = templateText.Replace("{TYPENAME}", typeName);
            templateText = templateText.Replace("List<TYPE>", fullName);
            templateText = templateText.Replace("{orderId}", orderId.ToString());

            var itemTypeName = type.GetArrayItemType().ToString();
            itemTypeName = itemTypeName.Replace(".", "").Replace("+", "").Replace("<", "").Replace(">", "");
            var local = itemTypeName + "Bind";
            templateText = templateText.Replace("{BINDTYPE}", $"{local}");

            var ctor = type.GetConstructor(new Type[] { typeof(int) });
            templateText = templateText.Replace("{COUNT}", ctor != null ? "count" : "");

            sb.Append(templateText);
            return sb;
        }

        public static StringBuilder BuildGenericAll(Type type, ref ushort orderId)
        {
            var sb = new StringBuilder();
            var templateText = @"using Net.System;
using Net.Serialize;

namespace Binding
{
	public readonly struct {TYPENAME}Bind : ISerialize<List<TYPE>>, ISerialize
	{
        public ushort HashCode { get { return {orderId}; } }

        public void Bind()
		{
			SerializeCache<List<TYPE>>.Serialize = this;
		}

		public void Write(List<TYPE> value, ISegment stream)
		{
			int count = value.Count;
			stream.Write(count);
			if (count == 0) return;
			var bind = new {BINDTYPE}();
			foreach (var value1 in value)
				bind.Write(value1, stream);
		}

		public List<TYPE> Read(ISegment stream)
		{
			var count = stream.ReadInt32();
			var value = new List<TYPE>({COUNT});
			if (count == 0) return value;
			var bind = new {BINDTYPE}();
			for (int i = 0; i < count; i++)
				value.Add(bind.Read(stream));
			return value;
		}

		public void WriteValue(object value, ISegment stream)
		{
			Write((List<TYPE>)value, stream);
		}

		public object ReadValue(ISegment stream)
		{
			return Read(stream);
		}
	}
}";
            var typeName = AssemblyHelper.GetCodeTypeName(type.ToString());
            var fullName = typeName;
            typeName = typeName.Replace(".", "").Replace("+", "").Replace("<", "").Replace(">", "");
            templateText = templateText.Replace("{TYPENAME}", typeName);
            templateText = templateText.Replace("List<TYPE>", fullName);
            templateText = templateText.Replace("{orderId}", orderId.ToString());

            var itemTypeName = type.GetArrayItemType().ToString();
            itemTypeName = itemTypeName.Replace(".", "").Replace("+", "").Replace("<", "").Replace(">", "");
            var local = itemTypeName + "Bind";
            templateText = templateText.Replace("{BINDTYPE}", $"{local}");

            var ctor = type.GetConstructor(new Type[] { typeof(int) });
            templateText = templateText.Replace("{COUNT}", ctor != null ? "count" : "");

            sb.Append(templateText);
            return sb;
        }

        public static string BuildDictionary(Type type, ref ushort orderId, out string fileTypeName)
        {
            var text =
    @"using Binding;
using Net.Serialize;
using Net.System;
using System.Collections.Generic;

public readonly struct {Dictionary}_{TKeyName}_{TValueName}_Bind : ISerialize<{Dictionary}<{TKey}, {TValue}>>, ISerialize
{
    public ushort HashCode { get { return {orderId}; } }

    public void Bind()
	{
		SerializeCache<{Dictionary}<{TKey}, {TValue}>>.Serialize = this;
	}

    public void Write({Dictionary}<{TKey}, {TValue}> value, ISegment stream)
    {
        int count = value.Count;
        stream.Write(count);
        if (count == 0) return;
        var bind = new {BindTypeName}();
        foreach (var value1 in value)
        {
            stream.Write(value1.Key);
            bind.Write(value1.Value, stream);
        }
    }

    public {Dictionary}<{TKey}, {TValue}> Read(ISegment stream)
    {
        var count = stream.ReadInt32();
        var value = new {Dictionary}<{TKey}, {TValue}>();
        if (count == 0) return value;
        var bind = new {BindTypeName}();
        for (int i = 0; i < count; i++)
        {
            var key = stream.Read{READ}();
            var value1 = bind.Read(stream);
            value.Add(key, value1);
        }
        return value;
    }

    public void WriteValue(object value, ISegment stream)
    {
        Write(({Dictionary}<{TKey}, {TValue}>)value, stream);
    }

    public object ReadValue(ISegment stream)
    {
        return Read(stream);
    }
}";
            var args = type.GenericTypeArguments;
            string value;
            string typeBindName;
            string keyRead = $"{Type.GetTypeCode(args[0])}";
            if (args[1].IsArray)
            {
                value = args[1].ToString().Replace("+", ".");
                typeBindName = args[1].ToString().ReplaceClear("+", ".", "[", "]", "<", ">") + "ArrayBind";
            }
            else if (args[1].IsGenericType)
            {
                value = AssemblyHelper.GetCodeTypeName(args[1].ToString());
                typeBindName = value.ReplaceClear("+", ".", "[", "]", "<", ">") + "Bind";
            }
            else
            {
                value = args[1].ToString().Replace("+", ".");
                typeBindName = args[1].ToString().Replace(".", "").Replace("+", "") + "Bind";
            }
            text = text.Replace("{TKeyName}", $"{args[0].ToString().Replace(".", "").Replace("+", "")}");
            text = text.Replace("{TValueName}", $"{typeBindName}");
            text = text.Replace("{TKey}", $"{args[0]}");
            text = text.Replace("{TValue}", $"{value}");
            text = text.Replace("{BindTypeName}", $"{typeBindName}");
            text = text.Replace("{READ}", $"{keyRead}");
            var dictName = type.Name.Replace("`2", "");
            text = text.Replace("{Dictionary}", $"{dictName}");
            text = text.Replace("{orderId}", orderId.ToString());

            fileTypeName = $"{dictName}_{args[0].ToString().Replace(".", "")}_{typeBindName}_Bind"; ;
            return text;
        }

        public static string GetDictionaryBindTypeName(Type type)
        {
            var args = type.GenericTypeArguments;
            string typeBindName;
            if (args[1].IsArray)
            {
                typeBindName = args[1].ToString().ReplaceClear("+", ".", "[", "]", "<", ">") + "ArrayBind";
            }
            else if (args[1].IsGenericType)
            {
                var value = AssemblyHelper.GetCodeTypeName(args[1].ToString());
                typeBindName = value.ReplaceClear("+", ".", "[", "]", "<", ">") + "Bind";
            }
            else
            {
                typeBindName = args[1].ToString().Replace(".", "").Replace("+", "") + "Bind";
            }
            var dictName = type.Name.Replace("`2", "");
            var fileTypeName = $"{dictName}_{args[0].ToString().Replace(".", "")}_{typeBindName}_Bind"; ;
            return fileTypeName;
        }
    }
}