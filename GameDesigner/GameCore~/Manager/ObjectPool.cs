using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using UnityEngine;
using GameCore;

namespace GameCore
{
    public class ObjectPool : MonoBehaviour
    {
        protected readonly Dictionary<Type, Queue<Object>> pool = new Dictionary<Type, Queue<Object>>();

        public T GetObject<T>(string resPath) where T : Object
        {
            var type = typeof(T);
            if (!pool.TryGetValue(type, out var queue))
                pool.Add(type, queue = new Queue<Object>());
            if (queue.Count > 0)
                return queue.Dequeue() as T;
            var resObj = Global.Resources.LoadAsset<T>(resPath);
            return Instantiate(resObj);
        }

        public T GetObject<T>(Object ifCreatObject, Transform ifCreatObjectParent = null) where T : Object
        {
            var type = typeof(T);
            if (!pool.TryGetValue(type, out var queue))
                pool.Add(type, queue = new Queue<Object>());
            T poolObj;
            while (queue.Count > 0) //如果池内的物体被意外删除了, 就会被清除忽略掉
            {
                poolObj = queue.Dequeue() as T;
                if (poolObj != null)
                    return poolObj;
            }
            poolObj = Instantiate(ifCreatObject as T, ifCreatObjectParent);
            return poolObj;
        }

        public void Recycling(Object obj)
        {
            var type = obj.GetType();
            if (!pool.TryGetValue(type, out var queue))
                pool.Add(type, queue = new Queue<Object>());
            queue.Enqueue(obj);
        }
    }
}

public static class ObjectPoolExt
{
    public static void RecyclingObjects<T>(this List<T> self) where T : Object
    {
        for (int i = 0; i < self.Count; i++)
            self[i]?.RecyclingObject();
        self.Clear();
    }

    public static void RecyclingObject<T>(this T self) where T : Object
    {
        Global.Pool.Recycling(self);
        if (self is GameObject go)
            go.SetActive(false);
        else if (self is Component com)
            com.gameObject.SetActive(false);
    }

    public static void RecyclingObjects<T>(this T[] self) where T : Object
    {
        for (int i = 0; i < self.Length; i++)
            self[i]?.RecyclingObject();
    }

    public static void RecyclingObjects<Key, Value>(this Dictionary<Key, Value> self) where Value : Object
    {
        foreach (var item in self)
            item.Value?.RecyclingObject();
        self.Clear();
    }
}