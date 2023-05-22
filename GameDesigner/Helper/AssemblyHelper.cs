using System;
using System.Collections.Generic;

namespace Net.Helper
{
    public class AssemblyHelper
    {
        private static readonly Dictionary<string, Type> TypeDict = new Dictionary<string, Type>();
        private static readonly HashSet<string> NotTypes = new HashSet<string>();

        /// <summary>
        /// ��Ӻ���Ҫ���ҵ�����
        /// </summary>
        /// <param name="type"></param>
        public static void AddFindType(Type type) 
        {
            TypeDict[type.ToString()] = type;
        }

        /// <summary>
        /// ��ȡ���ͣ� ��������Ѿ���ȡ��һ����ֱ��ȡ�������������г��򼯻�ȡ���ͣ�������ҵ������ӵ������ֵ��У��´β���Ҫ�ٱ������г���
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static Type GetType(string typeName)
        {
            if (TypeDict.TryGetValue(typeName, out var type))
                return type;
            if (NotTypes.Contains(typeName))
                goto JMP;
            type = Type.GetType(typeName);
            if (type != null)
            {
                TypeDict.Add(typeName, type);
                return type;
            }
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                type = assembly.GetType(typeName);
                if (type != null)
                {
                    TypeDict[typeName] = type;
                    return type;
                }
            }
            if (typeName.IndexOf("[") >= 0) //��Ȼ֧���˷��ͺ��ֵ���, ���ǲ�Ҫ���̫���ӻ�����ڵĻ�Ӱ�����ܺͲ���bug, ���ﻹ��һ������, ���ǵ�����il2cpp�����Ҳ���ȡ����
            {
                var index = typeName.IndexOf("[");
                var itemTypeName = typeName.Substring(0, index);
                var genericType = GetType(itemTypeName);
                if (genericType == null)
                    goto JMP;
                itemTypeName = typeName.Remove(0, index + 1);
                index = StringHelper.FindHitCount(itemTypeName, '[');
                StringHelper.RemoveHit(ref itemTypeName, ']', index);
                var itemTypeList = new List<Type>();
                if (genericType.GetGenericArguments().Length == 1)
                {
                    var itemType = GetType(itemTypeName);
                    if (itemType == null)
                        goto JMP;
                    itemTypeList.Add(itemType);
                }
                else
                {
                    var itemTypeNames = itemTypeName.Split(',');
                    foreach (var itemTypeNameN in itemTypeNames)
                    {
                        var itemType = GetType(itemTypeNameN);
                        if (itemType == null)
                            goto JMP;
                        itemTypeList.Add(itemType);
                    }
                }
                type = genericType.MakeGenericType(itemTypeList.ToArray());
                TypeDict[typeName] = type;
                return type;
            }
            NotTypes.Add(typeName);
        JMP: Event.NDebug.LogError($"�Ҳ�������:{typeName}, ����̫����ʱ��Ҫʹ�� AssemblyHelper.AddFindType(type) ��Ǻ���Ҫ���ҵ���");
            return null;
        }

        public static Type GetTypeNotOptimized(string typeName) 
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var type = assembly.GetType(typeName);
                if (type != null)
                    return type;
            }
            return null;
        }

        /// <summary>
        /// ��ȡ������ʽ����������, ��������,����
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetTypeName(Type type, 
            string baseBegin = "", string baseEnd = "",
            string normalBegin = "", string normalEnd = "",
            string baseArrayBegin = "", string baseArrayEnd = "",
            string arrayBegin = "", string arrayEnd = "",
            string baseGenericBegin = "", string baseGenericEnd = "",
            string genericBegin = "", string genericEnd = ""
            )
        {
            string typeName;
            if (type.IsArray)
            {
                var interfaceType = type.GetInterface(typeof(IList<>).FullName);
                var type1 = interfaceType.GetGenericArguments()[0];
                var typecode = Type.GetTypeCode(type1);
                if (typecode == TypeCode.Object)
                {
                    var typeName1 = GetTypeName(type1, baseBegin, baseEnd, normalBegin, normalEnd, baseArrayBegin, baseArrayEnd, arrayBegin, arrayEnd, baseGenericBegin, baseGenericEnd, genericBegin, genericEnd);
                    typeName = arrayBegin + $"{typeName1}[]" + arrayEnd;
                }
                else
                {
                    typeName = baseArrayBegin + $"{type1}[]" + baseArrayEnd;
                }
            }
            else if (type.IsGenericType)
            {
                typeName = type.ToString();
                var index = typeName.IndexOf("`");
                var count = typeName.IndexOf("[");
                typeName = typeName.Remove(index, count + 1 - index);
                typeName = typeName.Insert(index, "<");
                typeName = typeName.Substring(0, index + 1);
                var genericTypes = type.GenericTypeArguments;
                foreach (var item in genericTypes)
                {
                    var typecode = Type.GetTypeCode(item);
                    if (typecode == TypeCode.Object)
                    {
                        var typeName1 = GetTypeName(item, baseBegin, baseEnd, normalBegin, normalEnd, baseArrayBegin, baseArrayEnd, arrayBegin, arrayEnd, baseGenericBegin, baseGenericEnd, genericBegin, genericEnd);
                        typeName += genericBegin + $"{typeName1}" + genericEnd + ",";
                    }
                    else
                    {
                        var typeName1 = item.ToString();
                        typeName += baseGenericBegin + $"{typeName1}" + baseGenericEnd + ",";
                    }
                }
                typeName = typeName.TrimEnd(',') + ">";
            }
            else 
            {
                typeName = type.ToString();
                var typecode = Type.GetTypeCode(type);
                if (typecode == TypeCode.Object)
                {
                    typeName = normalBegin + typeName.Replace("+", ".") + normalEnd;
                }
                else
                {
                    typeName = baseBegin + typeName.Replace("+", ".") + baseEnd;
                }
            }
            return typeName;
        }
    }
}