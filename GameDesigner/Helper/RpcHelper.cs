﻿using Net.Server;
using Net.Share;
using Net.System;
using System;
using System.Collections.Generic;
using System.Reflection;
using WebSocketSharp;

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
                    RPCMethodBody body;
                    if (rpc.hash != 0)
                    {
                        if (!handle.RpcHashDic.TryGetValue(rpc.hash, out body))
                            handle.RpcHashDic.Add(rpc.hash, body = new RPCMethodBody());
                        body.Add(target, item);
                    }

                    var funcName = rpc.func.IsNullOrEmpty() ? item.method.Name : rpc.func;
                    if (!handle.RpcDic.TryGetValue(funcName, out body))
                        handle.RpcDic.Add(funcName, body = new RPCMethodBody());
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
                        if (handle.RpcHashDic.TryGetValue(item.rpc.hash, out var dict))
                        {
                            dict.Remove(target);
                        }
                        if (handle.RpcDic.TryGetValue(item.member.Name, out dict))
                        {
                            dict.Remove(target);
                        }
                    }
                    if (item.syncVar != null)
                    {
                        handle.SyncVarDic.Remove(item.syncVar.id);
                    }
                }
            }
        }

        public static void Invoke(IRpcHandler handle, NetPlayer client, RPCModel model, Action<MyDictionary<object, IRPCMethod>, NetPlayer, RPCModel> action, Action<int, NetPlayer, RPCModel> log)
        {
            RPCMethodBody body;
            if (model.methodHash != 0)
            {
                if (!handle.RpcHashDic.TryGetValue(model.methodHash, out body))
                {
                    log(0, client, model);
                    return;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(model.func))
                    return;
                if (!handle.RpcDic.TryGetValue(model.func, out body))
                {
                    log(1, client, model);
                    return;
                }
            }
            var timeout = (uint)Environment.TickCount;
            while (body.CallWaitQueue.TryDequeue(out RPCModelTask modelTask))
            {
                if (timeout > modelTask.timeout) //超时的等待队列忽略
                    continue;
                modelTask.model = model;
                modelTask.IsCompleted = true;
                if (modelTask.intercept)
                    return;
                break; //只能执行一次, 不能全部循环执行
            }
            if (body.Count <= 0)
            {
                log(2, client, model);
                return;
            }
            action(body.RpcDict, client, model);
        }
    }
}