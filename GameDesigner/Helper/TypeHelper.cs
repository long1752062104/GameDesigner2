using System;
using System.Collections.Generic;

namespace Net.Helper
{
    public static class TypeHelper
    {
        public static bool IsArrayOrList(this Type listType)
        {
            return listType.IsArray || listType.IsGenericType && listType.GetGenericTypeDefinition() == typeof(List<>);
        }

        public static Type GetArrayOrListElementType(this Type listType)
        {
            if (listType.IsArray)
                return listType.GetElementType();
            return listType.IsGenericType && listType.GetGenericTypeDefinition() == typeof(List<>) ? listType.GetGenericArguments()[0] : null;
        }

        public static Type GetArrayOrListOrGenericElementType(this Type listType)
        {
            if (listType.IsArray)
                return listType.GetElementType();
            if (listType.IsGenericType)
                return listType.GenericTypeArguments[0];
            return null;
        }

        public static Type[] GetInterfaceGenericTypeArguments(this Type type)
        {
            foreach (var interfaceType in type.GetInterfaces())
            {
                var genericTypeArguments = interfaceType.GetGenericArguments();
                if (genericTypeArguments == null)
                    continue;
                if (genericTypeArguments.Length == 0)
                    continue;
                return genericTypeArguments;
            }
            throw new Exception("获取接口失败!");
        }

        public static Type GetArrayItemType(this Type type)
        {
            return GetArrayOrListOrGenericElementType(type);
        }

        public static bool IsInterfaceType(this Type type, params Type[] interfaceTypes)
        {
            foreach (var interfaceType1 in type.GetInterfaces())
            {
                foreach (var interfaceType2 in interfaceTypes)
                {
                    if (interfaceType1 == interfaceType2)
                        return true;
                }
            }
            return false;
        }
    }
}