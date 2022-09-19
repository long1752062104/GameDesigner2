#if !CLOSE_ILR
using ILRuntime.Runtime.Intepreter;
using Net.Helper;
using Net.Share;
using Net.System;
using System.Collections.Generic;
using System.Reflection;

namespace Net.Client
{
    public static class ClientBaseExtensions
    {
        /// <summary>
        /// 添加网络Rpc ILRuntime
        /// </summary>
        /// <param name="target">注册的对象实例</param>
        public static void Add_ILR_RpcHandle(this ClientBase self, object target)
        {
            Add_ILR_RpcHandle(self, target, false);
        }

        /// <summary>
        /// 添加网络Rpc ILRuntime
        /// </summary>
        /// <param name="target">注册的对象实例</param>
        /// <param name="append">一个Rpc方法是否可以多次添加到Rpcs里面？</param>
        public static void Add_ILR_RpcHandle(this ClientBase self, object target, bool append)
        {
            lock (self)
            {
                var ilInstace = target as ILTypeInstance;
                var type = ilInstace.Type.ReflectionType;
                RpcHelper.AddRpc(self, target, type, append, null);
            }
        }
    }
}
#endif