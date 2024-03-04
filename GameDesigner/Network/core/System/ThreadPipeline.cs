using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Net.Event;
using Net.Share;

namespace Net.System
{
    /// <summary>
    /// 线程管线(流水线)
    /// </summary>
    /// <typeparam name="Worker"></typeparam>
    public class ThreadPipeline<Worker> : IDisposable //where Worker : class
    {
        /// <summary>
        /// 并发线程数量, 发送线程和接收处理线程数量
        /// </summary>
        public int MaxThread { get; set; } = Environment.ProcessorCount * 2;
        /// <summary>
        /// 是否工作中?
        /// </summary>
        public bool IsWorking { get; private set; }
        /// <summary>
        /// 线程组, 优化多线程资源竞争问题
        /// </summary>
        public List<ThreadGroup<Worker>> Groups { get; private set; } = new List<ThreadGroup<Worker>>();
        /// <summary>
        /// 当线程群处理
        /// </summary>
        public Action<ThreadGroup<Worker>> OnProcess;

        private int threadCount;

        /// <summary>
        /// 初始化线程管线
        /// </summary>
        /// <param name="name"></param>
        public void Init(string name)
        {
            for (int i = 0; i < MaxThread; i++)
            {
                var group = new ThreadGroup<Worker>()
                {
                    Id = i + 1
                };
                var thread = new Thread(Processing)
                {
                    IsBackground = true,
                    Name = name + i
                };
                group.Thread = thread;
                Groups.Add(group);
            }
        }

        /// <summary>
        /// 开始线程管线
        /// </summary>
        public void Start()
        {
            IsWorking = true;
            for (int i = 0; i < Groups.Count; i++)
            {
                Groups[i].Thread.Start(Groups[i]);
            }
        }

        /// <summary>
        /// 选择线程群
        /// </summary>
        /// <returns></returns>
        public ThreadGroup<Worker> SelectGroup()
        {
            var value = threadCount++;
            return Groups[value % Groups.Count];
        }

        /// <summary>
        /// 添加工人
        /// </summary>
        /// <param name="worker"></param>
        public void AddWorker(Worker worker)
        {
            SelectGroup().Add(worker);
        }

        /// <summary>
        /// 移除工人
        /// </summary>
        /// <param name="worker"></param>
        public void RemoveWorker(Worker worker)
        {
            for (int i = 0; i < Groups.Count; i++) //只能一个一个寻找移除
            {
                Groups[i].Remove(worker);
            }
        }

        /// <summary>
        /// 业务处理线程组
        /// </summary>
        /// <param name="state"></param>
        private void Processing(object state)
        {
            var group = state as ThreadGroup<Worker>;
            var stopwatch = Stopwatch.StartNew();
            while (IsWorking)
            {
                try
                {
                    stopwatch.Restart();
                    OnProcess(group);
                    stopwatch.Stop();
                    if (stopwatch.ElapsedMilliseconds < 1)
                        Thread.Sleep(1);
                }
                catch (Exception ex)
                {
                    NDebug.LogError(ex.ToString());
                }
            }
        }

        public void Dispose()
        {
            IsWorking = false;
            for (int i = 0; i < Groups.Count; i++)
            {
                Groups[i].Thread.Interrupt();
            }
            Groups.Clear();
        }
    }
}
