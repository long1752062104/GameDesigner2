using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Net.Event;
using Net.Share;
using Net.System;
using Newtonsoft_X.Json;
using Cysharp.Threading.Tasks;
using Net.Helper;
using System.Security.Authentication;
using System.Runtime.CompilerServices;

#if !UNITY_EDITOR && UNITY_WEBGL
using UnityWebSocket;
#else
using WebSocketSharp;
#endif
#if COCOS2D_JS
using System.Text;
using Net.Serialize;
#endif

namespace Net.Client
{
    /// <summary>
    /// web客户端类型
    /// 第三版本 2020.9.14
    /// </summary>
    [Serializable]
    public class WebClient : ClientBase
    {
        public WebSocket WSClient { get; private set; }
        /// <summary>
        /// websocket连接策略, 有wss和ws
        /// </summary>
        public string Scheme { get; set; } = "ws";
        /// <summary>
        /// Ssl类型
        /// </summary>
        public SslProtocols SslProtocols { get; set; }
        /// <summary>
        /// 证书
        /// </summary>
        public X509Certificate2 Certificate { get; set; }

        /// <summary>
        /// 构造websocket客户端
        /// </summary>
        public WebClient()
        {
        }

        /// <summary>
        /// 构造websocket客户端
        /// </summary>
        /// <param name="useUnityThread">使用unity多线程?</param>
        public WebClient(bool useUnityThread) : this()
        {
            UseUnityThread = useUnityThread;
        }

        ~WebClient()
        {
#if !UNITY_EDITOR
            Close();
#endif
        }

        public override void OnSetConfigInfo(params object[] args)
        {
            Scheme = args[0].ToString();
            SslProtocols = (SslProtocols)args[1];
            var pfxPath = args[2].ToString();
            var password = args[3].ToString();
            if (string.IsNullOrEmpty(pfxPath))
                return;
            if (!File.Exists(pfxPath))
                return;
            var pfxData = File.ReadAllBytes(pfxPath);
            Certificate = new X509Certificate2(pfxData, password);
        }

        protected override async UniTask<bool> ConnectResult(string host, int port, int localPort, Action<bool> result)
        {
            try
            {
                var isConnectFailed = false;
                if (host == "127.0.0.1" | host == "localhost")
                    host = NetPort.GetIP();
                WSClient = new WebSocket($"{Scheme}://{host}:{port}/");
#if UNITY_EDITOR || !UNITY_WEBGL
                if (Scheme == "wss")
                {
                    if (Certificate == null)
                        Certificate = CertificateHelper.GetDefaultCertificate();
                    WSClient.SslConfiguration.ClientCertificates = new X509CertificateCollection { Certificate };
                    WSClient.SslConfiguration.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                    WSClient.SslConfiguration.EnabledSslProtocols = SslProtocols;
                }
#endif
                WSClient.OnError += (sender, e) =>
                {
                    NDebug.LogError(e.Exception);
                };
                WSClient.OnClose += (sender, e) =>
                {
                    Connected = false;
                    NetworkState = NetworkState.ConnectLost;
                    InvokeInMainThread(OnConnectLostHandle);
                    RpcModels = new QueueSafe<RPCModel>();
                    NDebug.Log("websocket关闭！");
                    isConnectFailed = true;
                };
                WSClient.OnMessage += (sender, e) =>
                {
                    if (e.IsText)
                    {
                        receiveCount += e.Data.Length * 2;
                        receiveAmount++;
                        MessageModel model = JsonConvert.DeserializeObject<MessageModel>(e.Data);
                        RPCModel model1 = new RPCModel(model.cmd, model.func, model.GetPars());
                        CommandHandler(model1, null);
                    }
                    else if (e.IsBinary)
                    {
                        var data = e.RawData;
                        receiveCount += data.Length;
                        receiveAmount++;
                        var buffer = BufferPool.Take(data.Length);
                        Unsafe.CopyBlockUnaligned(ref buffer.Buffer[0], ref data[0], (uint)data.Length);
                        buffer.Count = data.Length;
                        ResolveBuffer(ref buffer, false);
                        BufferPool.Push(buffer);
                    }
                };
                WSClient.ConnectAsync();
                var tick = (uint)Environment.TickCount + 8000u;
                while (UID == 0)
                {
                    await UniTask.Yield();
                    if ((uint)Environment.TickCount >= tick)
                        throw new Exception("uid赋值失败!");
                    if (!openClient)
                        throw new Exception("客户端调用Close!");
                    if (isConnectFailed)
                        throw new Exception("连接服务器失败");
                }
                Connected = true;
                StartupThread();
                result(true);
                return await UniTask.FromResult(true);
            }
            catch (Exception ex)
            {
                NDebug.Log("连接错误: " + ex.ToString());
                result(false);
                return await UniTask.FromResult(false);
            }
        }

