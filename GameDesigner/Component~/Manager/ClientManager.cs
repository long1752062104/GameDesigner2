#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using System;
using Net.Client;
using Net.Helper;
using Net.Share;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Net.Component
{
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
        public bool startConnect = true;
        public NetworkUpdateMode updateMode = NetworkUpdateMode.Thread;
        public int heartInterval = 1000;
        public byte heartLimit = 5;
        public int reconnectInterval = 2000;
        public int reconnectCount = 10;
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
                    _client.UpdateMode = updateMode;
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
            _client.AddRpcHandle(this);
            return _client.Connect(result =>
            {
                if (result)
                {
                    _client.Call(new byte[1]);//发送一个字节:调用服务器的OnUnClientRequest方法, 如果不需要账号登录, 则会直接允许进入服务器
                }
            });
        }

        // Update is called once per frame
        public virtual void Update()
        {
            if (_client == null)
                return;
            _client.NetworkUpdate();
        }

        public virtual void OnDestroy()
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
        public void Call(uint protocol, params object[] pars)
        {
            ((ISendHandle)client).Call(protocol, pars);
        }

        public void Call(byte cmd, uint protocol, params object[] pars)
        {
            ((ISendHandle)client).Call(cmd, protocol, pars);
        }

        public void Call(byte[] buffer)
        {
            ((ISendHandle)client).Call(buffer);
        }

        public void Call(byte cmd, byte[] buffer)
        {
            ((ISendHandle)client).Call(cmd, buffer);
        }

        public void Call(string func, params object[] pars)
        {
            ((ISendHandle)client).Call(func, pars);
        }

        public void Call(byte cmd, string func, params object[] pars)
        {
            ((ISendHandle)client).Call(cmd, func, pars);
        }

        public void Call(byte cmd, uint protocol, byte[] buffer, params object[] pars)
        {
            ((ISendHandle)client).Call(cmd, protocol, buffer, pars);
        }

        public void Call(RPCModel model)
        {
            ((ISendHandle)client).Call(model);
        }

        public UniTask<RPCModelTask> Request(uint protocol, params object[] pars)
        {
            return ((ISendHandle)client).Request(protocol, pars);
        }

        public UniTask<RPCModelTask> Request(uint protocol, int timeoutMilliseconds, params object[] pars)
        {
            return ((ISendHandle)client).Request(protocol, timeoutMilliseconds, pars);
        }

        public UniTask<RPCModelTask> Request(uint protocol, int timeoutMilliseconds, bool intercept, params object[] pars)
        {
            return ((ISendHandle)client).Request(protocol, timeoutMilliseconds, intercept, pars);
        }

        public UniTask<RPCModelTask> Request(byte cmd, uint protocol, int timeoutMilliseconds, params object[] pars)
        {
            return ((ISendHandle)client).Request(cmd, protocol, timeoutMilliseconds, pars);
        }

        public UniTask<RPCModelTask> Request(byte cmd, uint protocol, int timeoutMilliseconds, bool intercept, params object[] pars)
        {
            return ((ISendHandle)client).Request(cmd, protocol, timeoutMilliseconds, intercept, pars);
        }

        public UniTask<RPCModelTask> Request(byte cmd, uint protocol, int timeoutMilliseconds, bool intercept, bool serialize, byte[] buffer, params object[] pars)
        {
            return ((ISendHandle)client).Request(cmd, protocol, timeoutMilliseconds, intercept, serialize, buffer, pars);
        }
        #endregion
    }
}
#endif