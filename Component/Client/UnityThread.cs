﻿#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA
using System;
using System.Threading.Tasks;
using Net.System;
using UnityEngine;

namespace Net.Component
{
    /// <summary>
    /// 提供对unity主线程的访问
    /// </summary>
    [ExecuteInEditMode]
    public class UnityThread : SingleCase<UnityThread>
    {
        public static QueueSafe<Action> WorkerQueue = new QueueSafe<Action>();

        void Update()
        {
            int count = WorkerQueue.Count;
            for (int i = 0; i < count; i++)
            {
                if (WorkerQueue.TryDequeue(out var callback))
                {
                    callback();
                }
            }
        }

        public static async Task<T> Call<T>(Func<T> func)
        {
            var isComplete = false;
            T t = default;
            WorkerQueue.Enqueue(()=> {
                t = func();
                isComplete = true;
            });
            while (!isComplete)
            {
                await Task.Yield();
            }
            return t;
        }

        public static async Task Call(Action action)
        {
            var isComplete = false;
            WorkerQueue.Enqueue(() => {
                action();
                isComplete = true;
            });
            while (!isComplete)
            {
                await Task.Yield();
            }
        }
    }
}
#endif