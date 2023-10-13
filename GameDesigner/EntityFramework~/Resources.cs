using System;
using System.Collections.Generic;

namespace Net.EntityFramework
{
    public sealed class Resources
    {
        internal static T[] ConvertObjects<T>(Object[] rawObjects) where T : Object
        {
            bool flag = rawObjects == null;
            T[] result;
            if (flag)
            {
                result = null;
            }
            else
            {
                T[] array = new T[rawObjects.Length];
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = (T)((object)rawObjects[i]);
                }
                result = array;
            }
            return result;
        }

        public static Object[] FindObjectsOfTypeAll(Type type)
        {
            throw new NotImplementedException();
        }

        public static T[] FindObjectsOfTypeAll<T>() where T : Object
        {
            return Resources.ConvertObjects<T>(Resources.FindObjectsOfTypeAll(typeof(T)));
        }

        public static Object Load(string path)
        {
            return Resources.Load(path, typeof(Object));
        }

        public static T Load<T>(string path) where T : Object
        {
            return (T)((object)Resources.Load(path, typeof(T)));
        }

        public static Object Load(string path, Type systemTypeInstance)
        {
            throw new NotImplementedException();
        }

        public static Object[] LoadAll(string path, Type systemTypeInstance)
        {
            throw new NotImplementedException();
        }

        public static Object[] LoadAll(string path)
        {
            return Resources.LoadAll(path, typeof(Object));
        }

        public static T[] LoadAll<T>(string path) where T : Object
        {
            return Resources.ConvertObjects<T>(Resources.LoadAll(path, typeof(T)));
        }

        public static Object GetBuiltinResource([NotNull("ArgumentNullException")] Type type, string path)
        {
            throw new NotImplementedException();
        }

        public static T GetBuiltinResource<T>(string path) where T : Object
        {
            return (T)((object)Resources.GetBuiltinResource(typeof(T), path));
        }

        public static void UnloadAsset(Object assetToUnload)
        {
            throw new NotImplementedException();
        }

        public static Object InstanceIDToObject(int instanceID)
        {
            throw new NotImplementedException();
        }

        public static Object LoadAssetAtPath(string assetPath, Type type)
        {
            return null;
        }

        public static T LoadAssetAtPath<T>(string assetPath) where T : Object
        {
            return default(T);
        }
    }
}