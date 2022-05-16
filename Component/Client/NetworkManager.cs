#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA
using Net.Client;
using Net.Event;
using Net.Share;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Threading.Tasks;
using System;

namespace Net.Component
{
    [Serializable]
    public class ClientGourp 
    {
        internal ClientBase _client;
        public TransportProtocol protocol = TransportProtocol.Gudp;
        public string ip = "127.0.0.1";
        public int port = 6666;
        public bool throwException;
        public bool debugRpc = true;
        public int frameRate = 60;
        public bool authorize;
        public bool startConnect = true;
        public bool md5CRC;
        public List<RPCMethod> rpcs = new List<RPCMethod>();
        [Header("序列化适配器")]
        public SerializeAdapterType type;
        public bool isEncrypt = false;//数据加密?

        public ClientBase client
        {
            get
            {
                if (_client != null)
                    return _client;
                switch (protocol)
                {
                    case TransportProtocol.Gudp:
                        _client = new UdpClient(true);
                        break;
                    case TransportProtocol.Tcp:
                        _client = new TcpClient(true);
                        break;
                    case TransportProtocol.Kcp:
                        _client = new KcpClient(true);
                        break;
                    case TransportProtocol.Udx:
                        _client = new UdxClient(true);
                        break;
#if UNITY_STANDALONE_WIN || UNITY_WSA
                    case TransportProtocol.Web:
                        _client = new Client.WebClient(true);
                        break;
#endif
                }
                _client.host = ip;
                _client.port = port;
                _client.ThrowException = throwException;
                _client.LogRpc = debugRpc;
                _client.MD5CRC = md5CRC;
                return _client;
            }
            set { _client = value; }
        }

        public Task<bool> Connect()
        {
            _client = client;
            var ips = Dns.GetHostAddresses(ip);
            if (ips.Length > 0)
                _client.host = ips[RandomHelper.Range(0, ips.Length)].ToString();
            else
                _client.host = ip;
            _client.port = port;
            switch (type)
            {
                case SerializeAdapterType.Default:
                    break;
                case SerializeAdapterType.PB_JSON_FAST:
                    _client.AddAdapter(new Adapter.SerializeFastAdapter() { isEncrypt = isEncrypt });
                    break;
                case SerializeAdapterType.Binary:
                    _client.AddAdapter(new Adapter.SerializeAdapter() { isEncrypt = isEncrypt });
                    break;
                case SerializeAdapterType.Binary2:
                    _client.AddAdapter(new Adapter.SerializeAdapter2() { isEncrypt = isEncrypt });
                    break;
                case SerializeAdapterType.Binary3:
                    _client.AddAdapter(new Adapter.SerializeAdapter3() { isEncrypt = isEncrypt });
                    break;
            }
            return _client.Connect(result =>
            {
                if (result)
                {
                    _client.Send(new byte[1]);//发送一个字节:调用服务器的OnUnClientRequest方法, 如果不需要账号登录, 则会直接允许进入服务器
                }
            });
        }

        public Task<bool> Connect(string ip, int port)
        {
            this.ip = ip;
            this.port = port;
            return Connect();
        }
    }

    public class NetworkManager : SingleCase<NetworkManager>
    {
        public List<ClientGourp> clients = new List<ClientGourp>();

        void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Application.runInBackground = true;
        }

        // Use this for initialization
        void Start()
        {
            NDebug.BindLogAll(Debug.Log);
            foreach (var client in clients)
            {
                if (client.startConnect)
                    client.Connect();
            }
        }

        // Update is called once per frame
        void Update()
        {
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i]._client == null)
                    continue;
                clients[i]._client.NetworkEventUpdate();
                clients[i].rpcs = clients[i]._client.RPCs;
            }
        }

        void OnDestroy()
        {
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i]._client == null)
                    continue;
                clients[i]._client.Close();
            }
        }

        public static void BindNetworkAllHandle(INetworkHandle handle)
        {
            foreach (var item in I.clients)
            {
                item.client.BindNetworkHandle(handle);
            }
        }

        public static void AddRpcHandle(object target)
        {
            foreach (var item in I.clients)
            {
                item.client.AddRpcHandle(target);
            }
        }

        public static void RemoveRpc(object target)
        {
            foreach (var item in I.clients)
            {
                item.client.RemoveRpc(target);
            }
        }

        public static void Close(bool v1, int v2)
        {
            foreach (var item in I.clients)
            {
                item.client.Close(v1, v2);
            }
        }

        public static void CallUnity(Action ptr)
        {
            I.clients[0].client.WorkerQueue.Enqueue(ptr);
        }

        public static void DispatcherRpc(ushort hash, params object[] parms)
        {
            I.clients[1].client.DispatcherRpc(hash, parms);
        }
    }
}
#endif