﻿using Net.System;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Net.Share
{
    public interface IRpcHandler
    {
        /// <summary>
        /// 远程方法优化字典
        /// </summary>
        public MyDictionary<string, MyDictionary<long, IRPCMethod>> RpcDic { get; set; }
        /// <summary>
        /// 远程方法哈希字典
        /// </summary>
        public MyDictionary<ushort, MyDictionary<long, IRPCMethod>> RpcHashDic { get; set; }
        /// <summary>
        /// 已经收集过的类信息
        /// </summary>
        public MyDictionary<Type, List<MemberData>> MemberInfos { get; set; }
        /// <summary>
        /// 当前收集rpc的对象信息
        /// </summary>
        public MyDictionary<long, MemberDataList> RpcTargetHash { get; set; }
        /// <summary>
        /// 字段同步信息
        /// </summary>
        public MyDictionary<ushort, SyncVarInfo> SyncVarDic { get; set; }
        /// <summary>
        /// 收集rpc的对象唯一id
        /// </summary>
        public ObjectIDGenerator IDGenerator { get; set; }
        /// <summary>
        /// 等待回调的异步Rpc
        /// </summary>
        public ConcurrentDictionary<string, RPCModelTask> RpcTasks { get; set; }
        /// <summary>
        /// 等待回调的异步Rpc
        /// </summary>
        public ConcurrentDictionary<ushort, RPCModelTask> RpcTasks1 { get; set; }
        /// <summary>
        /// Rpc任务队列
        /// </summary>
        public QueueSafe<IRPCData> RpcWorkQueue { get; set; }
        /// <summary>
        /// 移除target的所有rpc
        /// </summary>
        /// <param name="target"></param>
        void RemoveRpc(object target);
        /// <summary>
        /// 检查rpc对象或方法是否已被销毁（释放）
        /// </summary>
        void CheckRpc();
    }
}
