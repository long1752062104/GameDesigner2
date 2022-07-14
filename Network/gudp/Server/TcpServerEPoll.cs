namespace Net.Server
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
    using global::System.Collections.Generic;
    using global::System.Runtime.InteropServices;
    using Net.Event;

    /// <summary>
    /// TCP EPoll服务器类型
    /// 第三版本 2020.9.14
    /// <para>Player:当有客户端连接服务器就会创建一个Player对象出来, Player对象和XXXClient是对等端, 每当有数据处理都会通知Player对象. </para>
    /// <para>Scene:你可以定义自己的场景类型, 比如帧同步场景处理, mmorpg场景什么处理, 可以重写Scene的Update等等方法实现每个场景的更新和处理. </para>
    /// </summary>
    public class TcpServerEpoll<Player, Scene> : ServerBase<Player, Scene> where Player : NetPlayer, new() where Scene : NetScene<Player>, new()
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
            Server.Blocking = false;// 设置非阻塞
            Server.Bind(ip);//绑定UDP IP端口
            Server.Listen(LineUp);
            IsRunServer = true;
            Thread epoll = new Thread(EpollLoop) { IsBackground = true, Name = "EPollLoop" };
            epoll.Start();
            Thread suh = new Thread(SceneUpdateHandle) { IsBackground = true, Name = "SceneUpdateHandle" };
            suh.Start();
            ThreadManager.Invoke("DataTrafficThread", 1f, DataTrafficHandler);
            ThreadManager.Invoke("SingleHandler", SingleHandler);
            ThreadManager.Invoke("SyncVarHandler", SyncVarHandler);
            ThreadManager.Invoke("CheckHeartHandler", HeartInterval, CheckHeartHandler, true);
            threads.Add("EpollLoop", epoll);
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

        protected unsafe void EpollLoop()
        {
            var online = new FastListSafe<Player>();
            var ptrs = new FastListSafe<IntPtr>() { (IntPtr)1, Server.Handle };
            var clientDict = new Dictionary<IntPtr, Player>();
            var eventLoop = new TimerEvent();
            eventLoop.AddEvent("心跳计时器", HeartInterval, () =>
            {
                Player client = null;
                for (int i = 0; i < online.Count; i++)
                {
                    client = online[i];
                    if (client == null)
                        continue;
                    if (!client.Client.Connected)
                    {
                        RemoveClient(client);
                        continue;
                    }
                    if (client.heart > HeartLimit * 5)
                    {
                        Debug.LogWarning($"{client}:冗余连接!");
                        RemoveClient(client);
                        continue;
                    }
                    client.heart++;
                    if (RTOMode == RTOMode.Variable & OnPingCallback != null)
                        Ping(client);
                    if (client.heart <= HeartLimit)//确认心跳包
                        continue;
                    SendRT(client, NetCmd.SendHeartbeat, new byte[0]);//保活连接状态
                }
                return true;
            });
            while (IsRunServer)
            {
                try
                {
                    ptrs[0] = (IntPtr)online.Count + 1;
                    for (int i = 0; i < online.Count; i++)
                    {
                        var client = online[i];
                        if (client == null)
                            continue;
                        ptrs[i + 1] = client.Client.Handle;
                        SendDirect(client);
                    }
                    var microSeconds = 1;
                    var time = new TimeValue()
                    {
                        Seconds = (int)(microSeconds / 1000000L),
                        Microseconds = (int)(microSeconds % 1000000L)
                    };
                    var num = Win32KernelAPI.select(0, ptrs.Items, null, null, ref time);
                    for (int i = 0; i < num; i++)
                    {
                        if (ptrs[i + 1] == Server.Handle)
                        {
                            Socket socket = Server.Accept();
                            Player client = new Player();
                            client.Client = socket;
                            client.TcpRemoteEndPoint = socket.RemoteEndPoint;
                            client.RemotePoint = socket.RemoteEndPoint;
                            client.isDispose = false;
                            client.CloseSend = false;
                            if (!UserIDStack.TryPop(out int uid))
                                uid = GetCurrUserID();
                            client.UserID = uid;
                            client.PlayerID = uid.ToString();
                            client.Name = uid.ToString();
                            client.stackStream = BufferStreamShare.Take();
                            Interlocked.Increment(ref ignoranceNumber);
                            var buffer = BufferPool.Take(50);
                            buffer.Write(client.UserID);
                            buffer.Write(client.PlayerID);
                            SendRT(client, NetCmd.Identify, buffer.ToArray(true));
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
                            online.Add(client);
                            ptrs.Add(socket.Handle);
                            clientDict[socket.Handle] = client;
                        }
                        else
                        {
                            var segment = BufferPool.Take(65507);
                            fixed (byte* ptr = segment.Buffer)
                            {
                                segment.Count = Win32KernelAPI.recv(ptrs[i + 1], ptr, 65507, SocketFlags.None);
                                if (segment.Count == -1)
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
                                var client = clientDict[ptrs[i + 1]];
                                ResolveBuffer(client, ref segment);
                                BufferPool.Push(segment);
                            }
                        }
                    }
                    eventLoop.UpdateEventFixed(17, true);
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
        }
    }

    /// <summary>
    /// 默认tcp服务器，当不需要处理Player对象和Scene对象时可使用
    /// </summary>
    public class TcpServerEpoll : TcpServerEpoll<NetPlayer, DefaultScene>
    {
    }
}