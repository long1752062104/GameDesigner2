#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA
namespace Net.UnityComponent
{
    using global::System.Collections.Generic;
    using Net.Client;
    using Net.Component;
    using Net.Share;
    using Net.System;
    using UnityEngine;

    [RequireComponent(typeof(NetworkTime))]
    public class NetworkSceneManager : SingleCase<NetworkSceneManager>
    {
        public List<NetworkObject> registerObjects = new List<NetworkObject>();
        [HideInInspector]
        public MyDictionary<int, NetworkObject> identitys = new MyDictionary<int, NetworkObject>();
        [Tooltip("如果onExitDelectAll=true 当客户端退出游戏,客户端所创建的所有网络物体也将随之被删除? onExitDelectAll=false只删除玩家物体")]
        public bool onExitDelectAll = true;

        public virtual void OnConnectedHandle()
        {
            NetworkObject.Init(5000);
        }

        // Start is called before the first frame update
        public virtual void Start()
        {
            if (ClientBase.Instance == null)
                return;
            if (ClientBase.Instance.Connected)
                OnConnectedHandle();
            else
                ClientBase.Instance.OnConnectedHandle += OnConnectedHandle;
            ClientBase.Instance.OnOperationSync += OperationSync;
        }

        public virtual void Update() 
        {
            if (NetworkTime.CanSent) 
            {
                foreach (var identity in identitys.Values)
                {
                    if (!identity.enabled)
                        continue;
                    identity.CheckSyncVar();
                    identity.PropertyAutoCheckHandler();
                }
            }
        }

        private void OperationSync(OperationList list)
        {
            foreach (var opt in list.operations)
                OnNetworkOperSync(opt);
        }

        void OnNetworkOperSync(Operation opt)
        {
            switch (opt.cmd) 
            {
                case Command.Transform:
                    {
                        if (!identitys.TryGetValue(opt.identity, out NetworkObject identity))
                        {
                            identity = Instantiate(registerObjects[opt.index]);
                            identity.Identity = opt.identity;
                            identity.isOtherCreate = true;
                            identity.isInit = true;
                            identitys.Add(opt.identity, identity);
                            OnNetworkObjectCreate(opt, identity);
                            foreach (var item in identity.networkBehaviours)
                                item.OnNetworkObjectCreate(opt);
                        }
                        var nb = identity.networkBehaviours[opt.index1];
                        nb.OnNetworkOperationHandler(opt);
                    }
                    break;
                case Command.BuildComponent:
                    {
                        if (!identitys.TryGetValue(opt.identity, out NetworkObject identity))
                        {
                            identity = Instantiate(registerObjects[opt.index]);
                            identity.Identity = opt.identity;
                            identity.isOtherCreate = true;
                            identity.isInit = true;
                            identitys.Add(opt.identity, identity);
                            foreach (var item in identity.networkBehaviours)
                                item.OnNetworkObjectCreate(opt);
                        }
                        var nb = identity.networkBehaviours[opt.index1];
                        nb.OnNetworkOperationHandler(opt);
                    }
                    break;
                case Command.Destroy:
                    {
                        if (identitys.TryGetValue(opt.identity, out NetworkObject identity))
                        {
                            OnOtherDestroy(identity);
                        }
                    }
                    break;
                case Command.OnPlayerExit:
                    {
                        if (onExitDelectAll)
                        {
                            var uid = 10000 + ((opt.uid + 1 - 10000) * NetworkObject.Capacity);
                            var count = uid + NetworkObject.Capacity;
                            foreach (var item in identitys)
                            {
                                if (item.Key >= uid & item.Key < count)
                                {
                                    OnOtherDestroy(item.Value);
                                }
                            }
                        }
                        else 
                        {
                            if (identitys.TryGetValue(opt.uid, out NetworkObject identity))
                            {
                                OnOtherExit(identity);
                                OnOtherDestroy(identity);
                            }
                        }
                    }
                    break;
                case NetCmd.SyncVar:
                    {
                        if (!identitys.TryGetValue(opt.identity, out NetworkObject identity))
                        {
                            identity = Instantiate(registerObjects[opt.index]);
                            identity.Identity = opt.identity;
                            identity.isOtherCreate = true;
                            identity.isInit = true;
                            identitys.Add(opt.identity, identity);
                            foreach (var item in identity.networkBehaviours)
                                item.OnNetworkObjectCreate(opt);
                        }
                        identity.SyncVarHandler(opt);
                    }
                    break;
                default:
                    OnOtherOperator(opt);
                    break;
            }
        }

        public virtual void OnNetworkObjectCreate(Operation opt, NetworkObject identity)
        {
        }

        public virtual void OnOtherDestroy(NetworkObject identity)
        {
            Destroy(identity.gameObject);
        }

        public virtual void OnOtherExit(NetworkObject identity)
        {
        }

        public virtual void OnOtherOperator(Operation opt)
        {
        }

        public virtual void OnDestroy()
        {
            if (ClientBase.Instance == null)
                return;
            ClientBase.Instance.OnConnectedHandle -= OnConnectedHandle;
            ClientBase.Instance.OnOperationSync -= OperationSync;
        }
    }
}
#endif