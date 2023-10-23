using System;

namespace Net.Entities
{
    public class ObjectName 
    {
        public string Name { get; set; }
    }

    public class Object
    {
        internal ObjectName m_Name;
        public string name
        {
            get => m_Name.Name;
            set => m_Name.Name = value;
        }

        public static Object Instantiate(Object original, Vector3 position, Quaternion rotation)
        {
            throw new NotImplementedException();
        }

        public static Object Instantiate(Object original, Vector3 position, Quaternion rotation, Transform parent)
        {
            throw new NotImplementedException();
        }

        public static Object Instantiate(Object original)
        {
            throw new NotImplementedException();
        }

        public static Object Instantiate(Object original, Transform parent)
        {
            return Object.Instantiate(original, parent, false);
        }

        public static Object Instantiate(Object original, Transform parent, bool instantiateInWorldSpace)
        {
            throw new NotImplementedException();
        }

        public static T Instantiate<T>(T original) where T : Object
        {
            throw new NotImplementedException();
        }

        public static T Instantiate<T>(T original, Vector3 position, Quaternion rotation) where T : Object
        {
            return (T)((object)Object.Instantiate(original, position, rotation));
        }

        public static T Instantiate<T>(T original, Vector3 position, Quaternion rotation, Transform parent) where T : Object
        {
            return (T)((object)Object.Instantiate(original, position, rotation, parent));
        }

        public static T Instantiate<T>(T original, Transform parent) where T : Object
        {
            return Object.Instantiate<T>(original, parent, false);
        }

        public static T Instantiate<T>(T original, Transform parent, bool worldPositionStays) where T : Object
        {
            return (T)((object)Object.Instantiate(original, parent, worldPositionStays));
        }

        public static void Destroy(Object obj, [DefaultValue("0.0F")] float t) 
        {
            throw new NotImplementedException();
        }

        public static void Destroy(Object obj)
        {
            float t = 0f;
            Object.Destroy(obj, t);
        }

        public static void DestroyImmediate(Object obj, [DefaultValue("false")] bool allowDestroyingAssets) 
        {
            throw new NotImplementedException();
        }

        public static void DestroyImmediate(Object obj)
        {
            bool allowDestroyingAssets = false;
            Object.DestroyImmediate(obj, allowDestroyingAssets);
        }

        public static Object[] FindObjectsOfType(Type type)
        {
            return Object.FindObjectsOfType(type, false);
        }

        public static Object[] FindObjectsOfType(Type type, bool includeInactive) 
        {
            throw new NotImplementedException();
        }

        public static Object[] FindObjectsByType(Type type, FindObjectsSortMode sortMode)
        {
            return Object.FindObjectsByType(type, FindObjectsInactive.Exclude, sortMode);
        }

        public static Object[] FindObjectsByType(Type type, FindObjectsInactive findObjectsInactive, FindObjectsSortMode sortMode) 
        {
            throw new NotImplementedException();
        }

        public static void DontDestroyOnLoad([NotNull("NullExceptionObject")] Object target) 
        {
            throw new NotImplementedException();
        }

        public static void DestroyObject(Object obj, [DefaultValue("0.0F")] float t)
        {
            Object.Destroy(obj, t);
        }

        public static void DestroyObject(Object obj)
        {
            float t = 0f;
            Object.Destroy(obj, t);
        }

        public static Object[] FindSceneObjectsOfType(Type type)
        {
            return Object.FindObjectsOfType(type);
        }

        public static Object[] FindObjectsOfTypeIncludingAssets(Type type) 
        {
            throw new NotImplementedException();
        }

        public static T[] FindObjectsOfType<T>() where T : Object
        {
            return Resources.ConvertObjects<T>(Object.FindObjectsOfType(typeof(T), false));
        }

        public static T[] FindObjectsByType<T>(FindObjectsSortMode sortMode) where T : Object
        {
            return Resources.ConvertObjects<T>(Object.FindObjectsByType(typeof(T), FindObjectsInactive.Exclude, sortMode));
        }

        public static T[] FindObjectsOfType<T>(bool includeInactive) where T : Object
        {
            return Resources.ConvertObjects<T>(Object.FindObjectsOfType(typeof(T), includeInactive));
        }

        public static T[] FindObjectsByType<T>(FindObjectsInactive findObjectsInactive, FindObjectsSortMode sortMode) where T : Object
        {
            return Resources.ConvertObjects<T>(Object.FindObjectsByType(typeof(T), findObjectsInactive, sortMode));
        }

        public static T FindObjectOfType<T>() where T : Object
        {
            return (T)((object)Object.FindObjectOfType(typeof(T), false));
        }

        public static T FindObjectOfType<T>(bool includeInactive) where T : Object
        {
            return (T)((object)Object.FindObjectOfType(typeof(T), includeInactive));
        }

        public static T FindFirstObjectByType<T>() where T : Object
        {
            return (T)((object)Object.FindFirstObjectByType(typeof(T), FindObjectsInactive.Exclude));
        }

        public static T FindAnyObjectByType<T>() where T : Object
        {
            return (T)((object)Object.FindAnyObjectByType(typeof(T), FindObjectsInactive.Exclude));
        }

        public static T FindFirstObjectByType<T>(FindObjectsInactive findObjectsInactive) where T : Object
        {
            return (T)((object)Object.FindFirstObjectByType(typeof(T), findObjectsInactive));
        }

        public static T FindAnyObjectByType<T>(FindObjectsInactive findObjectsInactive) where T : Object
        {
            return (T)((object)Object.FindAnyObjectByType(typeof(T), findObjectsInactive));
        }

        public static Object[] FindObjectsOfTypeAll(Type type)
        {
            return Resources.FindObjectsOfTypeAll(type);
        }

        public static Object FindObjectOfType(Type type)
        {
            Object[] array = Object.FindObjectsOfType(type, false);
            bool flag = array.Length != 0;
            Object result;
            if (flag)
            {
                result = array[0];
            }
            else
            {
                result = null;
            }
            return result;
        }

        public static Object FindFirstObjectByType(Type type)
        {
            Object[] array = Object.FindObjectsByType(type, FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID);
            return (array.Length != 0) ? array[0] : null;
        }

        public static Object FindAnyObjectByType(Type type)
        {
            Object[] array = Object.FindObjectsByType(type, FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            return (array.Length != 0) ? array[0] : null;
        }

        public static Object FindObjectOfType(Type type, bool includeInactive)
        {
            Object[] array = Object.FindObjectsOfType(type, includeInactive);
            bool flag = array.Length != 0;
            Object result;
            if (flag)
            {
                result = array[0];
            }
            else
            {
                result = null;
            }
            return result;
        }

        public static Object FindFirstObjectByType(Type type, FindObjectsInactive findObjectsInactive)
        {
            Object[] array = Object.FindObjectsByType(type, findObjectsInactive, FindObjectsSortMode.InstanceID);
            return (array.Length != 0) ? array[0] : null;
        }

        public static Object FindAnyObjectByType(Type type, FindObjectsInactive findObjectsInactive)
        {
            Object[] array = Object.FindObjectsByType(type, findObjectsInactive, FindObjectsSortMode.None);
            return (array.Length != 0) ? array[0] : null;
        }

        public override string ToString()
        {
            return $"{name} ({GetType()})";
        }
    }
}