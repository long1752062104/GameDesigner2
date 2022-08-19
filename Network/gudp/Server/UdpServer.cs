﻿namespace Net.Server
{
    using Net.Share;
    using global::System;
    using global::System.Net;
    using global::System.Net.Sockets;
    using global::System.Threading;
    using Debug = Event.NDebug;
    using Net.System;

    /// <summary>
    /// Udp网络服务器
    /// <para>Player:当有客户端连接服务器就会创建一个Player对象出来, Player对象和XXXClient是对等端, 每当有数据处理都会通知Player对象. </para>
    /// <para>Scene:你可以定义自己的场景类型, 比如帧同步场景处理, mmorpg场景什么处理, 可以重写Scene的Update等等方法实现每个场景的更新和处理. </para>
    /// </summary>
    public class UdpServer<Player, Scene> : ServerBase<Player, Scene> where Player : NetPlayer, new() where Scene : NetScene<Player>, new()
    {
        /// <summary>
        /// 启动服务器
        /// </summary>
        /// <param name="port">端口</param>
        public override void Start(ushort port = 6666)
        {
            if (Server != null)//如果服务器套接字已创建
                throw new Exception("服务器已经运行，不可重新启动，请先关闭后在重启服务器");
            Port = port;
            RegisterEvent();
            Debug.BindLogAll(Log);
            OnStartingHandle();
            if (Instance == null)
                Instance = this;
            AddRpcHandle(this, true, false);
            Server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, port);
            Server.Bind(ip);
#if !UNITY_ANDROID && WINDOWS//在安卓启动服务器时忽略此错误
            uint IOC_IN = 0x80000000;
            uint IOC_VENDOR = 0x18000000;
            uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
            Server.IOControl((int)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);//udp远程关闭现有连接方案
#endif
            IsRunServer = true;
            Thread proRevd = new Thread(ProcessReceive) { IsBackground = true, Name = "ProcessReceive" };
            proRevd.Start();
            Thread send = new Thread(SendDataHandle) { IsBackground = true, Name = "SendDataHandle" };
            send.Start();
            Thread suh = new Thread(SceneUpdateHandle) { IsBackground = true, Name = "SceneUpdateHandle" };
            suh.Start();
            int id = 0;
            taskIDs[id++] = ThreadManager.Invoke("DataTrafficThread", 1f, DataTrafficHandler);
            taskIDs[id++] = ThreadManager.Invoke("SingleHandler", SingleHandler);
            taskIDs[id++] = ThreadManager.Invoke("SyncVarHandler", SyncVarHandler);
            taskIDs[id++] = ThreadManager.Invoke("CheckHeartHandler", HeartInterval, CheckHeartHandler, true);
            for (int i = 0; i < MaxThread; i++)
            {
                var rcvQueue = new QueueSafe<RevdDataBuffer>();
                RcvQueues.Add(rcvQueue);
                var rcv = new Thread(RcvDataHandle) { IsBackground = true, Name = "RcvDataHandle" + i };
                rcv.Start(rcvQueue);
                threads.Add("RcvDataHandle" + i, rcv);
            }
            threads.Add("ProcessReceive", proRevd);
            threads.Add("SendDataHandle", send);
            threads.Add("SceneUpdateHandle", suh);
            var scene = OnAddDefaultScene();
            MainSceneName = scene.Key;
            scene.Value.Name = MainSceneName;
            Scenes.TryAdd(scene.Key, scene.Value);
            scene.Value.onSerializeOptHandle = OnSerializeOpt;
            OnStartupCompletedHandle();
#if WINDOWS
            Win32KernelAPI.timeBeginPeriod(1);
#endif
            InitUserID();
        }

        protected virtual void ProcessReceive()
        {
            EndPoint remotePoint = Server.LocalEndPoint;
            while (IsRunServer)
            {
                try
                {
                    if (!Server.Poll(0, SelectMode.SelectRead))
                    {
                        Thread.Sleep(1);
                        continue;
                    }
                    var buffer = BufferPool.Take();
                    buffer.Count = Server.ReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref remotePoint);
                    receiveCount += buffer.Count;
                    receiveAmount++;
                    ReceiveProcessed(remotePoint, buffer, false);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.ToString());
                }
            }
        }

        protected override void AcceptHander(Player client)
        {
            client.Gcp = new Plugins.GcpKernel();
            client.Gcp.MTU = (ushort)MTU;
            client.Gcp.RTO = RTO;
            client.Gcp.MTPS = MTPS;
            client.Gcp.RemotePoint = client.RemotePoint;
            client.Gcp.OnSender += (bytes) => {
                Send(client, NetCmd.ReliableTransport, bytes);
            };
        }

        protected override void SendRTDataHandle(Player client, QueueSafe<RPCModel> rtRPCModels)
        {
            int count = rtRPCModels.Count;
            if (count <= 0)
                goto J;
            if (client.Gcp.HasSend())
                goto J;
            if (count > PackageLength)
                count = PackageLength;
            var stream = BufferPool.Take();
            WriteDataBody(client, ref stream, rtRPCModels, count, true);
            client.Gcp.Send(stream.ToArray(true));
            J: client.Gcp.Update();
        }
    }

    /// <summary>
    /// 默认udp服务器，当不需要处理Player对象和Scene对象时可使用
    /// </summary>
    public class UdpServer : UdpServer<NetPlayer, DefaultScene>
    {
#if UDPTEST
        protected override void ProcessReceive()
        {
        }
        internal void ReceiveTest(byte[] bytes, EndPoint remotePoint) 
        {
            var buffer = new Segment(bytes, false);
            receiveCount += buffer.Count;
            receiveAmount++;
            ReceiveProcessed(remotePoint, buffer, false);
        }
        internal void DataHandlerTest(byte[] bytes, EndPoint remotePoint)
        {
            DataHandle(AllClients[remotePoint], bytes);
        }
        protected override void SendByteData(NetPlayer client, byte[] buffer, bool reliable)
        {
            if (buffer.Length == frame)//解决长度==6的问题(没有数据)
                return;
            if (buffer.Length >= 65507)
            {
                Debug.LogError($"[{client.RemotePoint}][{client.UserID}] 数据太大! 请使用SendRT");
                return;
            }
            (Net.Client.UdpClient.Instance as Net.Client.UdpClient).ReceiveTest(buffer);
            sendAmount++;
            sendCount += buffer.Length;
        }
#endif
    }
}