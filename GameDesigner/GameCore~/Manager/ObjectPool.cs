using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using UnityEngine;
using GameCore;

namespace GameCore
{
    public class PoolListCache
    {
        public virtual void Add(Component poolObj)
        {
            throw new NotImplementedException();
        }

        public virtual void Clear()
        {
            throw new NotImplementedException();
        }

        public virtual object GetList()
        {
            throw new NotImplementedException();
        }

        public virtual object GetIndex(int index)
        {
            throw new NotImplementedException();
        }
    }

    public class PoolListCache<T> : PoolListCache where T : Component
    {
        public List<T> List = new List<T>();

        public override void Add(Component poolObj)
        {
            List.Add(poolObj as T);
        }

        public override void Clear()
        {
            foreach (var component in List)
            {
                if (component == null)
                    continue;
                component.RecyclingObject();
            }
            List.Clear();
        }

        public override object GetList()
        {
            return List;
        }

        public override object GetIndex(int index)
        {
            return List[index];
        }
    }

    public class ObjectPool : MonoBehaviour
    {
        protected readonly Dictionary<Type, Queue<Object>> pool = new Dictionary<Type, Queue<Object>>();
        protected readonly Dictionary<Enum, PoolListCache> cacheList = new Dictionary<Enum, PoolListCache>();

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

        /// <summary>
        /// 获取对象池物体并且添加到缓存列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <param name="component"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public T GetObjectAddList<T>(Enum command, T component, Transform parent = null) where T : Component
        {
            var type = typeof(T);
            if (!pool.TryGetValue(type, out var queue))
                pool.Add(type, queue = new Queue<Object>());
            T poolObj;
            while (queue.Count > 0) //如果池内的物体被意外删除了, 就会被清除忽略掉
            {
                poolObj = queue.Dequeue() as T;
                if (poolObj != null)
                    goto J;
            }
            poolObj = Instantiate(component);
        J: poolObj.transform.SetParent(parent);
            if (!cacheList.TryGetValue(command, out var comList))
                cacheList.Add(command, comList = new PoolListCache<T>());
            comList.Add(poolObj);
            return poolObj;
        }

        /// <summary>
        /// 回收对象池缓存列表
        /// </summary>
        /// <param name="command"></param>
        public void RecyclingObjectList<T>(Enum command) where T : Component
        {
            if (!cacheList.TryGetValue(command, out var comList))
                cacheList.Add(command, comList = new PoolListCache<T>());
            comList.Clear();
        }

        public List<T> GetObjectList<T>(Enum command) where T : Component
        {
            if (!cacheList.TryGetValue(command, out var comList))
                cacheList.Add(command, comList = new PoolListCache<T>());
            return comList.GetList() as List<T>;
        }

        public T GetObjectListIndex<T>(Enum command, int index) where T : Component
        {
            if (!cacheList.TryGetValue(command, out var comList))
                cacheList.Add(command, comList = new PoolListCache<T>());
            return comList.GetIndex(index) as T;
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