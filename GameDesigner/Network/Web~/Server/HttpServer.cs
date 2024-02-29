using System;
using System.Net;
using System.Text;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Net.Share;
using Net.System;
using Net.Helper;
using Newtonsoft_X.Json;
using WebSocketSharp.Server;
using WebSocketSharp;
using Debug = Net.Event.NDebug;
using UnityEngine;

namespace Net.Server
{
    /// <summary>
    /// http网络服务器 2020.8.25 七夕
    /// 通过JavaScript脚本, httml网页进行连接. 和 WebClient连接
    /// 客户端发送的数据请求请看Net.Share.MessageModel类定义
    /// <para>Player:当有客户端连接服务器就会创建一个Player对象出来, Player对象和XXXClient是对等端, 每当有数据处理都会通知Player对象. </para>
    /// <para>Scene:你可以定义自己的场景类型, 比如帧同步场景处理, mmorpg场景什么处理, 可以重写Scene的Update等等方法实现每个场景的更新和处理. </para>
    /// </summary>
    public class HttpServer<Player, Scene> : ServerBase<Player, Scene> where Player : HttpPlayer, new() where Scene : NetScene<Player>, new()
    {
        /// <summary>
        /// http服务器套接字
        /// </summary>
        public new WebSocketSharp.Server.HttpServer Server { get; protected set; }
        /// <summary>
        /// http连接策略, 有https和http
        /// </summary>
        public string Scheme { get; set; } = "http";
        /// <summary>
        /// 证书
        /// </summary>
        public X509Certificate2 Certificate { get; set; }
        /// <summary>
        /// Ssl类型
        /// </summary>
        public SslProtocols SslProtocols { get; set; }
        /// <summary>
        /// 文档根路径, 指定你的网站文件根路径
        /// </summary>
        public string DocumentRootPath { get; set; }

        protected override void CreateServerSocket(ushort port)
        {
            Server = new WebSocketSharp.Server.HttpServer($"{Scheme}://0.0.0.0:{port}");
            if (Scheme == "https")
            {
                if (Certificate == null)
                    Certificate = CertificateHelper.GetDefaultCertificate();
                Server.SslConfiguration.ServerCertificate = Certificate;
                Server.SslConfiguration.EnabledSslProtocols = SslProtocols;
            }
            Server.DocumentRootPath = DocumentRootPath;
            Server.OnConnect += OnConnectHandler;
            Server.OnGet += OnGetHandler;
            Server.OnPut += OnPutHandler;
            Server.OnHead += OnHeadHandler;
            Server.Start();
        }

        private void OnConnectHandler(object sender, HttpRequestEventArgs e)
        {
            //var context = WebSocket.Context;
            //var remotePoint = context.UserEndPoint;
            //client = Server.AcceptHander(null, remotePoint, WebSocket);
        }

        protected virtual void OnPutHandler(object sender, HttpRequestEventArgs e)
        {
        }

        protected virtual void OnGetHandler(object sender, HttpRequestEventArgs e)
        {
            var req = e.Request;
            var res = e.Response;
            var path = req.RawUrl;
            if (path == "/")
                path += "index.html";
            byte[] contents;
            if (!e.TryReadFile(path, out contents))
            {
                res.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }
            if (path.EndsWith(".html"))
            {
                res.ContentType = "text/html";
                res.ContentEncoding = Encoding.UTF8;
            }
            else if (path.EndsWith(".js"))
            {
                res.ContentType = "application/javascript";
                res.ContentEncoding = Encoding.UTF8;
            }
            res.ContentLength64 = contents.LongLength;
            res.Close(contents, true);
            sendAmount++;
            sendCount += contents.Length;
            Debug.Log(path);
        }

        protected virtual void OnHeadHandler(object sender, HttpRequestEventArgs e)
        {
            var req = e.Request;
            var res = e.Response;
            var path = req.RawUrl;
            if (path == "/")
                path += "index.html";
            if (!e.TryReadFileLength(path, out var length))
            {
                res.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }
            if (path.EndsWith(".html"))
            {
                res.ContentType = "text/html";
                res.ContentEncoding = Encoding.UTF8;
            }
            else if (path.EndsWith(".js"))
            {
                res.ContentType = "application/javascript";
                res.ContentEncoding = Encoding.UTF8;
            }
            res.Headers.Add("Custom-Header", length.ToString());
            res.Close();
            Debug.Log(path);
        }

        protected override void ReceiveProcessed(EndPoint remotePoint, ref bool isSleep)
        {
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
                var model1 = new RPCModel(model.cmd, model.func.GetHashCode(), model.GetPars())
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

        protected override void SendByteData(Player client, byte[] buffer)
        {
            if (buffer.Length == frame)//解决长度==6的问题(没有数据)
                return;
            client.WSClient.Send(buffer);
            sendAmount++;
            sendCount += buffer.Length;
        }

        public override void Close()
        {
            base.Close();
            Server.Stop();
        }
    }

    /// <summary>
    /// 默认http服务器，当不需要处理Player对象和Scene对象时可使用
    /// </summary>
    public class HttpServer : HttpServer<HttpPlayer, NetScene<HttpPlayer>>
    {
    }
}