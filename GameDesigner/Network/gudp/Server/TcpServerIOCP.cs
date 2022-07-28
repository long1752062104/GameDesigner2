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
            OnStartingHandle += OnStarting;
            OnStartupCompletedHandle += OnStartupCompleted;
            OnHasConnectHandle += OnHasConnect;
            OnRemoveClientHandle += OnRemoveClient;
            OnOperationSyncHandle += OnOperationSync;
            OnRevdBufferHandle += OnReceiveBuffer;
            OnReceiveFileHandle += OnReceiveFile;
            OnRevdRTProgressHandle += OnRevdRTProgress;
            OnSendRTProgressHandle += OnSendRTProgress;
            if (OnAddRpcHandle == null) OnAddRpcHandle = AddRpcInternal;//在start之前就要添加你的委托
            if (OnRemoveRpc == null) OnRemoveRpc = RemoveRpcInternal;
            if (OnRPCExecute == null) OnRPCExecute = OnRpcExecuteInternal;
            if (OnSerializeRPC == null) OnSerializeRPC = OnSerializeRpcInternal;
            if (OnDeserializeRPC == null) OnDeserializeRPC = OnDeserializeRpcInternal;
            if (OnSerializeOPT == null) OnSerializeOPT = OnSerializeOptInternal;
            if (OnDeserializeOPT == null) OnDeserializeOPT = OnDeserializeOptInternal;
            Debug.LogHandle += Log;
            Debug.LogWarningHandle += Log;
            Debug.LogErrorHandle += Log;
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
            AcceptConnect();
            Thread send = new Thread(SendDataHandle) { IsBackground = true, Name = "SendDataHandle" };
            send.Start();
            Thread suh = new Thread(SceneUpdateHandle) { IsBackground = true, Name = "SceneUpdateHandle" };
            suh.Start();
            int id = 0;
            taskIDs[id++] = ThreadManager.Invoke("DataTrafficThread", 1f, DataTrafficHandler);
            taskIDs[id++] = ThreadManager.Invoke("SingleHandler", SingleHandler);
            taskIDs[id++] = ThreadManager.Invoke("SyncVarHandler", SyncVarHandler);
            taskIDs[id++] = ThreadManager.Invoke("CheckHeartHandler", HeartInterval, CheckHeartHandler, true);
            for (int i = 0; i < MaxThread / 2; i++)
            {
                QueueSafe<RevdDataBuffer> revdQueue = new QueueSafe<RevdDataBuffer>();
                RevdQueues.Add(revdQueue);
                Thread revd = new Thread(RevdDataHandle) { IsBackground = true, Name = "RevdDataHandle" + i };
                revd.Start(revdQueue);
                threads.Add("RevdDataHandle" + i, revd);
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

        private void AcceptConnect()
        {
            try
            {
                if (!IsRunServer)
                    return;
                if (SocketAsync == null)
                {
                    SocketAsync = new SocketAsyncEventArgs();
                    SocketAsync.Completed += OnTCPIOCompleted;
                }
                SocketAsync.AcceptSocket = null;// 重用前进行对象清理
                Server.AcceptAsync(SocketAsync);
            }
            catch (Exception ex)
            {
                Debug.Log($"接受异常:{ex}");
            }
        }

        private void OnTCPIOCompleted(object sender, SocketAsyncEventArgs args)
        {
            Socket clientSocket = null;
            SocketAsyncOperation socketOpt = args.LastOperation;
        RevdData: switch (socketOpt)
            {
                case SocketAsyncOperation.Accept:
                    try
                    {
                        clientSocket = args.AcceptSocket;
                        if (clientSocket.RemoteEndPoint == null)
                            return;
                        var args1 = new SocketAsyncEventArgs();
                        args1.Completed += OnTCPIOCompleted;
                        args1.SetBuffer(new byte[65507], 0, 65507);
                        args1.UserToken = clientSocket;
                        var client = AcceptHander(clientSocket, clientSocket.RemoteEndPoint);
                        bool willRaiseEvent = clientSocket.ReceiveAsync(args1);
                        if (!willRaiseEvent)
                        {
                            socketOpt = SocketAsyncOperation.Receive;
                            goto RevdData;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex.ToString());
                    }
                    finally
                    {
                        AcceptConnect();
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
                        EndPoint remotePoint = clientSocket.RemoteEndPoint;
                        if (AllClients.TryGetValue(remotePoint, out Player client1))//在线客户端  得到client对象
                            client1.revdQueue.Enqueue(new RevdDataBuffer() { client = client1, buffer = buffer, tcp_udp = true });
                        if (!clientSocket.ReceiveAsync(args))
                            goto RevdData;
                    }
                    break;
                case SocketAsyncOperation.Send:
                    clientSocket = args.UserToken as Socket;
                    bool willRaiseEvent1 = clientSocket.ReceiveAsync(args);
                    if (!willRaiseEvent1)
                    {
                        socketOpt = SocketAsyncOperation.Receive;
                        goto RevdData;
                    }
                    break;
            }
        }

        public override void Close()
        {
            if (SocketAsync != null)
            {
                SocketAsync.Completed -= OnTCPIOCompleted;
                SocketAsync.Dispose();
                SocketAsync = null;
            }
            base.Close();
        }
    }
}