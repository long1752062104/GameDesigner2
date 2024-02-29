﻿using System;
using System.Collections.Generic;
using Net.Share;
using Net.Event;
using Net.Common;
using Net.Helper;
using Cysharp.Threading.Tasks;

namespace Net.System
{
    /// <summary>
    /// 数据缓存基类
    /// </summary>
    public class DataCacheBase
    {
        internal object key;
        public readonly object queryLock = new object();
        /// <summary>
        /// 是否查询过了, 比如123的账号, 查询一次mysql没有查询到, 第二次就不需要进行查询了, 再次查询只会占用IO, 这里可以设置查询间隔, 比如123查询不到, 就给个60秒的过期时间, 这样可以解决IO问题
        /// </summary>
        public virtual bool IsQuery { get; set; }
        /// <summary>
        /// 缓存过期时间 (毫秒单位), 默认是-1, 也就是无过期时间, 这是考虑到当new一个缓存对象时, 这时候多线程检查时直接把这个对象当成过期时间处理导致问题
        /// </summary>
        public virtual long ExpirationTime { get; set; } = -1;
        /// <summary>
        /// 数据有必要时锁
        /// </summary>
        public readonly FastLocking Locking = new FastLocking();
    }

    /// <summary>
    /// 数据缓存类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DataCache<T> : DataCacheBase
    {
        /// <summary>
        /// 缓存数据对象
        /// </summary>
        public virtual T Data { get; set; }

        /// <summary>
        /// 查询缓存对象或获取数据对象, 如果对象存在就直接获取, 如果对象不存在则需要查询对象进行缓存并获取
        /// </summary>
        /// <param name="queryFunc">第一次执行查询</param>
        /// <param name="followQuery">跟随查询, 比如查询user表成功后, 还需要查询item表, friend表, 其他关联的表等等</param>
        /// <returns></returns>
        public T QueryOrGet(Func<T> queryFunc, Action<T> followQuery = null)
        {
            if (Data != null)
            {
                ExpirationTime = DateTimeHelper.GetTickCount64() + DataCacheManager.Instance.CacheTimeout;
                return Data;
            }
            if (IsQuery)
            {
                ExpirationTime = DateTimeHelper.GetTickCount64() + DataCacheManager.Instance.NullQueryTimeout;
                return default;
            }
            IsQuery = true;
            Data = queryFunc();
            if (Data == null)
            {
                ExpirationTime = DateTimeHelper.GetTickCount64() + DataCacheManager.Instance.NullQueryTimeout;
                return default;
            }
            followQuery?.Invoke(Data);
            ExpirationTime = DateTimeHelper.GetTickCount64() + DataCacheManager.Instance.CacheTimeout;
            return Data;
        }

        /// <summary>
        /// 查询缓存对象或获取数据对象, 如果对象存在就直接获取, 如果对象不存在则需要查询对象进行缓存并获取
        /// </summary>
        /// <param name="queryFunc">第一次执行查询</param>
        /// <param name="followQuery">跟随查询, 比如查询user表成功后, 还需要查询item表, friend表, 其他关联的表等等</param>
        /// <returns></returns>
        public async UniTask<T> QueryOrGetAsync(Func<UniTask<T>> queryFunc, Action<T> followQuery = null)
        {
            if (Data != null)
            {
                ExpirationTime = DateTimeHelper.GetTickCount64() + DataCacheManager.Instance.CacheTimeout;
                return Data;
            }
            if (IsQuery)
            {
                ExpirationTime = DateTimeHelper.GetTickCount64() + DataCacheManager.Instance.NullQueryTimeout;
                return default;
            }
            IsQuery = true;
            Data = await queryFunc();
            if (Data == null)
            {
                ExpirationTime = DateTimeHelper.GetTickCount64() + DataCacheManager.Instance.NullQueryTimeout;
                return default;
            }
            followQuery?.Invoke(Data);
            ExpirationTime = DateTimeHelper.GetTickCount64() + DataCacheManager.Instance.CacheTimeout;
            return Data;
        }
    }

    public interface IDataCacheDictionary
    {
        IEnumerable<DataCacheBase> CacheValues { get; }
        void RemoveCache(DataCacheBase cache);
    }

    /// <summary>
    /// 数据缓存字典
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class DataCacheDictionary<TKey, TValue> : MyDictionary<TKey, TValue>, IDataCacheDictionary, IDisposable where TValue : DataCacheBase
    {
        IEnumerable<DataCacheBase> IDataCacheDictionary.CacheValues { get => Values; }

        public DataCacheDictionary() : this(1) { }

        public DataCacheDictionary(int capacity) : base(capacity)
        {
            DataCacheManager.Instance.AddCache(this);
        }

        protected override bool Insert(TKey key, TValue value, bool hasKeyThrow, out TValue oldValue)
        {
            lock (this)
            {
                value.key = key;
                return base.Insert(key, value, hasKeyThrow, out oldValue);
            }
        }

        public override bool TryRemove(TKey key, out TValue value)
        {
            lock (this)
            {
                return base.TryRemove(key, out value);
            }
        }

        public virtual void RemoveCache(DataCacheBase dataCache)
        {
            if (dataCache.key is TKey key)
                Remove(key);
        }

        public void Dispose()
        {
            DataCacheManager.Instance.RemoveCache(this);
        }
    }

    /// <summary>
    /// 数据缓存管理器类
    /// </summary>
    public class DataCacheManager : Singleton<DataCacheManager>
    {
        public FastList<IDataCacheDictionary> cacheDictionaries = new FastList<IDataCacheDictionary>();
        public int CacheTimeout = 1000 * 60 * 60 * 12; //缓存12个小时
        public int NullQueryTimeout { get; set; } = 1000 * 60; //空查询60秒恢复

        private readonly object _lock = new object();

        public void AddCache(IDataCacheDictionary cacheDictionary)
        {
            lock (_lock)
            {
                cacheDictionaries.Add(cacheDictionary);
            }
        }

        public void RemoveCache(int index)
        {
            lock (_lock)
            {
                cacheDictionaries.RemoveAt(index);
            }
        }

        public void RemoveCache(IDataCacheDictionary cacheDictionary)
        {
            lock (_lock)
            {
                cacheDictionaries.Remove(cacheDictionary);
            }
        }

        /// <summary>
        /// 检查缓存字段过期对象
        /// </summary>
        /// <returns></returns>
        public bool Executed()
        {
            try
            {
                var timeout = DateTimeHelper.GetTickCount64();
                for (int i = 0; i < cacheDictionaries.Count; i++)
                {
                    var cacheDictionary = cacheDictionaries[i];
                    if (cacheDictionary == null)
                    {
                        RemoveCache(i);
                        continue;
                    }
                    foreach (var cache in cacheDictionary.CacheValues)
                    {
                        if (cache == null)
                            continue;
                        if (cache.ExpirationTime == -1)
                            continue;
                        if (timeout >= cache.ExpirationTime)
                            cacheDictionary.RemoveCache(cache);
                    }
                }
            }
            catch (Exception ex)
            {
                NDebug.LogError(ex);
            }
            return true;
        }
    }
}
