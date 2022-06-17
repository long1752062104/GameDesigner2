﻿namespace Net.Server
{
    using Net.Share;
    using global::System;
    using global::System.Linq;
    using global::System.Net;
    using global::System.Net.Sockets;
    using global::System.Threading;
    using Debug = Event.NDebug;
    using Net.System;
    using global::System.Security.Cryptography;
    using Net.Helper;

    /// <summary>
    /// TCP服务器类型
    /// 第三版本 2020.9.14
    /// <para>Player:当有客户端连接服务器就会创建一个Player对象出来, Player对象和XXXClient是对等端, 每当有数据处理都会通知Player对象. </para>
    /// <para>Scene:你可以定义自己的场景类型, 比如帧同步场景处理, mmorpg场景什么处理, 可以重写Scene的Update等等方法实现每个场景的更新和处理. </para>
    /// </summary>
    public class TcpServer<Player, Scene> : ServerBase<Player, Scene> where Player : NetPlayer, new() where Scene : NetScene<Player>, new()
    {
        /// <summary>
        /// tcp数据长度(4) + 1CRC协议 = 5
        /// </summary>
        protected override byte frame { get; set; } = 5;
        public override bool MD5CRC
        {
            get => md5crc;
            set
            {
                md5crc = value;
                if (value)
                    frame = 5 + 16;
                else
                    frame = 5;
            }
        }
        public override byte HeartLimit { get; set; } = 60;//tcp 2分钟检测一次

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
            Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);//---TCP协议
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, port);//IP端口设置
            Server.NoDelay = true;
            Server.Bind(ip);//绑定UDP IP端口
            Server.Listen(LineUp);
            IsRunServer = true;
            Thread proAcc = new Thread(ProcessAcceptConnect) { IsBackground = true, Name = "ProcessAcceptConnect" };
            proAcc.Start();
            Thread proRevd = new Thread(ProcessReceive) { IsBackground = true, Name = "ProcessReceive" };
            proRevd.Start();
            Thread send = new Thread(SendDataHandle) { IsBackground = true, Name = "SendDataHandle" };
            send.Start();
            Thread suh = new Thread(SceneUpdateHandle) { IsBackground = true, Name = "SceneUpdateHandle" };
            suh.Start();
            ThreadManager.Invoke("DataTrafficThread", 1f, DataTrafficHandler);
            ThreadManager.Invoke("SingleHandler", SingleHandler);
            ThreadManager.Invoke("SyncVarHandler", SyncVarHandler);
            ThreadManager.Invoke("CheckHeartHandler", HeartInterval, CheckHeartHandler, true);
            for (int i = 0; i < MaxThread / 2; i++)
            {
                QueueSafe<RevdDataBuffer> revdQueue = new QueueSafe<RevdDataBuffer>();
                RevdQueues.Add(revdQueue);
                Thread revd = new Thread(RevdDataHandle) { IsBackground = true, Name = "RevdDataHandle" + i };
                revd.Start(revdQueue);
                threads.Add("RevdDataHandle" + i, revd);
            }
            threads.Add("ProcessAcceptConnect", proAcc);
            threads.Add("ProcessReceiveFrom", proRevd);
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

        private void ProcessAcceptConnect()
        {
            while (IsRunServer)
            {
                try
                {
                    Socket socket = Server.Accept();
                    Player client = new Player();
                    client.Client = socket;
                    client.TcpRemoteEndPoint = socket.RemoteEndPoint;
                    client.RemotePoint = socket.RemoteEndPoint;
                    client.isDispose = false;
                    client.CloseSend = false;
                    UserIDStack.TryPop(out int uid);
                    client.UserID = uid;
                    client.PlayerID = uid.ToString();
                    client.Name = uid.ToString();
                    client.stackStream = BufferStreamShare.Take();
                    Interlocked.Increment(ref ignoranceNumber);
                    var buffer = BufferPool.Take(50);
                    buffer.Write(client.UserID);
                    buffer.Write(client.PlayerID);
                    SendRT(client, NetCmd.Identify, buffer.ToArray(true));
                    client.revdQueue = RevdQueues[threadNum];
                    if (++threadNum >= RevdQueues.Count)
                        threadNum = 0;
                    AllClients.TryAdd(socket.RemoteEndPoint, client);//之前放在上面, 由于接收线程并行, 还没赋值revdQueue就已经接收到数据, 导致提示内存池泄露
                    OnHasConnectHandle(client);
                    if (AllClients.Count >= OnlineLimit + LineUp)
                    {
                        SendRT(client, NetCmd.ServerFull, new byte[0]);
                        Invoke(1f, () => {
                            client.Client.Close();
                        });
                    }
                    else if (AllClients.Count > OnlineLimit)
                    {
                        QueueUp.Enqueue(client);
                        client.QueueUpCount = QueueUp.Count;
                        var segment = BufferPool.Take(8);
                        segment.Write(QueueUp.Count);
                        segment.Write(client.QueueUpCount);
                        SendRT(client, NetCmd.QueueUp, segment.ToArray(true, true));
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log($"接受异常:{ex}");
                }
            }
        }

        private void ProcessReceive()
        {
            Player[] allClients = new Player[0];
            while (IsRunServer)
            {
                try
                {
                    Thread.Sleep(1);
                    if (allClients.Length != AllClients.Count)
                        allClients = AllClients.Values.ToArray();
                    for (int i = 0; i < allClients.Length; i++)
                    {
                        Player client = allClients[i];
                        if (!client.Client.Connected)
                            continue;
                        if (client.Client.Poll(0, SelectMode.SelectRead))
                        {
                            var segment = BufferPool.Take(65507);
                            segment.Count = client.Client.Receive(segment, 0, segment.Length, SocketFlags.None, out SocketError error);
                            if (error != SocketError.Success)
                            {
                                BufferPool.Push(segment);
                                continue;
                            }
                            if (segment.Count == 0)
                            {
                                BufferPool.Push(segment);
                                continue;
                            }
                            receiveCount += segment.Count;
                            receiveAmount++;
                            client.revdQueue.Enqueue(new RevdDataBuffer() { client = client, buffer = segment, tcp_udp = true });
                        }
                    }
                    revdLoopNum++;
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.ToString());
                }
            }
        }

        protected override void RevdDataHandle(object state)
        {
            var revdQueue = state as QueueSafe<RevdDataBuffer>;
            while (IsRunServer)
            {
                try
                {
                    int count = revdQueue.Count;
                    if (count <= 0)
                    {
                        Thread.Sleep(1);
                        continue;
                    }
                    while (count > 0)
                    {
                        count--;//避免崩错. 先--
                        if (revdQueue.TryDequeue(out RevdDataBuffer revdData))
                        {
                            ResolveBuffer(revdData.client as Player, ref revdData.buffer);
                            BufferPool.Push(revdData.buffer);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("处理异常:" + ex);
                }
            }
        }

        protected override bool IsInternalCommand(Player client, RPCModel model)
        {
            if (model.cmd == NetCmd.Connect | model.cmd == NetCmd.Broadcast)
                return true;
            return false;
        }

        protected override byte[] PackData(Segment stream)
        {
            stream.Flush();
            if (MD5CRC)
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                var head = frame;
                byte[] retVal = md5.ComputeHash(stream, head, stream.Count - head);
                EncryptHelper.ToEncrypt(Password, retVal);
                int len = stream.Count - head;
                var lenBytes = BitConverter.GetBytes(len);
                byte crc = CRCHelper.CRC8(lenBytes, 0, lenBytes.Length);
                stream.Position = 0;
                stream.Write(lenBytes, 0, 4);
                stream.WriteByte(crc);
                stream.Write(retVal, 0, retVal.Length);
                stream.Position = len + head;
            }
            else
            {
                int len = stream.Count - frame;
                var lenBytes = BitConverter.GetBytes(len);
                byte crc = CRCHelper.CRC8(lenBytes, 0, lenBytes.Length);
                stream.Position = 0;
                stream.Write(lenBytes, 0, 4);
                stream.WriteByte(crc);
                stream.Position = len + frame;
            }
            return stream.ToArray();
        }

        protected override void SendRTDataHandle(Player client, QueueSafe<RPCModel> rtRPCModels)
        {
            SendDataHandle(client, rtRPCModels, true);
        }

        protected override void SendByteData(Player client, byte[] buffer, bool reliable)
        {
            if (!client.Client.Connected)
                return;
            if (buffer.Length <= frame)//解决长度==6的问题(没有数据)
                return;
            if (client.Client.Poll(1, SelectMode.SelectWrite))
            {
                int count1 = client.Client.Send(buffer, 0, buffer.Length, SocketFlags.None, out SocketError error);
                if (error != SocketError.Success | count1 <= 0)
                {
                    OnSendErrorHandle?.Invoke(client, buffer, true);
                    return;
                }
                sendAmount++;
                sendCount += buffer.Length;
            }
            else
            {
                Debug.LogError($"[{client.RemotePoint}][{client.UserID}]发送缓冲列表已经超出限制!");
            }
        }

        protected override void HeartHandle()
        {
            foreach (var client in AllClients)
            {
                if (client.Value == null)
                    continue;
                if (!client.Value.Client.Connected)
                {
                    RemoveClient(client.Value);
                    continue;
                }
                if (client.Value.heart > HeartLimit * 5)
                {
                    Debug.LogWarning($"{client.Value}:冗余连接!");
                    RemoveClient(client.Value);
                    continue;
                }
                client.Value.heart++;
                if (RTOMode == RTOMode.Variable & OnPingCallback != null)
                    Ping(client.Value);
                if (client.Value.heart <= HeartLimit)//确认心跳包
                    continue;
                SendRT(client.Value, NetCmd.SendHeartbeat, new byte[0]);//保活连接状态
            }
        }
    }

    /// <summary>
    /// 默认tcp服务器，当不需要处理Player对象和Scene对象时可使用
    /// </summary>
    public class TcpServer : TcpServer<NetPlayer, DefaultScene>
    {
    }
}