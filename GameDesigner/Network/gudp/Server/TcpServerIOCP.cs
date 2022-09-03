namespace Net.Server
{
    using Net.Share;
    using global::System;
    using global::System.Collections.Generic;
    using global::System.Net;
    using global::System.Net.Sockets;
    using global::System.Threading;
    using Debug = Event.NDebug;
    using Net.System;

    /// <summary>
    /// tcp 输入输出完成端口服务器
    /// <para>Player:当有客户端连接服务器就会创建一个Player对象出来, Player对象和XXXClient是对等端, 每当有数据处理都会通知Player对象. </para>
    /// <para>Scene:你可以定义自己的场景类型, 比如帧同步场景处理, mmorpg场景什么处理, 可以重写Scene的Update等等方法实现每个场景的更新和处理. </para>
    /// </summary>
    public class TcpServerIOCP<Player, Scene> : TcpServer<Player, Scene> where Player : NetPlayer, new() where Scene : NetScene<Player>, new()
    {
        /// <summary>
        /// tcp数据长度(4) + 1CRC协议 = 5
        /// </summary>
        protected override byte frame { get; set; } = 5;

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
            Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, port);
            Server.NoDelay = true;
            Server.Bind(ip);
            Server.Listen(LineUp);
            IsRunServer = true;
            AcceptHandler();
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
            threads.Add("SendDataHandle", send);
            threads.Add("SceneUpdateHandle", suh);
            var scene = OnAddDefaultScene();
            MainSceneName = scene.Key;
            scene.Value.Name = scene.Key;
            Scenes.TryAdd(scene.Key, scene.Value);
            scene.Value.onSerializeOptHandle = OnSerializeOpt;
            OnStartupCompletedHandle();
#if WINDOWS
            Win32KernelAPI.timeBeginPeriod(1);
#endif
            InitUserID();
        }

        private void AcceptHandler()
        {
            try
            {
                if (!IsRunServer)
                    return;
                if (ServerArgs == null)
                {
                    ServerArgs = new SocketAsyncEventArgs();
                    ServerArgs.Completed += OnIOCompleted;
                }
                ServerArgs.AcceptSocket = null;// 重用前进行对象清理
                if (!Server.AcceptAsync(ServerArgs))
                    OnIOCompleted(null, ServerArgs);
            }
            catch (Exception ex)
            {
                Debug.Log($"接受异常:{ex}");
            }
        }

        protected override void OnIOCompleted(object sender, SocketAsyncEventArgs args)
        {
            Socket clientSocket = null;
            switch (args.LastOperation)
            {
                case SocketAsyncOperation.Accept:
                    try
                    {
                        clientSocket = args.AcceptSocket;
                        if (clientSocket.RemoteEndPoint == null)
                            return;
                        var client = AcceptHander(clientSocket, clientSocket.RemoteEndPoint);
                        client.ReceiveArgs = new SocketAsyncEventArgs();
                        client.ReceiveArgs.UserToken = clientSocket;
                        client.ReceiveArgs.RemoteEndPoint = clientSocket.RemoteEndPoint;
                        client.ReceiveArgs.SetBuffer(new byte[ushort.MaxValue], 0, ushort.MaxValue);
                        client.ReceiveArgs.Completed += OnIOCompleted;
                        if (!clientSocket.ReceiveAsync(client.ReceiveArgs))
                            OnIOCompleted(null, client.ReceiveArgs);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex.ToString());
                    }
                    finally
                    {
                        AcceptHandler();
                    }
                    break;
                case SocketAsyncOperation.Receive:
                    clientSocket = args.UserToken as Socket;
                    int count = args.BytesTransferred;
                    if (count > 0 & args.SocketError == SocketError.Success)
                    {
                        var buffer = BufferPool.Take();
                        buffer.Count = count;
                        Buffer.BlockCopy(args.Buffer, args.Offset, buffer, 0, count);
                        receiveCount += count;
                        receiveAmount++;
                        var remotePoint = args.RemoteEndPoint;
                        if (AllClients.TryGetValue(remotePoint, out Player client1))//在线客户端  得到client对象
                        {
                            if (client1.isDispose)
                                return;
                            client1.revdQueue.Enqueue(new RevdDataBuffer() { client = client1, buffer = buffer, tcp_udp = true });
                        }
                        if (!clientSocket.Connected)
                            return;
                        if (!clientSocket.ReceiveAsync(args))
                            OnIOCompleted(null, args);
                    }
                    break;
            }
        }

        protected override void SendByteData(Player client, byte[] buffer, bool reliable)
        {
            if (!client.Client.Connected)
                return;
            if (buffer.Length <= frame)//解决长度==6的问题(没有数据)
                return;
            if (client.Client.Poll(1, SelectMode.SelectWrite))
            {
                using (var args = new SocketAsyncEventArgs()) 
                {
                    args.SetBuffer(buffer, 0, buffer.Length);
                    args.RemoteEndPoint = client.RemotePoint;
                    args.Completed += OnIOCompleted;
                    if (!client.Client.SendAsync(args))
                        OnIOCompleted(client, args);
                }
                sendAmount++;
                sendCount += buffer.Length;
            }
            else
            {
                Debug.LogError($"[{client.RemotePoint}][{client.UserID}]发送缓冲列表已经超出限制!");
            }
        }
    }

    /// <summary>
    /// 默认tcpiocp服务器，当不需要处理Player对象和Scene对象时可使用
    /// </summary>
    public class TcpServerIOCP : TcpServerIOCP<NetPlayer, DefaultScene> { }
}