using System;

namespace Net.Share
{
    public interface IThreadArgs
    {
        void Invoke();
    }

    public readonly struct ThreadSpan : IThreadArgs
    {
        public readonly Action action;

        public ThreadSpan(Action action)
        {
            this.action = action;
        }

        public void Invoke()
        {
            action?.Invoke();
        }
    }

    public readonly struct ThreadArgs : IThreadArgs
    {
        public readonly Action<object> action;
        public readonly object arg;

        public ThreadArgs(Action<object> action, object arg)
        {
            this.action = action;
            this.arg = arg;
        }

        public void Invoke()
        {
            action?.Invoke(arg);
        }
    }

    public readonly struct ThreadArgsGeneric<T> : IThreadArgs
    {
        public readonly Action<T> callback;
        public readonly T arg;

        public ThreadArgsGeneric(Action<T> callback, T arg)
        {
            this.callback = callback;
            this.arg = arg;
        }

        public void Invoke()
        {
            callback?.Invoke(arg);
        }
    }

    public readonly struct ThreadArgsGeneric<T, T1> : IThreadArgs
    {
        public readonly Action<T, T1> callback;
        public readonly T arg1;
        public readonly T1 arg2;

        public ThreadArgsGeneric(Action<T, T1> callback, T arg1, T1 arg2)
        {
            this.callback = callback;
            this.arg1 = arg1;
            this.arg2 = arg2;
        }

        public void Invoke()
        {
            callback?.Invoke(arg1, arg2);
        }
    }

    public readonly struct RPCModelThreadArgs : IThreadArgs
    {
        public readonly RPCModelEvent callback;
        public readonly RPCModel arg1;

        public RPCModelThreadArgs(RPCModelEvent callback, RPCModel arg1)
        {
            this.callback = callback;
            this.arg1 = arg1;
        }

        public void Invoke()
        {
            callback?.Invoke(arg1);
        }
    }

    public readonly struct OperationSyncThreadArgs : IThreadArgs
    {
        public readonly OnOperationSyncEvent callback;
        public readonly OperationList arg1;

        public OperationSyncThreadArgs(OnOperationSyncEvent callback, in OperationList arg1)
        {
            this.callback = callback;
            this.arg1 = arg1;
        }

        public void Invoke()
        {
            callback?.Invoke(arg1);
        }
    }
}