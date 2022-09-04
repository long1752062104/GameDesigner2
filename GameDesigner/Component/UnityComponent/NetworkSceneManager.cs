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

        public virtual void OnConnected()
        {
            NetworkObject.Init(5000);
        }

        public virtual void Update() 
        {
            if (NetworkTime.CanSent) 
            {
                foreach (var identity in identitys.Values)
                {
                    if (identity == null)
                        continue;
                    if (!identity.enabled)
                        continue;
                    if (identity.isDispose)
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

        private void OnNetworkOperSync(Operation opt)
        {
            switch (opt.cmd) 
            {
                case Command.Transform:
                    OnBuildOrTransformSync(opt);
                    break;
                case Command.BuildComponent:
                    OnBuildOrTransformSync(opt);
                    break;
                case Command.Destroy:
                    OnNetworkObjectDestroy(opt);
                    break;
                case Command.OnPlayerExit:
                    OnPlayerExit(opt);
                    break;
                case NetCmd.SyncVar:
                    OnSyncVarHandler(opt);
                    break;
                default:
                    OnOtherOperator(opt);
                    break;
            }
        }

        /// <summary>
        /// 当检查网络标识物体，如果不存在就会实例化
        /// </summary>
        /// <param name="opt"></param>
        /// <returns></returns>
        public virtual NetworkObject OnCheckIdentity(Operation opt) 
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
        public virtual void OnBuildOrTransformSync(Operation opt) 
        {
            var identity = OnCheckIdentity(opt);
            if (identity == null)
                return;
            if (identity.isDispose)
                return;
            var nb = identity.networkBehaviours[opt.index1];
            nb.OnNetworkOperationHandler(opt);
        }

        public virtual void OnSyncVarHandler(Operation opt) 
        {
            var identity = OnCheckIdentity(opt);
            if (identity == null)
                return;
            if (identity.isDispose)
                return;
            identity.SyncVarHandler(opt);
        }

        /// <summary>
        /// 当其他网络物体被删除(入口1)
        /// </summary>
        /// <param name="opt"></param>
        public virtual void OnNetworkObjectDestroy(Operation opt) 
        {
            if (identitys.TryGetValue(opt.identity, out NetworkObject identity))
            {
                OnPlayerDestroy(identity, false);
            }
        }

        public virtual void OnPlayerExit(Operation opt)
        {
            if (identitys.TryGetValue(opt.identity, out NetworkObject identity))//删除退出游戏的玩家游戏物体
                OnPlayerDestroy(identity, true);
            if (onExitDelectAll)//删除此玩家所创建的所有游戏物体
            {
                var uid = 10000 + ((opt.identity + 1 - 10000) * NetworkObject.Capacity);
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
            if (identity.isDispose)
                return;
            identity.isDispose = true;
            if(isPlayer)
                OnOtherExit(identity);
            OnOtherDestroy(identity);
        }

        public void RemoveIdentity(int identity)
        {
            identitys.Remove(identity);
        }

        /// <summary>
        /// 当其他网络物体被创建(实例化)
        /// </summary>
        /// <param name="opt"></param>
        /// <param name="identity"></param>
        public virtual void OnNetworkObjectCreate(Operation opt, NetworkObject identity)
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