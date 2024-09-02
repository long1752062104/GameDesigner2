using System;
using System.Collections.Generic;

namespace Net.Helper
{
    public static class TypeHelper
    {
        public static bool IsArrayOrList(this Type listType)
        {
            return listType.IsArray || listType.IsGenericType && listType.GetGenericTypeDefinition() == typeof (List<>);
        }

        public static Type GetArrayOrListElementType(this Type listType)
        {
            if (listType.IsArray)
                return listType.GetElementType();
            return listType.IsGenericType && listType.GetGenericTypeDefinition() == typeof (List<>) ? listType.GetGenericArguments()[0] : (Type) null;
        }
    }
}