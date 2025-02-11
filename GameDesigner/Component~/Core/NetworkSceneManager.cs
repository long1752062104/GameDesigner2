﻿#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using System;
using System.Collections.Generic;
using Net.Client;
using Net.Share;
using Net.System;
using Net.Component;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Net.UnityComponent
{
    [DefaultExecutionOrder(1)]
    public class NetworkSceneManager : SingleCase<NetworkSceneManager>
    {
        public List<NetworkObject> registerObjects = new List<NetworkObject>();
        [HideInInspector]
        public MyDictionary<int, NetworkObject> identitys = new MyDictionary<int, NetworkObject>();
        [Tooltip("如果onExitDelectAll=true 当客户端退出游戏,客户端所创建的所有网络物体也将随之被删除? onExitDelectAll=false只删除玩家物体")]
        public bool onExitDelectAll = true;
        protected ClientBase client; //当多场景时, 退出战斗场景, 回主场景时, 先进入主场景再卸载战斗场景, 而ClientBase.Instance被赋值到其他多连接客户端对象上就会出现OnDestry时没有正确移除OnOperationSync事件
        protected Queue<Action> waitNetworkIdentityQueue = new Queue<Action>();
        protected MyDictionary<byte, HashSet<OnOperationEvent>> operationHandlerDict = new MyDictionary<byte, HashSet<OnOperationEvent>>();
        protected FastList<Operation> operations = new FastList<Operation>();

        public virtual void Start()
        {
            _ = WaitConnecting();
            if (NetworkTime.Instance == null)
                gameObject.AddComponent<NetworkTime>();
        }

        public virtual async UniTaskVoid WaitConnecting()
        {
            var outTime = DateTime.Now.AddSeconds(10);
            while (DateTime.Now < outTime)
            {
                if (ClientBase.Instance == null)
                    await UniTask.Yield();
                else if (!ClientBase.Instance.Connected)
                    await UniTask.Yield();
                else
                    break;
            }
            if (DateTime.Now > outTime)
            {
                Debug.Log("连接超时!");
                return;
            }
            OnConnected();
            client = ClientBase.Instance;
            client.OnOperationSync += OperationSyncHandler;
            while (waitNetworkIdentityQueue.Count > 0)
                waitNetworkIdentityQueue.Dequeue()?.Invoke();
            AddOperation(new Operation(Command.OnPlayerEnter, client.UID));
        }

        public virtual void OnConnected()
        {
            NetworkObject.Init(5000);
        }

        /// <summary>
        /// 等待网络标识初始化, 当标识初始化完成调用onInitComplete委托
        /// </summary>
        /// <param name="onInitComplete"></param>
        public virtual void WaitNetworkIdentityInit(Action onInitComplete)
        {
            if (NetworkObject.IsInitIdentity)
                onInitComplete?.Invoke();
            else
                waitNetworkIdentityQueue.Enqueue(onInitComplete);
        }

        public virtual void Update()
        {
            var isNetworkTick = NetworkTime.CanSent;
            for (int i = 0; i < identitys.count; i++)
            {
                if (identitys.entries[i].hashCode == -1)
                    continue;
                var identity = identitys.entries[i].value;
                if (identity == null)
                    continue;
                if (!identity.enabled)
                    continue;
                if (identity.IsDispose)
                    continue;
                identity.NetworkUpdate(isNetworkTick);
            }
            if (client != null && isNetworkTick)
            {
                client.AddOperations(operations);
                operations._size = 0;
            }
        }

        protected virtual void OperationSyncHandler(in OperationList list)
        {
            foreach (var opt in list.operations)
                OnNetworkOperSync(opt);
        }

        protected virtual void OnNetworkOperSync(in Operation opt)
        {
            switch (opt.cmd)
            {
                case Command.Transform:
                    OnBuildOrTransformSync(opt);
                    break;
                case Command.NetworkComponent:
                    OnBuildOrTransformSync(opt);
                    break;
                case Command.Destroy:
                    OnNetworkObjectDestroy(opt);
                    break;
                case Command.OnPlayerEnter:
                    OnPlayerEnter(opt);
                    break;
                case Command.OnPlayerExit:
                    OnPlayerExit(opt);
                    break;
                case NetCmd.SyncVarNetObj:
                    OnSyncVarHandler(opt);
                    break;
                case NetCmd.CallRpc:
                    var segment = BufferPool.NewSegment(opt.buffer, 0, opt.buffer.Length, false);
                    var data = new RPCModel();
                    client.OnDeserializeRPC(segment, data);
                    client.DispatchRpc(data.protocol, data.pars);
                    break;
                default:
                    if (operationHandlerDict.TryGetValue(opt.cmd, out var operList))
                    {
                        foreach (var operAction in operList)
                            operAction.Invoke(opt);
                        break;
                    }
                    OnOtherOperator(opt);
                    break;
            }
        }

        /// <summary>
        /// 注册帧数据处理回调
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="operCall"></param>
        public void RegisterOperationCommand(byte cmd, OnOperationEvent operCall)
        {
            if (!operationHandlerDict.TryGetValue(cmd, out var operList))
                operationHandlerDict.Add(cmd, operList = new HashSet<OnOperationEvent>());
            operList.Add(operCall);
        }

        /// <summary>
        /// 移除帧数据处理回调
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="operCall"></param>
        public void RemoveOperationCommand(byte cmd, OnOperationEvent operCall)
        {
            if (!operationHandlerDict.TryGetValue(cmd, out var operList))
                operationHandlerDict.Add(cmd, operList = new HashSet<OnOperationEvent>());
            operList.Remove(operCall);
        }

        /// <summary>
        /// 当检查网络标识物体，如果不存在就会实例化 --- 在这里用到了<see cref="Operation.identity"/>作为网络物体标识, <see cref="Operation.index"/>作为要实例化<see cref="registerObjects"/>的物体索引
        /// </summary>
        /// <param name="opt"></param>
        /// <returns></returns>
        public virtual NetworkObject OnCheckIdentity(in Operation opt)
        {
            if (!identitys.TryGetValue(opt.identity, out NetworkObject identity))
            {
                if (opt.index >= registerObjects.Count)
                    return null;
                identity = Instantiate(registerObjects[opt.index]);
                identity.Identity = opt.identity;
                identity.isLocal = false;
                identity.isInit = true;
                identity.InitAll(opt);
                identitys.TryAdd(opt.identity, identity);
                OnNetworkObjectCreate(opt, identity);
            }
            return identity;
        }

        /// <summary>
        /// 当BuildComponent指令或Transform指令同步时调用
        /// </summary>
        /// <param name="opt"></param>
        public virtual void OnBuildOrTransformSync(in Operation opt)
        {
            var identity = OnCheckIdentity(opt);
            if (identity == null)
                return;
            if (identity.IsDispose)
                return;
            var nb = identity.networkBehaviours[opt.index1];
            if (nb == null)
                return;
            if (nb.isInitSync)
            {
                nb.isInitSync = false;
                nb.OnInitialSynchronization(opt);
            }
            nb.OnNetworkOperationHandler(opt);
        }

        public virtual void OnSyncVarHandler(in Operation opt)
        {
            var identity = OnCheckIdentity(opt);
            if (identity == null)
                return;
            if (identity.IsDispose)
                return;
            identity.SyncVarHandler(opt);
        }

        /// <summary>
        /// 当其他网络物体被删除(入口1)
        /// </summary>
        /// <param name="opt"></param>
        public virtual void OnNetworkObjectDestroy(in Operation opt)
        {
            //如果在本地执行移除id，在网络延迟时，会偶尔出现本机的物体被删除后，操作同步才到达，导致又重新实例化网络物体，这时候id栈已经压入当前网络物体的id，导致id冲突提示实例化两次的问题
            if (identitys.TryRemove(opt.identity, out var identity))
                OnPlayerDestroy(identity, false);
        }

        public virtual void OnPlayerEnter(in Operation opt)
        {
            if (opt.identity == client.UID) //这个命令如果是本机发起，则不进行处理，这里控制如果是公共网络物体时会出现拉回原位问题
                return;
            foreach (var netObj in identitys.Values)
            {
                if (!netObj.IsLocal)
                    continue;
                foreach (var syncVar in netObj.syncVarInfos.Values)
                    syncVar.SetDefaultValue();
                netObj.CheckSyncVar();
                for (int i = 0; i < netObj.networkBehaviours.Count; i++)
                    if (netObj.networkBehaviours[i] is NetworkTransformBase networkTransform)
                        networkTransform.ForcedSynchronous();
            }
        }

        public virtual void OnPlayerExit(in Operation opt)
        {
            if (identitys.TryRemove(opt.identity, out var identity))//删除退出游戏的玩家游戏物体
                OnPlayerDestroy(identity, true);
            if (onExitDelectAll)//删除此玩家所创建的所有游戏物体
            {
                var uid = NetworkObject.GetUserIdOffset(opt.identity);
                var count = uid + NetworkObject.Capacity;
                foreach (var item in identitys)
                    if (item.Key >= uid & item.Key < count)
                        OnPlayerDestroy(item.Value, false);
            }
        }

        private void OnPlayerDestroy(NetworkObject identity, bool isPlayer)
        {
            if (identity == null)
                return;
            if (identity.IsDispose)
                return;
            NetworkObject.PushIdentity(identity.Identity);
            if (isPlayer)
                OnOtherExit(identity);
            OnOtherDestroy(identity);
        }

        /// <summary>
        /// 当其他网络物体被创建(实例化)
        /// </summary>
        /// <param name="opt"></param>
        /// <param name="identity"></param>
        public virtual void OnNetworkObjectCreate(in Operation opt, NetworkObject identity)
        {
        }

        /// <summary>
        /// 当其他网络物体被删除(入口2)
        /// </summary>
        /// <param name="identity"></param>
        public virtual void OnOtherDestroy(NetworkObject identity)
        {
            Destroy(identity.gameObject);
        }

        /// <summary>
        /// 当其他玩家网络物体退出(删除)
        /// </summary>
        /// <param name="identity"></param>
        public virtual void OnOtherExit(NetworkObject identity)
        {
        }

        /// <summary>
        /// 当其他操作指令调用
        /// </summary>
        /// <param name="opt"></param>
        public virtual void OnOtherOperator(in Operation opt)
        {
        }

        private void OnApplicationQuit()
        {
            ExitSceneHandler();
        }

        /// <summary>
        /// 当退出场景时有些网络物体是不应该被销毁的
        /// </summary>
        public void ExitSceneHandler()
        {
            foreach (var identity in identitys)
            {
                identity.Value.Identity = -1;
            }
        }

        public virtual void OnDestroy()
        {
            NetworkObject.UnInit();//每次离开战斗场景都要清除初始化identity
            if (client == null)
                return;
            client.OnOperationSync -= OperationSyncHandler;
        }

        /// <summary>
        /// 添加场景同步操作
        /// </summary>
        /// <param name="operation"></param>
        public void AddOperation(in Operation operation)
        {
            operations.Add(operation);
        }
    }
}
#endif