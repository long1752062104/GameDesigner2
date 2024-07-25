using System;
using Net.System;
using Net.Server;
using Net.Serialize;

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
    public delegate bool DeserializeRpcDelegate(ISegment segment, RPCModel model);
    public delegate byte[] SerializeOptDelegate(in OperationList list);

    /// <summary>
    /// 序列化适配器
    /// </summary>
    public interface ISerializeAdapter : IAdapter
    {
        /// <summary>
        /// 当序列化远程过程调用模型
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        bool OnSerializeRpc(ISegment segment, RPCModel model);

        /// <summary>
        /// 当反序列化远程过程调用模型
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        bool OnDeserializeRpc(ISegment segment, RPCModel model);

        /// <summary>
        /// 当序列化帧同步操作列表
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        byte[] OnSerializeOpt(in OperationList list);

        /// <summary>
        /// 当反序列化帧同步操作列表
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        OperationList OnDeserializeOpt(ISegment segment);
    }

    /// <summary>
    /// 客户端RPC适配器
    /// </summary>
    public interface IRPCAdapter : IAdapter
    {
        /// <summary>
        /// 添加远程过程调用事件
        /// </summary>
        /// <param name="target"></param>
        /// <param name="append"></param>
        /// <param name="onSyncVarCollect"></param>
        void AddRpc(object target, bool append, Action<SyncVarInfo> onSyncVarCollect);

        /// <summary>
        /// 当执行远程过程调用
        /// </summary>
        /// <param name="model"></param>
        void OnRpcExecute(RPCModel model);

        /// <summary>
        /// 移除远程过程调用事件
        /// </summary>
        /// <param name="target"></param>
        void RemoveRpc(object target);
    }

    /// <summary>
    /// 服务器RPC适配器
    /// </summary>
    /// <typeparam name="Player"></typeparam>
    public interface IRPCAdapter<Player> : IAdapter where Player : NetPlayer
    {
        /// <summary>
        /// 添加远程过程调用事件
        /// </summary>
        /// <param name="target"></param>
        /// <param name="append"></param>
        /// <param name="onSyncVarCollect"></param>
        void AddRpc(object target, bool append, Action<SyncVarInfo> onSyncVarCollect);

        /// <summary>
        /// 当执行远程过程调用
        /// </summary>
        /// <param name="client"></param>
        /// <param name="model"></param>
        void OnRpcExecute(Player client, RPCModel model);

        /// <summary>
        /// 移除远程过程调用事件
        /// </summary>
        /// <param name="target"></param>
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
