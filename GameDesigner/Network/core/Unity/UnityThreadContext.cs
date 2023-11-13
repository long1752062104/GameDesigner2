using System;
using System.Collections.Generic;
using System.Threading;
using Net.Helper;
using UnityEngine.LowLevel;

namespace Net.Unity
{
    /// <summary>
    /// Unity主线程中心, 当多线程在主线程调用时可以使用Call或Invoke方法
    /// </summary>
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    public class UnityThreadContext
    {
        private static int mainThreadId;
        /// <summary>
        /// unity主线程id
        /// </summary>
        public static int MainThreadId => mainThreadId;
        private static JobQueueHelper runner;

        static UnityThreadContext()
        {
            mainThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Initialize()
        {
            mainThreadId = Thread.CurrentThread.ManagedThreadId;
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            runner = new JobQueueHelper();
            var runnerLoop = new PlayerLoopSystem
            {
                type = runner.GetType(),
                updateDelegate = runner.Execute
            };
            var copyList = new List<PlayerLoopSystem>(playerLoop.subSystemList)
            {
                runnerLoop
            };
            playerLoop.subSystemList = copyList.ToArray();
            PlayerLoop.SetPlayerLoop(playerLoop);
        }

        /// <summary>
        /// 在Unity主线程调用
        /// </summary>
        /// <param name="action"></param>
        public static void Call(Action action)
        {
            if (Thread.CurrentThread.ManagedThreadId == mainThreadId)
                action();
            else
                runner.Call(action);
        }

        /// <summary>
        /// 在Unity主线程调用
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="arg"></param>
        public static void Call<T>(Action<T> action, T arg)
        {
            if (Thread.CurrentThread.ManagedThreadId == mainThreadId)
                action(arg);
            else
                runner.Call(action, arg);
        }

        /// <summary>
        /// 在Unity主线程调用
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="action"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        public static void Call<T, T1>(Action<T, T1> action, T arg1, T1 arg2)
        {
            if (Thread.CurrentThread.ManagedThreadId == mainThreadId)
                action(arg1, arg2);
            else
                runner.Call(action, arg1, arg2);
        }

        /// <summary>
        /// 在主线程调用, 并返回结果到此线程, 如果此线程是主线程, 则直接调用并返回
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ptr"></param>
        /// <returns></returns>
        public static T Get<T>(Func<T> ptr)
        {
            if (Thread.CurrentThread.ManagedThreadId == mainThreadId)
                return ptr();
            var complete = false;
            T result = default;
            runner.Call((arg) =>
            {
                try { result = arg.Invoke(); }
                finally { complete = true; }
            }, ptr);
            while (!complete)
                Thread.Sleep(1);
            return result;
        }
    }
}