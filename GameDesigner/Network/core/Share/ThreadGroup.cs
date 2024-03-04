using System;
using System.Threading;
using Net.System;

namespace Net.Share
{
    public class ThreadGroup
    {
        public int Id;
        public Thread Thread;
        public int FPS;

        public virtual void Add(object client)
        {
        }

        public virtual void Remove(object client)
        {
        }

        public override string ToString()
        {
            return $"线程组ID:{Id} 线程ID:{Thread.ManagedThreadId}";
        }
    }

    public class ThreadGroup<Worker> : ThreadGroup //where Worker : class
    {
        public FastListSafe<Worker> Workers = new FastListSafe<Worker>();

        public override void Add(object worker)
        {
            Workers.Add((Worker)worker);
        }

        public override void Remove(object worker)
        {
            Workers.Remove((Worker)worker);
        }

        public override string ToString()
        {
            return $"线程组ID:{Id} 线程ID:{Thread.ManagedThreadId} 工人数量:{Workers.Count}";
        }
    }
}
