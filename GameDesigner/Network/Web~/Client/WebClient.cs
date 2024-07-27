using System;
using System.IO;
using Net.Event;
using Net.Share;
using Net.System;
using Net.Helper;
using Newtonsoft_X.Json;
using Cysharp.Threading.Tasks;
using System.Security.Authentication;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Net.Sockets;


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
        public override int HeartInterval { get; set; } = 1000 * 60 * 10;//10分钟跳一次
        public override byte HeartLimit { get; set; } = 2;//确认两次
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
#if UNITY_EDITOR || !UNITY_WEBGL
                if (host == "127.0.0.1" | host == "localhost")
                    host = NetPort.GetIP();
#endif
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
                WSClient.OnOpen += (sender, e) =>
                {
                    var segment = BufferPool.Take(SendBufferSize);
                    segment.Write(PreUserId);
#if UNITY_EDITOR || !UNITY_WEBGL
                    WSClient.Send(segment.ToArray());
#else
                    WSClient.SendAsync(segment.ToArray());
#endif
                };
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
                };
                WSClient.OnMessage += (sender, e) =>
                {
                    if (e.IsText)
                    {
                        receiveCount += e.Data.Length * 2;
                        receiveAmount++;
                        var model = JsonConvert.DeserializeObject<MessageModel>(e.Data);
                        var model1 = new RPCModel(model.cmd, model.func.CRCU32(), model.GetPars());
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
                        ResolveBuffer(ref buffer);
                        BufferPool.Push(buffer);
                    }
                };
                WSClient.ConnectAsync(); //这里必须是Async才能在WebGL对接相同的API
                await UniTaskNetExtensions.Wait(8000, (state) =>
                {
                    NetworkTick();
                    return UID != 0 | !openClient; //如果在爆满事件关闭客户端就需要判断一下
                }, null);
                if (UID == 0 && openClient)
                    throw new Exception("连接握手失败!");
                if (UID == 0 && !openClient)
                    throw new Exception("客户端调用Close!");
                Connected = true;
                StartupThread();
                await UniTask.Yield(); //切换到线程池中, 不要由事件线程去往下执行, 如果有耗时就会卡死事件线程, 在unity会切换到unity线程去执行，解决unity组件访问错误问题
                result(true);
                return true;
            }
            catch (Exception ex)
            {
                NDebug.Log("连接错误: " + ex.ToString());
                await UniTask.Yield(); //在unity会切换到unity线程去执行，解决unity组件访问错误问题
                result(false);
                return false;
            }
        }

        public override void ReceiveHandler()
        {
        }

        protected override void SendByteData(ISegment buffer)
        {
            if (buffer.Count <= Frame)//解决长度==5的问题(没有数据)
                return;
            sendCount += buffer.Count;
            sendAmount++;
#if UNITY_EDITOR || !UNITY_WEBGL
            WSClient.Send(new MemoryStream(buffer.Buffer, buffer.Offset, buffer.Count, true, true), buffer.Count);
#else
            WSClient.SendAsync(buffer.ToArray());
#endif
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
            base.Close(await, millisecondsTimeout);
            if (WSClient != null)
            {
                WSClient.CloseAsync();
                WSClient = null;
            }
        }
    }
}