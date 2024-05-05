using Net.Share;
using Net.System;
using System;

namespace Net.Helper
{
    public class JobQueueHelper
    {
        /// <summary>
        /// 跨线程调用任务队列
        /// </summary>
        public QueueSafe<IThreadArgs> WorkerQueue = new QueueSafe<IThreadArgs>();

        public void Call(IThreadArgs action)
        {
            WorkerQueue.Enqueue(action);
        }

        public void Call(Action action)
        {
            WorkerQueue.Enqueue(new ThreadSpan(action));
        }

        public void Call<T>(Action<T> action, T arg)
        {
            WorkerQueue.Enqueue(new ThreadArgsGeneric<T>(action, arg));
        }

        public void Call<T, T1>(Action<T, T1> action, T arg1, T1 arg2)
        {
            WorkerQueue.Enqueue(new ThreadArgsGeneric<T, T1>(action, arg1, arg2));
        }

        public void Call(RPCModelEvent action, RPCModel model)
        {
            WorkerQueue.Enqueue(new RPCModelThreadArgs(action, model));
        }

        public void Call(OnOperationSyncEvent action, in OperationList operList)
        {
            WorkerQueue.Enqueue(new OperationSyncThreadArgs(action, operList));
        }

        public void Execute()
        {
            int count = WorkerQueue.Count;
            for (int i = 0; i < count; i++)
                if (WorkerQueue.TryDequeue(out var callback))
                    callback.Invoke();
        }
    }
}
