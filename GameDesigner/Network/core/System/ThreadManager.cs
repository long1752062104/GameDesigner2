using Net.Event;
using System;
using System.Threading;

namespace Net.System
{
    /// <summary>
    /// 主线程管理中心
    /// </summary>
    public static class ThreadManager
    {
        private static Thread MainThread;
        public static TimerEvent Event { get; private set; } = new TimerEvent();
        /// <summary>
        /// 时间计数间隔
        /// </summary>
        public static uint Interval { get; set; } = 2;

        static ThreadManager()
        {
            Run();
        }

        private static void Run()
        {
#pragma warning disable IDE0017 // 简化对象初始化
            MainThread = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        Thread.Sleep(1);
                        Event.UpdateEvent(Interval);
                    }
                    catch (Exception ex)
                    {
                        NDebug.LogError("主线程异常:" + ex);
                    }
                }
            });
#pragma warning restore IDE0017 // 简化对象初始化
            MainThread.Name = "网络主线程";
            MainThread.IsBackground = true;
            MainThread.Priority = ThreadPriority.Highest;
            MainThread.Start();
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
    }
}
