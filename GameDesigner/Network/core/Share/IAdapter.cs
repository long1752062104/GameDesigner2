﻿using Net.Serialize;
using Net.Server;
using Net.System;
using System;

namespace Net.Share
{
    public enum AdapterType
    {
        Serialize,
        RPC,
        NetworkEvt,
        Package,
    }

    /// <summary>
    /// 基础适配器接口
    /// </summary>
    public interface IAdapter
    {
    }

    public delegate bool SerializeRpcDelegate(ISegment segment, RPCModel model);
    public delegate byte[] SerializeOptDelegate(in OperationList list);

    /// <summary>
    /// 序列化适配器
    /// </summary>
    public interface ISerializeAdapter : IAdapter
    {
        bool OnSerializeRpc(ISegment segment, RPCModel model);

        FuncData OnDeserializeRpc(ISegment segment);

        byte[] OnSerializeOpt(in OperationList list);

        OperationList OnDeserializeOpt(ISegment segment);
    }

    /// <summary>
    /// 客户端RPC适配器
    /// </summary>
    public interface IRPCAdapter : IAdapter
    {
        void AddRpcHandle(object target, bool append, Action<SyncVarInfo> onSyncVarCollect);

        void OnRpcExecute(RPCModel model);

        void RemoveRpc(object target);
        RPCMethodBody OnRpcTaskRegister(uint callback);
    }

    /// <summary>
    /// 服务器RPC适配器
    /// </summary>
    /// <typeparam name="Player"></typeparam>
    public interface IRPCAdapter<Player> : IAdapter where Player : NetPlayer
    {
        void AddRpcHandle(object target, bool append, Action<SyncVarInfo> onSyncVarCollect);

        void OnRpcExecute(Player client, RPCModel model);

        void RemoveRpc(object target);
    }

    /// <summary>
    /// 网络事件适配器
    /// </summary>
    public interface INetworkEvtAdapter : INetworkHandle, IAdapter
    {
    }

    /// <summary>
    /// 数据包适配器
    /// </summary>
    public interface IPackageAdapter : IAdapter
    {
        /// <summary>
        /// 头部长度
        /// </summary>
        int HeadCount { get; set; }
        /// <summary>
        /// 封包
        /// </summary>
        /// <param name="stream"></param>
        void Pack(ISegment stream);
        /// <summary>
        /// 解包
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="frame"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        bool Unpack(ISegment stream, int frame, int uid);
    }
}
