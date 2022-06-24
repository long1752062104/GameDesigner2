using Net.Event;
using System;
using System.Diagnostics;
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
        /// <summary>
        /// 运行中?
        /// </summary>
        public static bool IsRuning { get; set; }

        static ThreadManager()
        {
            Init();
            Start();
        }

        private static async void Init()
        {
            IsRuning = true;
#if UNITY_EDITOR
            while (UnityEditor.EditorApplication.isPlaying)
            {
                await global::System.Threading.Tasks.Task.Yield();
            }
            IsRuning = false;
#endif
        }

        private static void Start()
        {
            MainThread = new Thread(Execute);
            MainThread.Name = "网络主线程";
            MainThread.IsBackground = true;
            MainThread.Start();
        }

        public static void Run()
        {
            if (MainThread != null) 
            {
                MainThread.Abort();
                MainThread = null;
            }
            Execute();
        }

        public static void Execute()
        {
            int frame = 0;//一秒60次
            var nextTime = DateTime.Now.AddSeconds(1);
            var stopwatch = Stopwatch.StartNew();
            while (IsRuning)
            {
                try
                {
                    var frameRate = 1000 / Interval;
                    var now = DateTime.Now;
                    if (now >= nextTime)
                    {
                        if (frame < frameRate)
                        {
                            var step = (frameRate - frame) * Interval;
                            Event.UpdateEvent((uint)step);
                        }
                        nextTime = DateTime.Now.AddSeconds(1);
                        frame = 0;
                        stopwatch.Restart();
                    }
                    else if (frame < frameRate & stopwatch.ElapsedMilliseconds >= frame * Interval)
                    {
                        Event.UpdateEvent(Interval);
                        frame++;
                    }
                    else if ((nextTime - now).TotalSeconds > 60d)//当系统时间被改很长后问题
                    {
                        nextTime = DateTime.Now.AddSeconds(1);
                    }
                    else
                    {
                        Thread.Sleep(1);
                    }
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
