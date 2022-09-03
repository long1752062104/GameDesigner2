#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA
namespace Net.UnityComponent
{
    using global::System.Collections.Generic;
    using Net.Client;
    using Net.Component;
    using Net.Helper;
    using Net.Share;
    using UnityEngine;

    /// <summary>
    /// 网络物体标识组件
    /// </summary>
    [DisallowMultipleComponent]
    public class NetworkObject : MonoBehaviour
    {
        public static bool IsInit { get; private set; }
        internal static int IDENTITY { get; private set; } = -1;
        internal static int IDENTITY_MAX { get; private set; }
        internal static Queue<int> IDENTITY_POOL = new Queue<int>();
        public static int Capacity { get; private set; }
        [SerializeField] [DisplayOnly] private int m_identity = -1;
        [Tooltip("自定义唯一标识, 当值不为0后,可以不通过NetworkSceneManager的registerObjects去设置, 直接放在设计的场景里面, 不需要做成预制体")]
        [SerializeField] private int identity;//可以设置的id
        [Tooltip("注册的网络物体索引, registerObjectIndex要对应NetworkSceneManager的registerObjects数组索引, 如果设置了自定义唯一标识, 则此字段无效!")]
        public int registerObjectIndex;
        internal bool isOtherCreate;
        internal List<NetworkBehaviour> networkBehaviours = new List<NetworkBehaviour>();
        internal List<SyncVarInfo> syncVarInfos = new List<SyncVarInfo>();
        internal bool isInit;
        internal bool isDispose;

        /// <summary>
        /// 每个网络对象的唯一标识
        /// </summary>
        public int Identity
        {
            get{ return m_identity; }
            set { m_identity = value; }
        }
        public virtual void Start()
        {
            Init();
        }
        public void Init()
        {
            if (isInit)
                return;
            isInit = true;
            if (IDENTITY == -1)
            {
                Debug.LogError("网络标识未初始化，请调用NetworkObject.Init(5000);初始化");
                Destroy(gameObject);
                return;
            }
            var sm = NetworkSceneManager.I;
            if (sm == null)
            {
                Debug.Log("没有找到NetworkSceneManager组件！NetworkIdentity组件无效！");
                Destroy(gameObject);
                return;
            }
            if (isOtherCreate | m_identity > 0)
            {
                goto J1;
            }
            if (identity > 0)
            {
                m_identity = identity;
                goto J1;
            }
            if (IDENTITY_POOL.Count > 0)
            {
                m_identity = IDENTITY_POOL.Dequeue();
                goto J1;
            }
            if (IDENTITY < IDENTITY_MAX)
            {
                m_identity = IDENTITY++;
                goto J1;
            }
            else
            {
                Debug.LogError("网络标识已用完! 如果有需要请加大网络标识数量NetworkObject.Init(10000);");
                Destroy(gameObject);
                return;
            }
        J1:
            if (!sm.identitys.TryAdd(m_identity, this, out var netObj))
            {
                if (netObj == this | netObj == null)
                    return;
                netObj.m_identity = -1;
                Debug.Log($"uid:{m_identity}发生了两次实例化!");
                Destroy(netObj.gameObject);
            }
        }
        public void InitAll()
        {
            Init();
            var nbs = GetComponentsInChildren<NetworkBehaviour>();
            foreach (var np in nbs)
            {
                np.Init();
            }
        }
        internal void InitSyncVar(object target)
        {
            ClientBase.Instance.AddRpcHandle(target, false, true, (info) =>
            {
                info.id = (ushort)syncVarInfos.Count;
                syncVarInfos.Add(info);
            });
        }

        internal void CheckSyncVar()
        {
            SyncVarHelper.CheckSyncVar(isOtherCreate, syncVarInfos, (buffer)=> 
            {
                ClientBase.Instance.AddOperation(new Operation(NetCmd.SyncVar, m_identity)
                {
                    uid = ClientBase.Instance.UID,
                    index = registerObjectIndex,
                    buffer = buffer
                });
            });
        }

        internal void SyncVarHandler(Operation opt)
        {
            if (opt.cmd != NetCmd.SyncVar | opt.uid == ClientBase.Instance.UID)
                return;
            SyncVarHelper.SyncVarHandler(syncVarInfos, opt.buffer);
        }

        internal void RemoveSyncVar(NetworkBehaviour target)
        {
            SyncVarHelper.RemoveSyncVar(syncVarInfos, target);
        }

        internal void PropertyAutoCheckHandler() 
        {
            foreach (var networkBehaviour in networkBehaviours)
            {
                if (!networkBehaviour.CheckEnabled())
                    continue;
                networkBehaviour.OnPropertyAutoCheck();
            }
        }

        public virtual void OnDestroy()
        {
            isDispose = true;
            if (m_identity == -1)
                return;
            var nsm = NetworkSceneManager.I;
            if(nsm != null)
                nsm.identitys.Remove(m_identity);
            if (isOtherCreate | m_identity < 10000)//identity < 10000则是自定义唯一标识
                return;
            IDENTITY_POOL.Enqueue(m_identity);
            if (ClientBase.Instance == null)
                return;
            ClientBase.Instance.AddOperation(new Operation(Command.Destroy, m_identity));
        }

        /// <summary>
        /// 初始化网络唯一标识
        /// </summary>
        /// <param name="capacity">一个客户端可以用的唯一标识容量</param>
        public static void Init(int capacity = 5000) 
        {
            if (IsInit)
                return;
            IsInit = true;
            Capacity = capacity;
            //服务器的记录uid从10000开始,所以这里uid+1-10000=1(网络物体记录从1开始, 而0则可以用作核心网络物体,比如玩家的网络物体), 这里 * 5000是每个客户端都可以实例化5000个networkObject网络物体组件
            //并且保证唯一id都是正确的,如果一个客户端实例化超过5000个, 就会和uid=10001的玩家networkObject网络物体组件唯一id碰撞, 会出现鬼畜问题
            IDENTITY = 10000 + ((ClientBase.Instance.UID + 1 - 10000) * capacity);
            IDENTITY_MAX = IDENTITY + capacity;
            //for (int i = IDENTITY; i < IDENTITY + capacity; i++)
            //    IDENTITY_POOL.Enqueue(i);
        }

        /// <summary>
        /// 获取玩家id的偏移量, 此方法算出来每个玩家可实例化多少个网络对象
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static int GetUserIdOffset(int uid)
        {
            return 10000 + ((uid + 1 - 10000) * Capacity);
        }
    }
}
#endif