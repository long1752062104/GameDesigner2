namespace Net.Server
{
    using global::System;
    using global::System.Collections.Generic;
    using global::System.Net;
    using global::System.Net.Sockets;
    using global::System.Reflection;
    using global::System.Collections.Concurrent;
    using global::System.IO;
    using Net.Event;
    using Net.Share;
    using Net.System;
    using Net.Helper;

    /// <summary>
    /// 网络玩家 - 当客户端连接服务器后都会为每个客户端生成一个网络玩家对象，(玩家对象由服务器管理) 2019.9.9
    /// <code>注意:不要试图new player出来, new出来后是没有作用的!</code>
    /// </summary>
    public partial class NetPlayer : IDisposable, IRpcHandler
    {
        /// <summary>
        /// 玩家名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Tcp套接字
        /// </summary>
        public Socket Client { get; set; }
        /// <summary>
        /// io完成端口接收对象
        /// </summary>
        public SocketAsyncEventArgs ReceiveArgs { get; set; }
        /// <summary>
        /// 存储客户端终端
        /// </summary>
        public EndPoint RemotePoint { get; set; }
        /// <summary>
        /// 此玩家所在的场景名称
        /// </summary>
        public string SceneName { get; set; } = string.Empty;
        /// <summary>
        /// 客户端玩家的标识
        /// </summary>
        public string PlayerID { get; set; } = string.Empty;
        /// <summary>
        /// 玩家所在的场景实体
        /// </summary>
        public virtual object Scene { get; set; }
        /// <summary>
        /// 远程调用方法收集
        /// </summary>
        public MyDictionary<uint, RPCMethodBody> RpcCollectDic { get; set; } = new MyDictionary<uint, RPCMethodBody>();
        /// <summary>
        /// 已经收集过的类信息
        /// </summary>
        public MyDictionary<Type, List<MemberData>> MemberInfos { get; set; } = new MyDictionary<Type, List<MemberData>>();
        /// <summary>
        /// 当前收集rpc的对象信息
        /// </summary>
        public MyDictionary<object, MemberDataList> RpcTargetHash { get; set; } = new MyDictionary<object, MemberDataList>();
        /// <summary>
        /// 字段同步信息
        /// </summary>
        public MyDictionary<ushort, SyncVarInfo> SyncVarDic { get; set; } = new MyDictionary<ushort, SyncVarInfo>();
        /// <summary>
        /// 跨线程调用任务队列
        /// </summary>
        public JobQueueHelper WorkerQueue { get; set; }
        /// <summary>
        /// 跳动的心
        /// </summary>
        public byte heart { get; set; } = 0;
        /// <summary>
        /// TCP叠包值， 0:正常 >1:叠包次数 >25:清空叠包缓存流
        /// </summary>
        internal int stacking;
        internal int stackingOffset;
        internal int stackingCount;
        /// <summary>
        /// 数据缓冲流
        /// </summary>
        internal MemoryStream BufferStream;
        /// <summary>
        /// 用户唯一身份标识
        /// </summary>
        public int UserID { get; internal set; }
        public QueueSafe<RPCModel> RpcModels = new QueueSafe<RPCModel>();
        public QueueSafe<ISegment> RevdQueue = new QueueSafe<ISegment>();
        private ThreadGroup group;
        /// <summary>
        /// 当前玩家所在的线程组对象
        /// </summary>
        public ThreadGroup Group
        {
            get => group;
            set
            {
                group?.Remove(this);
                group = value;
                group?.Add(this); //当释放后Group = null;
            }
        }
        internal int SceneHash;
        public bool Login { get; internal set; }
        public bool isDispose { get; internal set; }
        /// <summary>
        /// 是否处于连接
        /// </summary>
        public bool Connected { get; set; }
        internal MyDictionary<int, BigData> BigDataDic = new MyDictionary<int, BigData>();
        private byte[] addressBuffer;
        /// <summary>
        /// 当前排队座号
        /// </summary>
        public int QueueUpNo { get; internal set; }
        /// <summary>
        /// 是否属于排队状态
        /// </summary>
        public bool IsQueueUp => QueueUpNo > 0;
        /// <summary>
        /// GCP协议接口
        /// </summary>
        public IGcp Gcp { get; set; }
        /// <summary>
        /// 客户端连接时间
        /// </summary>
        public DateTime ConnectTime { get; set; }
        /// <summary>
        /// 断线重连等待时间
        /// </summary>
        public uint ReconnectTimeout { get; set; }
        /// <summary>
        /// 此客户端接收到的字节总量
        /// </summary>
        public long BytesReceived { get; set; }
        /// <summary>
        /// 当前客户端请求的Token, 用于客户端响应, 如果在Rpc执行方法使用异步, 则需要记录一下token再异步, 否则token会被冲掉, 导致响应token错误
        /// </summary>
        public uint Token { get; set; }
        /// <summary>
        /// 服务器对象
        /// </summary>
        public ServerBase Server { get; set; }
        /// <summary>
        /// CRC校验错误次数, 如果有错误每秒提示一次
        /// </summary>
        public int CRCError { get; set; }
        /// <summary>
        /// 发送窗口已满提示次数
        /// </summary>
        public int WindowFullError { get; set; }
        /// <summary>
        /// 数据大小错误, 数据被拦截修改或者其他问题导致错误
        /// </summary>
        public int DataSizeError { get; set; }
        /// <summary>
        /// 数据队列溢出错误, 当要发送的数据队列堆积到<see cref="ServerBase{Player, Scene}.LimitQueueCount"/> 后提示错误
        /// </summary>
        public int DataQueueOverflowError { get; set; }
        /// <summary>
        /// 大数据传输缓存最大长度错误次数, 请在<see cref="ServerBase{Player, Scene}.BigDataCacheLength"/>设置最大缓存长度
        /// </summary>
        public int BigDataCacheLengthError { get; set; }
        /// <summary>
        /// 当接收到发送的文件进度
        /// </summary>
        public Action<BigDataProgress> OnRevdFileProgress { get; set; }
        /// <summary>
        /// 当发送的文件进度
        /// </summary>
        public Action<BigDataProgress> OnSendFileProgress { get; set; }

        private int sendFileTick;

        #region 创建网络客户端(玩家)
        /// <summary>
        /// 构造网络客户端
        /// </summary>
        public NetPlayer() { }

        /// <summary>
        /// 构造网络客户端，Tcp
        /// </summary>
        /// <param name="client">客户端套接字</param>
        public NetPlayer(Socket client)
        {
            Client = client;
            RemotePoint = client.RemoteEndPoint;
        }

        /// <summary>
        /// 构造网络客户端
        /// </summary>
        /// <param name="remotePoint"></param>
        public NetPlayer(EndPoint remotePoint)
        {
            RemotePoint = remotePoint;
        }
        #endregion

        #region 客户端释放内存
        /// <summary>
        /// 析构网络客户端
        /// </summary>
        ~NetPlayer()
        {
            Dispose();
        }

        /// <summary>
        /// 释放
        /// </summary>
        public virtual void Dispose()
        {
            if (isDispose)
                return;
            isDispose = true;
            if (ReceiveArgs != null)
            {
                ReceiveArgs.Dispose();
                ReceiveArgs = null;
            }
            if (Client != null)
            {
                Client.Shutdown(SocketShutdown.Both);
                Client.Close();
            }
            BufferStream?.Close();
            BufferStream = null;
            stacking = 0;
            stackingOffset = 0;
            stackingCount = 0;
            Connected = false;
            heart = 0;
            RpcModels = new QueueSafe<RPCModel>();
            Login = false;
            addressBuffer = null;
            Gcp?.Dispose();
            Group = null;
        }
        #endregion

        #region 客户端(玩家)Rpc(远程过程调用)处理
        /// <summary>
        /// 添加远程过程调用函数,从对象进行收集
        /// </summary>
        /// <param name="append">可以重复添加rpc?</param>
        public void AddRpc(bool append = false)
        {
            AddRpc(this, append);
        }

        /// <summary>
        /// 添加远程过程调用函数,从对象进行收集
        /// </summary>
        /// <param name="target"></param>
        /// <param name="append">可以重复添加rpc?</param>
        /// <param name="onSyncVarCollect"></param>
        public void AddRpc(object target, bool append = false, Action<SyncVarInfo> onSyncVarCollect = null)
        {
            RpcHelper.AddRpc(this, target, append, null);
        }

        /// <summary>
        /// 移除网络远程过程调用函数
        /// </summary>
        /// <param name="target">移除的rpc对象</param>
        public void RemoveRpc(object target)
        {
            RpcHelper.RemoveRpc(this, target);
        }

        internal byte[] RemoteAddressBuffer()
        {
            if (addressBuffer != null)
                return addressBuffer;
            var socketAddress = RemotePoint.Serialize();
            addressBuffer = new byte[socketAddress.Size];
            for (int i = 0; i < socketAddress.Size; i++)
                addressBuffer[i] = socketAddress[i];
            return addressBuffer;
        }
        #endregion

        #region 客户端数据处理函数
        /// <summary>
        /// 当未知客户端发送数据请求，返回<see langword="false"/>，不做任何事，返回<see langword="true"/>，添加到<see cref="ServerBase{Player, Scene}.Players"/>中
        /// 客户端玩家的入口点，在这里可以控制客户端是否可以进入服务器与其他客户端进行网络交互
        /// 在这里可以用来判断客户端登录和注册等等进站许可
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Obsolete("此方法存在严重漏洞, 已被弃用, 统一使用Server类的OnUnClientRequest方法处理", true)]
        public virtual bool OnUnClientRequest(RPCModel model)
        {
            return true;
        }

        /// <summary>
        /// 当web服务器未知客户端发送数据请求，返回<see langword="false"/>，不做任何事，返回<see langword="true"/>，添加到<see cref="ServerBase{Player, Scene}.Players"/>中
        /// 客户端玩家的入口点，在这里可以控制客户端是否可以进入服务器与其他客户端进行网络交互
        /// 在这里可以用来判断客户端登录和注册等等进站许可
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual bool OnWSUnClientRequest(MessageModel model)
        {
            return true;
        }

        /// <summary>
        /// 当接收到客户端自定义数据请求,在这里可以使用你自己的网络命令，系列化方式等进行解析网络数据。（你可以在这里使用ProtoBuf或Json来解析网络数据）
        /// </summary>
        /// <param name="model"></param>
        public virtual void OnRevdBufferHandle(RPCModel model) { }

        /// <summary>
        /// 当接收到webSocket客户端自定义数据请求,在这里可以使用你自己的网络命令，系列化方式等进行解析网络数据。（你可以在这里使用ProtoBuf或Json来解析网络数据）
        /// </summary>
        /// <param name="model"></param>
        public virtual void OnWSRevdBuffer(MessageModel model) { }

        /// <summary>
        /// 当客户端连接中断, 此时还会等待客户端重连, 如果10秒后没有重连上来就会真的断开
        /// </summary>
        public virtual void OnConnectLost() { }

        /// <summary>
        /// 当断线重连成功触发
        /// </summary>
        public virtual void OnReconnecting() { }

        /// <summary>
        /// 当服务器判定客户端为断线或连接异常时，移除客户端时调用
        /// </summary>
        public virtual void OnRemoveClient() { }

        /// <summary>
        /// 当执行Rpc(远程过程调用函数)时, 提高性能可重写此方法进行指定要调用的函数
        /// </summary>
        /// <param name="model"></param>
        public virtual void OnRpcExecute(RPCModel model) => RpcHelper.Invoke(this, this, model, AddRpcWorkQueue, RpcLog);

        private void RpcLog(int log, NetPlayer client, RPCModel model)
        {
            switch (log)
            {
                case 0:
                    NDebug.LogWarning($"{this} [protocol:{model.protocol}]的远程方法未被收集!请定义[Rpc(hash = {model.protocol})] void xx方法和参数, 并使用client.AddRpc方法收集rpc方法!");
                    break;
                case 1:
                    NDebug.LogWarning($"{this} [protocol={model.protocol}]服务器响应的Token={model.token}没有进行设置!");
                    break;
                case 2:
                    NDebug.LogWarning($"{this} {model}的远程方法未被收集!请定义[Rpc]void xx方法和参数, 并使用client.AddRpc方法收集rpc方法!");
                    break;
            }
        }

        private void AddRpcWorkQueue(MyDictionary<object, IRPCMethod> methods, NetPlayer client, RPCModel model)
        {
            foreach (RPCMethod rpc in methods.Values)
            {
                rpc.Invoke(model.pars);
            }
        }

        #endregion

        #region 提供简便的重写方法
        /// <summary>
        /// 当玩家登录成功初始化调用
        /// </summary>
        public virtual void OnStart()
        {
            NDebug.Log($"玩家[{Name}]登录了游戏...");
        }

        /// <summary>
        /// 当玩家更新操作
        /// </summary>
        public virtual void OnUpdate() { }

        /// <summary>
        /// 当玩家进入场景 ->场景对象在Scene属性
        /// </summary>
        public virtual void OnEnter() { }

        /// <summary>
        /// 当玩家退出场景 ->场景对象在Scene属性
        /// </summary>
        public virtual void OnExit() { }

        /// <summary>
        /// 当玩家退出登录时调用
        /// </summary>
        public virtual void OnSignOut() { }

        /// <summary>
        /// 当场景被移除 ->场景对象在Scene属性
        /// </summary>
        [Obsolete("此方法已不再使用, 请使用Scene的OnRemove方法")]
        public virtual void OnRemove() { }

        /// <summary>
        /// 当接收到客户端使用<see cref="Net.Client.ClientBase.AddOperation(Operation)"/>方法发送的请求时调用. 如果重写此方法, 
        /// <code>返回false, 则服务器对象类会重新把操作列表加入到场景中, 你可以重写服务器的<see cref="ServerBase{Player, Scene}.OnOperationSync(Player, OperationList)"/>方法让此方法失效</code>
        /// <code>返回true, 服务器不再把数据加入到场景列表, 认为你已经在此处把数据处理了</code>
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public virtual bool OnOperationSync(OperationList list) { return false; }

        /// <summary>
        /// 当属性同步-- 当MysqlBuild生成的类属性在客户端被修改后同步上来会调用此方法
        /// </summary>
        /// <param name="model"></param>
        public virtual void OnSyncPropertyHandler(RPCModel model) { }

        /// <summary>
        /// 此方法需要自己实现, 实现内容如下: <see langword="xxServer.Instance.RemoveClient(this);"/>
        /// </summary>
        public virtual void Close() { }
        #endregion

        #region 客户端发送请求
        public virtual void Call(uint protocol, params object[] pars)
            => Call(NetCmd.CallRpc, protocol, true, false, 0, null, pars);
        public virtual void Call(byte cmd, uint protocol, params object[] pars)
            => Call(cmd, protocol, true, false, 0, null, pars);
        public virtual void Response(uint protocol, bool serialize, uint token, params object[] pars)
            => Call(NetCmd.CallRpc, protocol, true, serialize, token, null, pars);
        public virtual void Response(uint protocol, uint token, params object[] pars)
            => Call(NetCmd.CallRpc, protocol, true, false, token, null, pars);
        public virtual void Response(byte cmd, uint protocol, uint token, params object[] pars)
            => Call(cmd, protocol, true, false, token, null, pars);

        public virtual void Call(string func, params object[] pars)
            => Call(NetCmd.CallRpc, func.CRCU32(), true, false, 0, null, pars);
        public virtual void Call(byte cmd, string func, params object[] pars)
            => Call(cmd, func.CRCU32(), true, false, 0, null, pars);
        public virtual void Response(string func, bool serialize, uint token, params object[] pars)
            => Call(NetCmd.CallRpc, func.CRCU32(), true, serialize, token, null, pars);
        public virtual void Response(string func, uint token, params object[] pars)
            => Call(NetCmd.CallRpc, func.CRCU32(), true, false, token, null, pars);
        public virtual void Response(byte cmd, string func, uint token, params object[] pars)
            => Call(cmd, func.CRCU32(), true, false, token, null, pars);

        public virtual void Call(byte cmd, uint protocol, bool serialize, uint token, params object[] pars)
            => Call(cmd, protocol, true, serialize, token, null, pars);

        public virtual void Call(byte[] buffer) => Call(NetCmd.OtherCmd, 0, false, false, 0, buffer);
        public virtual void Call(byte cmd, byte[] buffer) => Call(cmd, 0, false, false, 0, buffer);
        public void Call(byte cmd, byte[] buffer, bool kernel, bool serialize) => Call(cmd, 0, kernel, serialize, 0, buffer);
        public void Call(byte cmd, uint protocol, bool kernel, bool serialize, uint token, byte[] buffer, params object[] pars)
        {
            if (buffer != null)
            {
                Call(new RPCModel(cmd, buffer, kernel, serialize, protocol));
            }
            else
            {
                var model = new RPCModel(cmd, protocol, pars, kernel, !serialize) { token = token };
                if (serialize)
                {
                    var segment = BufferPool.Take();
                    Server.OnSerializeRPC(segment, model);
                    model.buffer = segment.ToArray(true);
                }
                Call(model);
            }
        }

        public void Call(RPCModel model)
        {
            if (!Connected)
                return;
            if (RpcModels.Count >= Server.LimitQueueCount)
            {
                DataQueueOverflowError++;
                return;
            }
            RpcModels.Enqueue(model);
        }
        #endregion

        /// <summary>
        /// 发送文件, 客户端可以使用事件<see cref="Client.ClientBase.OnReceiveFileHandle"/>来监听并处理
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="bufferSize">每次发送数据大小, 如果想最大化发送，你可以设置bufferSize参数为PackageSize - 2048</param>
        /// <returns></returns>
        public bool SendFile(string filePath, int bufferSize = 50000)
        {
            var path1 = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(path1))
            {
                NDebug.LogError($"[{this}]文件不存在! 或者文件路径字符串编码错误! 提示:可以使用Notepad++查看, 编码是ANSI,不是UTF8");
                return false;
            }
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, bufferSize);
            SendFile(NetCmd.UploadData, fileStream, Path.GetFileName(filePath), bufferSize);
            return true;
        }

        private void SendFile(byte cmd, Stream stream, string name, int bufferSize = 50000)
        {
            var data = new BigData
            {
                Id = stream.GetHashCode(),
                Stream = stream,
                Name = name,
                bufferSize = bufferSize
            };
            BigDataDic.Add(data.Id, data);
            SendFile(cmd, data.Id, data);
        }

        internal void SendFile(byte cmd, int id, BigData fileData)
        {
            var stream = fileData.Stream;
            var complete = false;
            long bufferSize = fileData.bufferSize;
            if (stream.Position + fileData.bufferSize >= stream.Length)
            {
                bufferSize = stream.Length - stream.Position;
                complete = true;
            }
            var buffer = new byte[bufferSize];
            stream.Read(buffer, 0, buffer.Length);
            var size = (fileData.Name.Length * 2) + 12;
            var segment = BufferPool.Take((int)bufferSize + size);
            var type = (byte)(fileData.Stream is FileStream ? 0 : 1);
            segment.Write(cmd);
            segment.Write(type);
            segment.Write(fileData.Id);
            segment.Write(fileData.Stream.Length);
            segment.Write(fileData.Name);
            segment.Write(buffer);
            Call(NetCmd.UploadData, segment.ToArray(true));
            if (complete)
            {
                if (OnSendFileProgress != null & type == 0)
                    OnSendFileProgress(new BigDataProgress(fileData.Name, stream.Position / (float)stream.Length * 100f, BigDataState.Complete));
                BigDataDic.Remove(id);
                fileData.Stream.Close();
            }
            else if (Environment.TickCount >= sendFileTick)
            {
                sendFileTick = Environment.TickCount + 1000;
                if (OnSendFileProgress != null & type == 0)
                    OnSendFileProgress(new BigDataProgress(fileData.Name, stream.Position / (float)stream.Length * 100f, BigDataState.Sending));
            }
        }

        /// <summary>
        /// 检查send方法的发送队列是否已到达极限, 到达极限则不允许新的数据放入发送队列, 需要等待队列消耗后才能放入新的发送数据
        /// </summary>
        /// <returns>是否可发送数据</returns>
        public bool CheckCall()
        {
            return RpcModels.Count < Server.LimitQueueCount;
        }

        public override string ToString()
        {
            return $"玩家ID:{PlayerID} 用户ID:{UserID} IP:{RemotePoint} 场景ID:{SceneName} 登录:{Login}";
        }
    }
}