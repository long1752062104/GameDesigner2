using Net.Helper;
using Net.Share;
using Net.System;
using System;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using WebSocketSharp.Net;
using Debug = Net.Event.NDebug;

namespace Net.Server
{
    public class WebServerNew<Player, Scene> : ServerBase<Player, Scene> where Player : WebPlayerNew, new() where Scene : NetScene<Player>, new()
    {
        public override int HeartInterval { get; set; } = 1000 * 60 * 2;//2分钟跳一次
        public override byte HeartLimit { get; set; } = 2;//确认两次

        /// <summary>
        /// websocket连接策略, 有wss和ws
        /// </summary>
        public string Scheme { get; set; } = "ws";
        /// <summary>
        /// 证书
        /// </summary>
        public X509Certificate2 Certificate { get; set; }
        /// <summary>
        /// Ssl类型
        /// </summary>
        public SslProtocols SslProtocols { get; set; }
        private RemoteCertificateValidationCallback _clientCertValidationCallback;
        public RemoteCertificateValidationCallback ClientCertificateValidationCallback
        {
            get => _clientCertValidationCallback ??= DefaultValidateClientCertificate;
            set => _clientCertValidationCallback = value;
        }
        public bool ClientCertificateRequired { get; private set; }
        public bool CheckCertificateRevocation { get; private set; }

        private bool DefaultValidateClientCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;

        protected override void CreateOtherThread()
        {
            var thread = new Thread(ProcessAcceptConnect) { IsBackground = true, Name = "ProcessAcceptConnect" };
            thread.Start();
            ServerThreads.Add("ProcessAcceptConnect", thread);
        }

        protected override void CreateServerSocket(ushort port)
        {
            try
            {
                var address = new IPEndPoint(IPAddress.Any, port);
                Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    SendBufferSize = SendBufferSize,
                    ReceiveBufferSize = ReceiveBufferSize,
                    NoDelay = true
                };
                if (Certificate == null)
                    Certificate = CertificateHelper.GetDefaultCertificate();
                Server.Bind(address);
                Server.Listen(LineUp);
            }
            catch (Exception ex)
            {
                Debug.LogError("监听异常:" + ex);
            }
        }

        protected override void StartSocketHandler()
        {
        }

        private void ProcessAcceptConnect()
        {
            var acceptList = new FastList<WebSocketSession>();
            WebSocketSession session;
            while (IsRunServer)
            {
                try
                {
                    if (Server.Poll(0, SelectMode.SelectRead))
                    {
                        var socket = Server.Accept();
                        socket.ReceiveTimeout = (int)ReconnectionTimeout;
                        session = new WebSocketSession(socket);
                        var stream = new NetworkStream(socket);
                        if (Scheme == "wss")
                        {
                            var sslStream = new SslStream(stream, false, ClientCertificateValidationCallback);
                            sslStream.AuthenticateAsServer(Certificate, ClientCertificateRequired, SslProtocols, CheckCertificateRevocation);
                            session.stream = sslStream;
                        }
                        else session.stream = stream;
                        acceptList.Add(session);
                    }
                    else Thread.Sleep(1);
                    CheckAcceptList(acceptList);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"接受异常:{ex}");
                }
            }
        }

        private void CheckAcceptList(FastList<WebSocketSession> acceptList)
        {
            WebSocketSession session;
            for (int i = 0; i < acceptList.Count; i++)
            {
                session = acceptList[i];
                if (!session.Connected)
                {
                    session.Close();
                    acceptList.RemoveAt(i);
                    continue;
                }
                if (!session.isHandshake)
                {
                    session.PerformHandshake();
                    continue;
                }
                session.Receive((opcode, segment) =>
                {
                    CheckReconnect(session.socket, segment, session);
                    acceptList.RemoveAt(i);
                });
            }
        }

        protected override void AcceptHander(Player client, params object[] args)
        {
            client.Session = args[0] as WebSocketSession;
        }

        protected override void ResolveDataQueue(Player client, ref bool isSleep, uint tick)
        {
            if (!client.Client.Connected)
                return;
            client.Session.Receive((opcode, segment) =>
            {
                receiveAmount++;
                receiveCount += segment.Count;
                client.BytesReceived += segment.Count;
                ResolveBuffer(client, ref segment);
            });
        }

        protected override bool CheckIsConnected(Player client, uint tick)
        {
            if (!client.Connected)
            {
                if (tick >= client.ReconnectTimeout)
                    RemoveClient(client);
                return false;
            }
            if (!client.Client.Connected)
            {
                ConnectLost(client, tick);
                return false;
            }
            return true;
        }

        protected override void ReceiveProcessed(EndPoint remotePoint, ref bool isSleep)
        {
        }

        protected override bool IsInternalCommand(Player client, RPCModel model)
        {
            if (model.cmd == NetCmd.Connect | model.cmd == NetCmd.Broadcast)
                return true;
            return false;
        }

        protected override void SendByteData(Player client, ISegment buffer)
        {
            if (!client.Client.Connected)
                return;
            if (buffer.Count <= frame)//解决长度==6的问题(没有数据)
                return;
            if (client.Client.Poll(0, SelectMode.SelectWrite))
            {
                sendAmount++;
                sendCount += buffer.Count;
                client.Session.Send(buffer.Buffer, buffer.Offset, buffer.Count);
            }
            else
            {
                client.WindowFullError++;
            }
        }

        protected override void OnCheckPerSecond(Player client)
        {
            base.OnCheckPerSecond(client);
            if (client.WindowFullError > 0)
            {
                Debug.LogError($"[{client}]发送窗口已满,等待对方接收中! {client.WindowFullError}/秒");
                client.WindowFullError = 0;
            }
            if (client.DataSizeError > 0)
            {
                Debug.LogError($"[{client}]数据被拦截修改或数据量太大, 如果想传输大数据, 请设置PackageSize属性! {client.DataSizeError}/秒");
                client.DataSizeError = 0;
            }
        }
    }

    public class WebServerNew : WebServerNew<WebPlayerNew, NetScene<WebPlayerNew>>
    {
    }
}
