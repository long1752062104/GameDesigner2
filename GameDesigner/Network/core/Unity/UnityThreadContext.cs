using System;
using System.Collections.Generic;
using System.Threading;
using Net.System;
using UnityEngine.LowLevel;

namespace Net.Unity
{
    readonly struct UnityThreadContextData
    {
        public readonly Action<object> action;
        public readonly object arg;

        public UnityThreadContextData(Action<object> action, object arg)
        {
            this.action = action;
            this.arg = arg;
        }

        internal void Invoke()
        {
            action?.Invoke(arg);
        }
    }

    /// <summary>
    /// Unity主线程中心, 当多线程在主线程调用时可以使用Call或Invoke方法
    /// </summary>
    public class UnityThreadContext
    {
        private static int mainThreadId;
        /// <summary>
        /// unity主线程id
        /// </summary>
        public static int MainThreadId => mainThreadId;
        private static UnityThreadContextLoopRunner runner;

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Initialize()
        {
            mainThreadId = Thread.CurrentThread.ManagedThreadId;
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            runner = new UnityThreadContextLoopRunner();
            var runnerLoop = new PlayerLoopSystem
            {
                type = runner.GetType(),
                updateDelegate = runner.Run
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
        /// <param name="arg"></param>
        public static void Call(Action<object> action, object arg = null) => Invoke(action, arg);

        /// <summary>
        /// 在Unity主线程调用
        /// </summary>
        /// <param name="action"></param>
        /// <param name="arg"></param>
        public static void Invoke(Action<object> action, object arg = null)
        {
            if (Thread.CurrentThread.ManagedThreadId == mainThreadId)
                action(arg);
            else
                runner.WorkQueue.Enqueue(new UnityThreadContextData(action, arg));
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
            runner.WorkQueue.Enqueue(new UnityThreadContextData((arg) =>
            {
                try { result = ((Func<T>)arg).Invoke(); }
                finally { complete = true; }
            }, ptr));
            while (!complete)
                Thread.Sleep(1);
            return result;
        }
    }

    class UnityThreadContextLoopRunner
    {
        internal QueueSafe<UnityThreadContextData> WorkQueue = new QueueSafe<UnityThreadContextData>();

        internal void Run()
        {
            while (WorkQueue.TryDequeue(out var contextData))
            {
                contextData.Invoke();
            }
        }
    }
}