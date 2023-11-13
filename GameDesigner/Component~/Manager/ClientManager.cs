#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using System;
using System.Net;
using Net.Client;
using Net.Helper;
using Net.Share;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Security.Authentication;

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

    [DefaultExecutionOrder(1)]//在NetworkTransform组件之前执行OnDestroy，控制NetworkTransform处于Control模式时退出游戏会同步删除所有网络物体
    public class ClientManager : SingleCase<ClientManager>, ISendHandle
    {
        private bool mainInstance;
        private ClientBase _client;
        public TransportProtocol protocol = TransportProtocol.Tcp;
        public string ip = "127.0.0.1";
        public int port = 9543;
#if UNITY_EDITOR
        public bool localTest;
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
        public bool dontDestroyOnLoad = true;

#pragma warning disable IDE1006 // 命名样式
        public ClientBase client
#pragma warning restore IDE1006 // 命名样式
        {
            get
            {
                if (_client == null)
                {
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
                }
                return _client;
            }
            set { _client = value; }
        }

        /// <summary>
        /// 客户端唯一标识
        /// </summary>
        public static string Identify { get { return Instance.client.Identify; } }
        /// <summary>
        /// 客户端唯一标识
        /// </summary>
        public static int UID { get { return Instance.client.UID; } }

        protected override void Awake()
        {
            base.Awake();
            mainInstance = true;
            if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);
            Application.runInBackground = true;
        }

        // Use this for initialization
        protected virtual void Start()
        {
            if (startConnect)
                Connect();
        }

        public UniTask<bool> Connect()
        {
            _client = client;
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
            _client.AddRpcHandle(this);
            return _client.Connect(result =>
            {
                if (result)
                {
                    _client.Send(new byte[1]);//发送一个字节:调用服务器的OnUnClientRequest方法, 如果不需要账号登录, 则会直接允许进入服务器
                }
            });
        }

        // Update is called once per frame
        void Update()
        {
            if (_client == null)
                return;
            _client.NetworkUpdate();
        }

        void OnDestroy()
        {
            if (!mainInstance)
                return;
            _client?.Close();
        }

        /// <summary>
        /// 发起场景同步操作, 在同一个场景的所有客户端都会收到该操作参数operation
        /// </summary>
        /// <param name="operation"></param>
        public static void AddOperation(Operation operation)
        {
            Instance.client.AddOperation(operation);
        }

        public static void AddRpc(object target)
        {
            I.client.AddRpcHandle(target);
        }

        public static void RemoveRpc(object target)
        {
            I.client.RemoveRpc(target);
        }

        /// <summary>
        /// 判断name是否是本地唯一id(本机玩家标识)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal static bool IsLocal(string name)
        {
            if (Instance == null)
                return false;
            return instance._client.Identify == name;
        }

        /// <summary>
        /// 判断uid是否是本地唯一id(本机玩家标识)
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        internal static bool IsLocal(int uid)
        {
            if (Instance == null)
                return false;
            return instance._client.UID == uid;
        }

        public static void CallUnity(Action ptr)
        {
            I.client.WorkerQueue.Call(ptr);
        }

        #region 发送接口实现
        public void Send(byte[] buffer)
        {
            ((ISendHandle)_client).Send(buffer);
        }

        public void Send(byte cmd, byte[] buffer)
        {
            ((ISendHandle)_client).Send(cmd, buffer);
        }

        public void Send(string func, params object[] pars)
        {
            ((ISendHandle)_client).Send(func, pars);
        }

        public void Send(byte cmd, string func, params object[] pars)
        {
            ((ISendHandle)_client).Send(cmd, func, pars);
        }

        public void SendRT(string func, params object[] pars)
        {
            ((ISendHandle)_client).SendRT(func, pars);
        }

        public void SendRT(byte cmd, string func, params object[] pars)
        {
            ((ISendHandle)_client).SendRT(cmd, func, pars);
        }

        public void SendRT(byte[] buffer)
        {
            ((ISendHandle)_client).SendRT(buffer);
        }

        public void SendRT(byte cmd, byte[] buffer)
        {
            ((ISendHandle)_client).SendRT(cmd, buffer);
        }
        #endregion
    }
}
#endif