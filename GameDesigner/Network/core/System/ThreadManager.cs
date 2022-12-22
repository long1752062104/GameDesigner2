using Net.Event;
using System;
using System.Threading;
#if UNITY_WEBGL
using UnityEngine.LowLevel;
using System.Collections.Generic;
#endif

namespace Net.System
{
    /// <summary>
    /// 主线程管理中心
    /// </summary>
    public static class ThreadManager
    {
#if !UNITY_WEBGL
        private static Thread MainThread;
#endif
        public static TimerEvent Event { get; private set; } = new TimerEvent();
        /// <summary>
        /// 时间计数间隔
        /// </summary>
        public static uint Interval { get; set; } = 1;
        /// <summary>
        /// 运行中?
        /// </summary>
        public static bool IsRuning { get; set; }

        static ThreadManager()
        {
            Init();
            Start();
        }

        private static void Init()
        {
            IsRuning = true;
        }

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialize()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += (state) =>
            {
                switch (state)
                {
                    case UnityEditor.PlayModeStateChange.EnteredPlayMode:
                        IsRuning = true;
                        break;
                    case UnityEditor.PlayModeStateChange.ExitingPlayMode:
                        IsRuning = false;
                        break;
                }
            };
#endif
#if UNITY_WEBGL
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            var runner = new PlayerLoopRunner();
            var runnerLoop = new PlayerLoopSystem
            {
                type = typeof(PlayerLoopRunner),
                updateDelegate = runner.Run
            };
            var copyList = new List<PlayerLoopSystem>(playerLoop.subSystemList)
            {
                runnerLoop
            };
            playerLoop.subSystemList = copyList.ToArray();
            PlayerLoop.SetPlayerLoop(playerLoop);
#endif
        }

        private static void Start()
        {
#if !UNITY_WEBGL
            MainThread = new Thread(Execute);
            MainThread.Name = "网络主线程";
            MainThread.IsBackground = true;
            MainThread.Start();
#endif
        }

        /// <summary>
        /// 控制台死循环线程
        /// </summary>
        public static void Run()
        {
            Stop();
            Execute();
        }

        /// <summary>
        /// unity update每帧调用
        /// </summary>
        public static void Run(uint interval)
        {
            Stop();
            Event.UpdateEventFixed(interval, false);
        }

        public static void Stop()
        {
#if !UNITY_WEBGL
            if (MainThread != null)
            {
                MainThread.Abort();
                MainThread = null;
            }
#endif
        }

        public static void Execute()
        {
            while (IsRuning)
            {
                try
                {
                    Event.UpdateEventFixed(Interval, true);
                }
                catch (ThreadAbortException ex)
                {
                    NDebug.LogWarning("主线程:" + ex.Message);
                }
                catch (Exception ex)
                {
                    NDebug.LogError("主线程异常:" + ex);
                }
            }
        }

        public static int Invoke(Func<bool> ptr, bool isAsync = false) 
        {
            return Event.AddEvent(0, ptr, isAsync);
        }

        public static int Invoke(Action ptr, bool isAsync = false)
        {
            return Event.AddEvent(0, ptr, isAsync);
        }

        public static int Invoke(string name, Func<bool> ptr, bool isAsync = false)
        {
            return Event.AddEvent(name, 0, ptr, isAsync);
        }

        public static int Invoke(float time, Func<bool> ptr, bool isAsync = false)
        {
            return Event.AddEvent(time, ptr, isAsync);
        }

        public static int Invoke(string name, float time, Func<bool> ptr, bool isAsync = false)
        {
            return Event.AddEvent(name, time, ptr, isAsync);
        }

        public static int Invoke(string name, int time, Func<bool> ptr, bool isAsync = false)
        {
            return Event.AddEvent(name, time, ptr, isAsync);
        }
    }
}
