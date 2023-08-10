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

    /// <summary>
    /// 序列化适配器
    /// </summary>
    public interface ISerializeAdapter : IAdapter
    {
        /// <summary>
        /// 是否加密传输?
        /// </summary>
        bool IsEncrypt { get; set; }
        /// <summary>
        /// 加密密码
        /// </summary>
        int Password { get; set; }

        byte[] OnSerializeRpc(RPCModel model);

        FuncData OnDeserializeRpc(byte[] buffer, int index, int count);

        byte[] OnSerializeOpt(OperationList list);

        OperationList OnDeserializeOpt(byte[] buffer, int index, int count);
    }

    /// <summary>
    /// 客户端RPC适配器
    /// </summary>
    public interface IRPCAdapter : IAdapter
    {
        void AddRpcHandle(object target, bool append, Action<SyncVarInfo> onSyncVarCollect);

        void OnRpcExecute(RPCModel model);

        void RemoveRpc(object target);
        RPCMethodBody OnRpcTaskRegister(ushort methodHash, string callbackFunc);
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
