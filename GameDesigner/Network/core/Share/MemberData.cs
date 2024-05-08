using Net.Adapter;
using Net.Event;
using Net.Helper;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Net.Share
{
    public class MemberData
    {
        public MemberInfo member;
        public RPC rpc;
        public SyncVarInfo syncVar;
        public RPCPTR ptr;

        public override string ToString()
        {
            return $"{member}";
        }
    }

    public class MemberDataList
    {
        public List<MemberData> members = new List<MemberData>();
    }

    public struct RPCMethodPtr : IRPCMethod
    {
        public string Name => method.ToString();
        public byte cmd { get; set; }
        public object target { get; set; }
        public MethodInfo method { get; set; }
        public RPCPTR ptr;

        public RPCMethodPtr(object target, MethodInfo method, byte cmd, RPCPTR ptr) : this()
        {
            this.target = target;
            this.cmd = cmd;
            this.ptr = ptr;
            this.method = method;
        }

        public void Invoke()
        {
            ptr.Invoke(target, null);
        }

        public void Invoke(params object[] pars)
        {
            ptr.Invoke(target, pars);
        }
    }

    public readonly struct RpcInvokePtr : IThreadArgs
    {
        /// <summary>
        /// 函数和参数的名称
        /// </summary>
        public string name => method.ToString();
        public readonly bool logRpc;
        /// <summary>
        /// 存储封包反序列化出来的对象
        /// </summary>
        public readonly object target;
        /// <summary>
        /// 存储反序列化的函数
        /// </summary>
        public readonly MethodInfo method;
        public readonly RPCPTR ptr;
        /// <summary>
        /// 存储反序列化参数
        /// </summary>
        public readonly object[] pars;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="logRpc"></param>
        /// <param name="target">远程调用对象</param>
        /// <param name="method">远程调用方法</param>
        /// <param name="ptr"></param>
        /// <param name="pars">远程调用参数</param>
        public RpcInvokePtr(bool logRpc, object target, MethodInfo method, RPCPTR ptr, params object[] pars)
        {
            this.logRpc = logRpc;
            this.target = target;
            this.method = method;
            this.pars = pars;
            this.ptr = ptr;
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <returns></returns>
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
                if (ptr == null)
                    return;
                ptr.Invoke(target, pars);
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