using Net.Helper;
using Net.System;
using System;
using System.Collections.Generic;

namespace Net.Share
{
    public class RPCMethodBody
    {
        public MyDictionary<object, IRPCMethod> RpcDict = new MyDictionary<object, IRPCMethod>();
        public SafeDictionary<uint, RPCModelTask> RequestDict = new SafeDictionary<uint, RPCModelTask>();
        public int Count => RpcDict.Count;

        internal void Add(object key, IRPCMethod value)
        {
            RpcDict.Add(key, value);
        }

        internal void Remove(object target)
        {
            RpcDict.Remove(target);
        }
    }

    /// <summary>
    /// 远程过程调用处理接口
    /// </summary>
    public interface IRpcHandler
    {
        /// <summary>
        /// 远程调用方法收集
        /// </summary>
        MyDictionary<uint, RPCMethodBody> RpcCollectDic { get; set; }
        /// <summary>
        /// 已经收集过的类信息
        /// </summary>
        MyDictionary<Type, List<MemberData>> MemberInfos { get; set; }
        /// <summary>
        /// 当前收集rpc的对象信息
        /// </summary>
        MyDictionary<object, MemberDataList> RpcTargetHash { get; set; }
        /// <summary>
        /// 字段同步信息
        /// </summary>
        MyDictionary<ushort, SyncVarInfo> SyncVarDic { get; set; }
        /// <summary>
        /// 跨线程调用任务队列
        /// </summary>
        JobQueueHelper WorkerQueue { get; set; }
        /// <summary>
        /// 添加Rpc
        /// </summary>
        /// <param name="target">注册的对象实例</param>
        /// <param name="append">一个Rpc方法是否可以多次添加到Rpcs里面？</param>
        /// <param name="onSyncVarCollect">字段同步收集回调</param>
        void AddRpc(object target, bool append = false, Action<SyncVarInfo> onSyncVarCollect = null);
        /// <summary>
        /// 移除target的所有rpc
        /// </summary>
        /// <param name="target"></param>
        void RemoveRpc(object target);
    }
}