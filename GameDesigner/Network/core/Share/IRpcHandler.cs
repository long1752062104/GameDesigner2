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
        /// Rpc任务队列
        /// </summary>
        QueueSafe<IRPCData> RpcWorkQueue { get; set; }
        /// <summary>
        /// 移除target的所有rpc
        /// </summary>
        /// <param name="target"></param>
        void RemoveRpc(object target);
    }
}