﻿using System;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Net.Share;
using Net.System;
using Net.Helper;
using Newtonsoft_X.Json;
using WebSocketSharp.Server;
using WebSocketSharp;
using Debug = Net.Event.NDebug;
using System.IO;
using WebSocketSharp.Net.WebSockets;

namespace Net.Server
{
    /// <summary>
    /// web网络服务器 2020.8.25 七夕
    /// 通过JavaScript脚本, httml网页进行连接. 和 WebClient连接
    /// 客户端发送的数据请求请看Net.Share.MessageModel类定义
    /// <para>Player:当有客户端连接服务器就会创建一个Player对象出来, Player对象和XXXClient是对等端, 每当有数据处理都会通知Player对象. </para>
    /// <para>Scene:你可以定义自己的场景类型, 比如帧同步场景处理, mmorpg场景什么处理, 可以重写Scene的Update等等方法实现每个场景的更新和处理. </para>
    /// </summary>
    public class WebServer<Player, Scene> : ServerBase<Player, Scene> where Player : WebPlayer, new() where Scene : NetScene<Player>, new()
    {
        /// <summary>
        /// webSocket服务器套接字
        /// </summary>
        public new WebSocketServer Server { get; protected set; }
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

        internal class WebServerBehavior : WebSocketBehavior
        {
            internal WebServer<Player, Scene> Server;
            internal Player client;

            protected override void OnOpen()
            {
                var context = WebSocket.Context;
                var remotePoint = context.UserEndPoint;
                client = Server.AcceptHander(null, remotePoint, WebSocket);
            }

            protected override void OnMessage(MessageEventArgs e)
            {
                var buffer = e.RawData;
                var count = buffer.Length;
                Server.receiveCount += count;
                Server.receiveAmount++;
                client.BytesReceived += count;
                if (e.IsBinary)
                {
                    var segment = BufferPool.NewSegment(buffer, 0, count, false);
                    client.RevdQueue.Enqueue(segment);
                }
                else if (e.IsText)
                {
                    Server.WSRevdHandler(client, buffer, e.Data);
                }
            }

            protected override void OnClose(CloseEventArgs e)
            {
                Server.RemoveClient(client);
            }
        }

        protected override void CreateServerSocket(ushort port)
        {
            Server = new WebSocketServer($"{Scheme}://{NetPort.GetIP()}:{port}");
            if (Scheme == "wss")
            {
                if (Certificate == null)
                    Certificate = CertificateHelper.GetDefaultCertificate();
                Server.SslConfiguration.ServerCertificate = Certificate;
                Server.SslConfiguration.EnabledSslProtocols = SslProtocols;
            }
            Server.AddWebSocketService<WebServerBehavior>("/", client => client.Server = this);
            Server.Start();
        }

        protected override void ReceiveProcessed(EndPoint remotePoint, ref bool isSleep)
        {
        }

        protected override void DataCRCHandler(Player client, ISegment buffer, bool isTcp)
        {
            if (!isTcp)
            {
                ResolveBuffer(client, ref buffer);
                return;
            }
            if (!PackageAdapter.Unpack(buffer, frame, client.UserID))
                return;
            DataHandler(client, buffer);
        }

        protected override void AcceptHander(Player client, params object[] args)
        {
            client.WSClient = args[0] as WebSocket; //在这里赋值才不会在多线程并行状态下报null问题
        }

        protected override bool IsInternalCommand(Player client, RPCModel model)
        {
            if (model.cmd == NetCmd.Connect)
                return true;
            if (model.cmd == NetCmd.Broadcast)
                return true;
            return false;
        }

        private void WSRevdHandler(Player client, byte[] buffer, string message)
        {
            try
            {
                var model = JsonConvert.DeserializeObject<MessageModel>(message);
                var model1 = new RPCModel(model.cmd, model.func.CRCU32(), model.GetPars())
                {
                    buffer = buffer,
                    count = buffer.Length
                };
                DataHandler(client, model1, null);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{client}]json解析出错:" + ex);
                var model = new MessageModel(0, "error", new object[] { ex.Message });
                var jsonStr = JsonConvert.SerializeObject(model);
                client.WSClient.Send(jsonStr);
            }
        }

        protected override void SendByteData(Player client, ISegment buffer)
        {
            if (buffer.Count == frame)//解决长度==6的问题(没有数据)
                return;
            sendAmount++;
            sendCount += buffer.Count;
            client.WSClient.Send(new MemoryStream(buffer.Buffer, buffer.Offset, buffer.Count, true, true), buffer.Count);
        }

        public override void Close()
        {
            base.Close();
            Server.Stop();
        }

#if COCOS2D_JS
        protected override byte[] OnSerializeRpc(RPCModel model)
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

        protected override FuncData OnDeserializeRpc(byte[] buffer, int index, int count)
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
    }

    /// <summary>
    /// 默认web服务器，当不需要处理Player对象和Scene对象时可使用
    /// </summary>
    public class WebServer : WebServer<WebPlayer, NetScene<WebPlayer>>
    {
    }
}