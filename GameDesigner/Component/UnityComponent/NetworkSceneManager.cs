#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA
namespace Net.UnityComponent
{
    using global::System;
    using global::System.Collections.Generic;
    using global::System.Threading.Tasks;
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

        public virtual void OnConnected()
        {
            NetworkObject.Init(5000);
        }

        // Start is called before the first frame update
        public virtual void Start()
        {
            WaitConnecting();
        }

        public virtual async void WaitConnecting()
        {
            var outTime = DateTime.Now.AddSeconds(10);
            while (DateTime.Now < outTime)
            {
                if (ClientBase.Instance == null)
                    await Task.Yield();
                if (!ClientBase.Instance.Connected)
                    await Task.Yield();
                else
                    break;
            }
            if (DateTime.Now > outTime)
            {
                Debug.Log("连接超时!");
                return;
            }
            OnConnected();
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
                    OnNetworkObjectDestroy(opt);
                    break;
                case Command.OnPlayerExit:
                    OnPlayerExit(opt);
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

        public virtual void OnNetworkObjectDestroy(Operation opt) 
        {
            if (identitys.TryGetValue(opt.identity, out NetworkObject identity))
            {
                identitys.Remove(opt.identity);
                OnOtherDestroy(identity);
            }
        }

        public virtual void OnPlayerExit(Operation opt)
        {
            if (onExitDelectAll)
            {
                var uid = 10000 + ((opt.identity + 1 - 10000) * NetworkObject.Capacity);
                var count = uid + NetworkObject.Capacity;
                foreach (var item in identitys)
                {
                    if (item.Key >= uid & item.Key < count)
                    {
                        identitys.Remove(item.Key);
                        OnOtherExit(item.Value);
                        OnOtherDestroy(item.Value);
                    }
                }
            }
            else
            {
                if (identitys.TryGetValue(opt.identity, out NetworkObject identity))
                {
                    identitys.Remove(opt.identity);
                    OnOtherExit(identity);
                    OnOtherDestroy(identity);
                }
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
            ClientBase.Instance.OnConnectedHandle -= OnConnected;
            ClientBase.Instance.OnOperationSync -= OperationSync;
        }
    }
}
#endif