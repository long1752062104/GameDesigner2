#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using Net.Client;
using Net.Event;
using Net.Share;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System;
using Net.Helper;
using Cysharp.Threading.Tasks;

namespace Net.Component
{
    [Serializable]
    public class ClientGourp
    {
        public string name;
        public ClientBase _client;
        public TransportProtocol protocol = TransportProtocol.Tcp;
        public string ip = "127.0.0.1";
        public int port = 9543;
#if UNITY_EDITOR
        public bool localTest;//本机测试
#endif
        public bool debugRpc = true;
        public bool authorize;
        public bool startConnect = true;
        public bool singleThread;
        public int reconnectCount = 10;
        public int reconnectInterval = 2000;
        public byte heartLimit = 5;
        public int heartInterval = 1000;
        public WebSetting webSetting = new WebSetting();

        public ClientBase Client
        {
            get
            {
                if (_client != null)
                    return _client;
                var typeName = $"Net.Client.{protocol}Client";
                var type = AssemblyHelper.GetType(typeName);
                if (type == null)
                    throw new Exception($"请导入:{protocol}协议!!!");
                _client = Activator.CreateInstance(type, new object[] { true }) as ClientBase;
                _client.host = ip;
                _client.port = port;
                _client.LogRpc = debugRpc;
                _client.IsMultiThread = !singleThread;
                _client.ReconnectCount = reconnectCount;
                _client.ReconnectInterval = reconnectInterval;
                _client.SetHeartTime(heartLimit, heartInterval);
                _client.OnSetConfigInfo(webSetting.scheme, webSetting.sslProtocols, webSetting.pfxPath, webSetting.password);
                return _client;
            }
            set { _client = value; }
        }

        public UniTask<bool> Connect()
        {
            _client = Client;
#if !UNITY_WEBGL
            var ips = Dns.GetHostAddresses(ip);
            if (ips.Length > 0)
                _client.host = ips[RandomHelper.Range(0, ips.Length)].ToString();
            else
#endif
            _client.host = ip;
#if UNITY_EDITOR
            if (localTest) _client.host = "127.0.0.1";
#endif
            _client.port = port;
            return _client.Connect(result =>
            {
                if (result)
                {
                    _client.Send(new byte[1]);//发送一个字节:调用服务器的OnUnClientRequest方法, 如果不需要账号登录, 则会直接允许进入服务器
                }
            });
        }

        public UniTask<bool> Connect(string ip, int port)
        {
            this.ip = ip;
            this.port = port;
            return Connect();
        }
    }

    public class NetworkManagerBase : MonoBehaviour
    {
#if UNITY_2020_1_OR_NEWER
        [NonReorderable]
#endif
        public List<ClientGourp> clients = new List<ClientGourp>();

        public ClientBase this[int index]
        {
            get { return clients[index].Client; }
            set { clients[index].Client = value; }
        }

        public virtual void Awake()
        {
        }

        // Use this for initialization
        public virtual void Start()
        {
            foreach (var client in clients)
                if (client.startConnect)
                    client.Connect();
        }

        // Update is called once per frame
        public virtual void Update()
        {
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i]._client == null)
                    continue;
                clients[i]._client.NetworkUpdate();
            }
        }

        public virtual void OnDestroy()
        {
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i]._client == null)
                    continue;
                clients[i]._client.Close();
            }
        }

        public void BindNetworkAll(INetworkHandle handle)
        {
            foreach (var item in clients)
            {
                item.Client.BindNetworkHandle(handle);
            }
        }

        /// <summary>
        /// 添加索引0的客户端rpc, 也就是1的客户端
        /// </summary>
        /// <param name="target"></param>
        public void AddRpcOne(object target)
        {
            clients[0].Client.AddRpc(target);
        }

        /// <summary>
        /// 添加索引1的客户端, 也就是2的客户端
        /// </summary>
        /// <param name="target"></param>
        public void AddRpcTwo(object target)
        {
            clients[1].Client.AddRpc(target);
        }

        /// <summary>
        /// 添加指定索引的客户端rpc, 如果索引小于0则为全部添加
        /// </summary>
        /// <param name="clientIndex"></param>
        /// <param name="target"></param>
        public void AddRpc(int clientIndex, object target)
        {
            if (clientIndex < 0)
                foreach (var item in clients)
                    item.Client.AddRpc(target);
            else clients[clientIndex].Client.AddRpc(target);
        }

        /// <summary>
        /// 移除索引0的客户端rpc, 也就是1的客户端
        /// </summary>
        /// <param name="target"></param>
        public void RemoveRpcOne(object target)
        {
            clients[0].Client.RemoveRpc(target);
        }

        /// <summary>
        /// 移除索引1的客户端rpc, 也就是2的客户端
        /// </summary>
        /// <param name="target"></param>
        public void RemoveRpcTwo(object target)
        {
            clients[1].Client.RemoveRpc(target);
        }

        /// <summary>
        /// 移除指定索引的客户端rpc, 如果索引小于0则为全部添加
        /// </summary>
        /// <param name="clientIndex"></param>
        /// <param name="target"></param>
        public void RemoveRpc(int clientIndex, object target)
        {
            if (clientIndex < 0)
                foreach (var item in clients)
                    item.Client.RemoveRpc(target);
            else clients[clientIndex].Client.RemoveRpc(target);
        }

        public void Close(bool v1, int v2)
        {
            foreach (var item in clients)
            {
                item.Client.Close(v1, v2);
            }
        }

        public void CallUnity(Action ptr)
        {
            clients[0].Client.WorkerQueue.Call(ptr);
        }

        public void DispatcherRpc(ushort hash, params object[] parms)
        {
            clients[1].Client.DispatchRpc(hash, parms);
        }
    }

    public class NetworkManager : NetworkManagerBase
    {
        private static NetworkManager instance;
        public static NetworkManager Instance
        {
            get
            {
                if (instance == null)
                    instance = FindObjectOfType<NetworkManager>();
                return instance;
            }
        }
        public static NetworkManager I => Instance;
        public bool dontDestroyOnLoad = true;

        public override void Awake()
        {
            instance = this;
            if (dontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);
            Application.runInBackground = true;
        }
    }
}
#endif