        public override void ReceiveHandler()
        {
        }

        protected override bool HeartHandler()
        {
            try
            {
                if (++heart <= HeartLimit)
                    return true;
                if (Connected)
                    Send(NetCmd.SendHeartbeat, new byte[0]);
                else//尝试连接执行
                    InternalReconnection();
            }
            catch { }
            return openClient & CurrReconnect < ReconnectCount;
        }

        protected override void SendByteData(byte[] buffer)
        {
            sendCount += buffer.Length;
            sendAmount++;
            WSClient.SendAsync(buffer);
        }

#if COCOS2D_JS
        protected internal override byte[] OnSerializeRpcInternal(RPCModel model)
        {
            if (!string.IsNullOrEmpty(model.func) | model.methodHash != 0)
            {
                var model1 = new MessageModel(model.cmd, model.func, model.pars);
                string jsonStr = JsonConvert.SerializeObject(model1);
                byte[] jsonStrBytes = Encoding.UTF8.GetBytes(jsonStr);
                byte[] bytes = new byte[jsonStrBytes.Length + 1];
                bytes[0] = 32; //32=utf8的" "空字符
                Unsafe.CopyBlockUnaligned(ref bytes[1], ref jsonStrBytes[0], (uint)jsonStrBytes.Length);
                return bytes;
            }
            return NetConvert.Serialize(model, new byte[] { 10 });//10=utf8的\n字符
        }

        protected internal override FuncData OnDeserializeRpcInternal(byte[] buffer, int index, int count)
        {
            if (buffer[index++] == 32)
            {
                var message = Encoding.UTF8.GetString(buffer, index, count - 1);
                var model = JsonConvert.DeserializeObject<MessageModel>(message);
                return new FuncData(model.func, model.GetPars());
            }
            return NetConvert.Deserialize(buffer, index, count - 1);
        }
#endif

        public override void Close(bool await = true, int millisecondsTimeout = 100)
        {
            var isDispose = openClient;
            Connected = false;
            openClient = false;
            NetworkState = NetworkState.ConnectClosed;
            InvokeInMainThread(OnCloseConnectHandle);
            AbortedThread();
            if (WSClient != null)
            {
                WSClient.CloseAsync();
                WSClient = null;
            }
            StackStream?.Close();
            StackStream = null;
            stack = 0;
            UID = 0;
            PreUserId = 0;
            CurrReconnect = 0;
            if (Instance == this) Instance = null;
            if (Gcp != null) Gcp.Dispose();
            if (isDispose) NDebug.Log("客户端已关闭！");
        }

        /// <summary>
        /// udp压力测试
        /// </summary>
        /// <param name="ip">服务器ip</param>
        /// <param name="port">服务器端口</param>
        /// <param name="clientLen">测试客户端数量</param>
        /// <param name="dataLen">每个客户端数据大小</param>
        public static CancellationTokenSource Testing(string ip, int port, int clientLen, int dataLen)
        {
            var cts = new CancellationTokenSource();
            Task.Run(() =>
            {
                var clients = new List<WebSocket>();
                for (int i = 0; i < clientLen; i++)
                {
                    var socket = new WebSocket($"ws://{ip}:{port}/");
                    socket.ConnectAsync();
                    clients.Add(socket);
                }
                var buffer = new byte[dataLen];
                using (var stream = new MemoryStream(512))
                {
                    int crcIndex = 0;
                    byte crcCode = 0x2d;
                    stream.Write(new byte[4], 0, 4);
                    stream.WriteByte((byte)crcIndex);
                    stream.WriteByte(crcCode);
                    var rPCModel = new RPCModel(NetCmd.CallRpc, buffer);
                    stream.WriteByte((byte)(rPCModel.kernel ? 68 : 74));
                    stream.WriteByte(rPCModel.cmd);
                    stream.Write(BitConverter.GetBytes(rPCModel.buffer.Length), 0, 4);
                    stream.Write(rPCModel.buffer, 0, rPCModel.buffer.Length);

                    stream.Position = 0;
                    int len = (int)stream.Length - 6;
                    stream.Write(BitConverter.GetBytes(len), 0, 4);
                    stream.Position = len + 6;
                    buffer = stream.ToArray();
                }
                while (!cts.IsCancellationRequested)
                {
                    Thread.Sleep(31);
                    for (int i = 0; i < clients.Count; i++)
                        clients[i].SendAsync(buffer);
                }
                for (int i = 0; i < clients.Count; i++)
                    clients[i].CloseAsync();
            }, cts.Token);
            return cts;
        }
    }
}