using Net.Event;
using Net.Helper;
using System;
using System.Reflection;
using System.Text.RegularExpressions;

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

    public readonly struct RpcInvokeArgs : IThreadArgs
    {
        public string name => method.ToString();
        public readonly bool logRpc;
        public readonly object target;
        public readonly MethodInfo method;
        public readonly object[] pars;

        public RpcInvokeArgs(bool logRpc, object target, MethodInfo method, object[] pars)
        {
            this.logRpc = logRpc;
            this.target = target;
            this.method = method;
            this.pars = pars;
        }

        public void Invoke()
        {
            try
            {
                if (logRpc)
                {
                    if (!ScriptHelper.Cache.TryGetValue(target.GetType().FullName + "." + method.Name, out var sequence))
                        sequence = new SequencePoint();
                    NDebug.Log($"RPC:{method} () (at {sequence.FilePath}:{sequence.StartLine}) \n");
                }
                if (method == null)
                    return;
                method.Invoke(target, pars);
            }
            catch (Exception ex)
            {
#if UNITY_EDITOR
                if (!ScriptHelper.Cache.TryGetValue(target.GetType().FullName + "." + method.Name, out var sequence))
                    sequence = new SequencePoint();
                var info = $"{method.Name}方法内部发生错误!\n() (at {sequence.FilePath}:{sequence.StartLine}) \n";
                var reg = new Regex(@"\)\s\[0x[0-9,a-f]*\]\sin\s(.*:[0-9]*)\s");
                info += reg.Replace(ex.ToString(), ") (at $1) ");
                var dataPath = PathHelper.PlatformReplace(UnityEngine.Application.dataPath).Replace("Assets", "");
                info = PathHelper.PlatformReplace(info.Replace(dataPath, ""));
                NDebug.LogError(info);
#else
                NDebug.LogError($"{method.Name}方法内部发生错误! 详细信息:" + ex);
#endif
            }
        }

        public override string ToString()
        {
            return $"{target}->{name}";
        }
    }
}