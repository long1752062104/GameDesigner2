#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using System;
using System.Collections.Generic;
using System.Security.Authentication;
using Net.Client;
using Net.Helper;
using Net.Share;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Net.Component
{
    public enum TransportProtocol
    {
        Tcp, Gcp, Udx, Kcp, Web
    }

    public enum LogMode
    {
        None,
        /// <summary>
        /// 消息输出, 警告输出, 错误输出, 三种模式各自输出
        /// </summary>
        Default,
        /// <summary>
        /// 所有消息输出都以白色消息输出
        /// </summary>
        LogAll,
        /// <summary>
        /// 警告信息和消息一起输出为白色
        /// </summary>
        LogAndWarning,
        /// <summary>
        /// 警告和错误都输出为红色提示
        /// </summary>
        WarnAndError,
        /// <summary>
        /// 只输出错误日志
        /// </summary>
        OnlyError,
        /// <summary>
        /// 只输入警告和错误日志
        /// </summary>
        OnlyWarnAndError,
    }

    [Serializable]
    public class WebSetting
    {
        public string scheme = "ws";
        public SslProtocols sslProtocols;
        public string pfxPath;
        public string password;
    }

    [Serializable]
    public class ClientUnit
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
        public bool startConnect = true;
        public NetworkUpdateMode updateMode = NetworkUpdateMode.Thread;
        public Performance performance = Performance.Realtime;
        public int heartInterval = 1000;
        public byte heartLimit = 5;
        public int reconnectInterval = 2000;
        public int reconnectCount = 10;
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
                _client.UpdateMode = updateMode;
                _client.Performance = performance;
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
            var ips = global::System.Net.Dns.GetHostAddresses(ip);
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
                    _client.Call(new byte[1]);//发送一个字节:调用服务器的OnUnClientRequest方法, 如果不需要账号登录, 则会直接允许进入服务器
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

    public abstract class NetworkManagerBase : MonoBehaviour
    {
#if UNITY_2020_1_OR_NEWER
        [NonReorderable]
#endif
        public List<ClientUnit> clients = new List<ClientUnit>();

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

        public void Close(bool isWait = true, int millisecondsTimeout = 100)
        {
            foreach (var item in clients)
            {
                item.Client.Close(isWait, millisecondsTimeout);
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
}
#endif