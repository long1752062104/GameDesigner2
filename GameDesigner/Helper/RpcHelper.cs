using Net.Server;
using Net.Share;
using Net.System;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace Net.Helper
{
    /// <summary>
    /// 远程过程调用(RPC)帮助类
    /// </summary>
    public class RpcHelper
    {
        public static void AddRpc(IRpcHandler handle, object target, bool append, Action<SyncVarInfo> onSyncVarCollect)
        {
            AddRpc(handle, target, target.GetType(), append, onSyncVarCollect);
        }

        public static void AddRpc(IRpcHandler handle, object target, Type type, bool append, Action<SyncVarInfo> onSyncVarCollect)
        {
            AddRpc(handle, target, type, append, onSyncVarCollect, null, (member) => new RPCMethod(target, member.member as MethodInfo, member.rpc.cmd));
        }

        public static void AddRpc(IRpcHandler handle, object target, Type type, bool append, Action<SyncVarInfo> onSyncVarCollect, Action<MemberInfo, MemberData> action, Func<MemberData, IRPCMethod> func)
        {
            if (!append)
                if (handle.RpcTargetHash.ContainsKey(target))
                    return;
            if (!handle.MemberInfos.TryGetValue(type, out var list))
            {
                list = new List<MemberData>();
                var members = type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var info in members)
                {
                    var data = new MemberData() { member = info };
                    if (info.MemberType == MemberTypes.Method)
                    {
                        var attributes = info.GetCustomAttributes(typeof(RPC), true);//兼容ILR写法
                        if (attributes.Length > 0)
                        {
                            data.rpc = attributes[0] as RPC;
                            action?.Invoke(info, data);
                        }
                    }
                    else if (info.MemberType == MemberTypes.Field | info.MemberType == MemberTypes.Property)
                    {
                        SyncVarHelper.InitSyncVar(info, target, (syncVar) =>
                        {
                            data.syncVar = syncVar;
                        });
                    }
                    if (data.rpc != null | data.syncVar != null)
                        list.Add(data);
                }
                handle.MemberInfos.Add(type, list);
            }
            foreach (var member in list)
            {
                var rpc = member.rpc;
                if (rpc != null)
                {
                    var item = func(member);
                    if (string.IsNullOrEmpty(rpc.func))
                        rpc.func = item.method.Name;
                    if (!handle.RpcCollectDic.TryGetValue(rpc.func.CRCU32(), out var body))
                        handle.RpcCollectDic.Add(rpc.func.CRCU32(), body = new RPCMethodBody());
                    if (rpc.hash != 0)
                        if (!handle.RpcCollectDic.ContainsKey(rpc.hash))
                            handle.RpcCollectDic.Add(rpc.hash, body);
                    body.Add(target, item);
                }
                var syncVar = member.syncVar;
                if (syncVar != null)
                {
                    var syncVar1 = syncVar.Clone(target);
                    if (syncVar1.id == 0)
                        onSyncVarCollect?.Invoke(syncVar1);
                    else
                        handle.SyncVarDic.TryAdd(syncVar1.id, syncVar1);
                }
            }
            if (list.Count > 0)
                handle.RpcTargetHash.Add(target, new MemberDataList() { members = list });
        }

        public static void RemoveRpc(IRpcHandler handle, object target)
        {
            if (handle.RpcTargetHash.TryRemove(target, out var list))
            {
                foreach (var item in list.members)
                {
                    if (item.rpc != null)
                    {
                        if (handle.RpcCollectDic.TryGetValue(item.rpc.hash, out var dict))
                            dict.Remove(target);
                        if (handle.RpcCollectDic.TryGetValue(item.rpc.func.CRCU32(), out dict))
                            dict.Remove(target);
                    }
                    if (item.syncVar != null)
                        handle.SyncVarDic.Remove(item.syncVar.id);
                }
            }
        }

        public delegate void RpcInvokeDelegate(MyDictionary<object, IRPCMethod> rpcDict, NetPlayer client, RPCModel model);
        public delegate void RpcLogDelegate(int code, NetPlayer client, RPCModel model);

        public static void Invoke(IRpcHandler handle, NetPlayer client, RPCModel model, RpcInvokeDelegate action, RpcLogDelegate log)
        {
            if (!handle.RpcCollectDic.TryGetValue(model.protocol, out var body))
            {
                log(0, client, model);
                return;
            }
            if (model.token == 0 && body.RequestDict.Count > 0)
            {
                log(1, client, model);
                return;
            }
            if (model.token != 0)
            {
                if (body.RequestDict.TryRemove(model.token, out var modelTask))
                {
                    modelTask.model = model;
                    modelTask.IsCompleted = true;
                    var original = Interlocked.Exchange(ref modelTask.callback, null);
                    original?.Invoke();
                    if (modelTask.intercept)
                        return;
                }
                //如果超时了会执行到这里，因为开发者是以Request发起的，所以大概率不会再定义一个[Rpc]来二次调用! 如果你二次调用也是没有问题，但是没有二次调用时，我们不应该提示添加[Rpc]
                //如果你设置intercept=true并且[Rpc]方法存在才会往下执行，要不然就直接返回，也不需要提示添加[Rpc]警告，因为这个不是错误问题
                if (body.Count <= 0)
                    return;
            }
            else if (body.Count <= 0)
            {
                log(2, client, model);
                return;
            }
            action(body.RpcDict, client, model);
        }
    }
}