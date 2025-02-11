/*版权所有（C）GDNet框架
*
*该软件按“原样”提供，不提供任何形式的明示或暗示担保，
*无论是由于软件，使用或其他方式产生的，侵权或其他形式的任何索赔，损害或其他责任，作者或版权所有者概不负责。
*
*允许任何人出于任何目的使用本框架，
*包括商业应用程序，并对其进行修改和重新发布自由
*
*受以下限制：
*
*  1. 不得歪曲本软件的来源；您不得
*声称是你写的原始软件。如果你用这个框架
*在产品中，产品文档中要确认感谢。
*  2. 更改的源版本必须清楚地标记来源于GDNet框架，并且不能
*被误传为原始软件。
*  3. 本通知不得从任何来源分发中删除或更改。
*/
namespace Net.Server
{
    using global::System;
    using global::System.Collections.Concurrent;
    using global::System.Collections.Generic;
    using global::System.IO;
    using global::System.Net;
    using global::System.Net.Sockets;
    using global::System.Reflection;
    using global::System.Text;
    using global::System.Threading;
    using global::System.Threading.Tasks;
    using Net.Share;
    using Net.System;
    using Net.Serialize;
    using Net.Helper;
    using Net.Event;
    using Net.Adapter;
#if WINDOWS
    using Microsoft.Win32;
#endif
    using Debug = Event.NDebug;

    /// <summary>
    /// 网络服务器核心基类（非泛型）
    /// </summary>
    public abstract class ServerBase
    {
        #region 属性
        /// <summary>
        /// (分布式)服务器名称
        /// </summary>
        public string Name { get; set; } = "GDNet";
        /// <summary>
        /// 分布式(集群)服务器区域名称
        /// </summary>
        public string AreaName { get; set; } = "电信1区";
        /// <summary>
        /// 服务器套接字
        /// </summary>
        public Socket Server { get; protected set; }
        /// <summary>
        /// io完成端口对象
        /// </summary>
        public SocketAsyncEventArgs ServerArgs { get; protected set; }
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
        public JobQueueHelper WorkerQueue { get; set; } = new JobQueueHelper();
        /// <summary>
        /// 服务器端口
        /// </summary>
        public ushort Port { get; protected set; }
        /// <summary>
        /// 服务器是否处于运行状态, 如果服务器套接字已经被释放则返回False, 否则返回True. 当调用Close方法后将改变状态
        /// </summary>
        public bool IsRunServer { get; set; }
        /// <summary>
        /// 网络场景同步时间(帧同步间隔), 默认每33毫秒同步一次, 一秒同步30次, 可自己设置
        /// </summary>
        public int SyncSceneTime { get; set; } = 33;
        /// <summary>
        /// 获取或设置最大可排队人数， 如果未知客户端人数超出LineUp值将不处理超出排队的未知客户端数据请求 ， 默认排队5000人
        /// </summary>
        public int LineUp { get; set; } = 5000;
        /// <summary>
        /// 允许玩家在线人数最大值（玩家在线上限）默认2000人同时在线
        /// </summary>
        public int OnlineLimit { get; set; } = 2000;
        /// <summary>
        /// 服务器主场景名称
        /// </summary>
        public string MainSceneName { get; protected set; } = "MainScene";
        /// <summary>
        /// 网络统计发送数据长度/秒
        /// </summary>
        protected int sendCount;
        /// <summary>
        /// 网络统计发送次数/秒
        /// </summary>
        protected int sendAmount;
        /// <summary>
        /// 网络统计解析次数/秒
        /// </summary>
        protected int resolveAmount;
        /// <summary>
        /// 网络统计接收次数/秒
        /// </summary>
        protected int receiveAmount;
        /// <summary>
        /// 网络统计接收长度/秒
        /// </summary>
        protected int receiveCount;
        /// <summary>
        /// 从启动到现在总流出的数据流量
        /// </summary>
        protected long outflowTotal;
        /// <summary>
        /// 从启动到现在总流入的数据流量
        /// </summary>
        protected long inflowTotal;
        /// <summary>
        /// 4个字节记录数据长度 + 1CRC校验
        /// </summary>
        public virtual byte frame { get; protected set; } = 5;
        /// <summary>
        /// 接收缓冲区最大可接收的数据包大小 (默认是5m)
        /// </summary>
        public int PackageSize { get; set; } = 1024 * 1024 * 5;
        /// <summary>
        /// 心跳时间间隔, 默认每2秒检查一次玩家是否离线, 玩家心跳确认为5次, 如果超出5次 则移除玩家客户端. 确认玩家离线总用时10秒, 
        /// 如果设置的值越小, 确认的速度也会越快. 但发送的数据也会增加. [开发调式时尽量把心跳值设置高点]
        /// </summary>
        public virtual int HeartInterval { get; set; } = 2000;
        /// <summary>
        /// <para>心跳检测次数, 默认为5次检测, 如果5次发送心跳给客户端或服务器, 没有收到回应的心跳包, 则进入断开连接处理</para>
        /// <para>当一直有数据往来时是不会发送心跳数据的, 只有当没有数据往来了, 才会进入发送心跳数据</para>
        /// </summary>
        public virtual byte HeartLimit { get; set; } = 5;
        /// <summary>
        /// 由于随机数失灵导致死循环, 所以用计数来标记用户标识 (从10000开始标记)
        /// </summary>
        public int BeginUserID { get; set; } = 10000;
        /// <summary>
        /// 当前玩家唯一标识计数
        /// </summary>
        public int CurrUserID { get; set; }
        /// <summary>
        /// 玩家唯一标识栈
        /// </summary>
        protected ConcurrentStack<int> UserIDStack = new ConcurrentStack<int>();
        /// <summary>
        /// <para>（Maxium Transmission Unit）最大传输单元, 最大传输单元为1500字节</para>
        /// <para>1.链路层：以太网的数据帧的长度为(64+18)~(1500+18)字节，其中18是数据帧的帧头和帧尾，所以数据帧的内容最大为1500字节（不包括帧头和帧尾），即MUT为1500字节</para>
        /// <para>2.网络层：IP包的首部要占用20字节，所以这里的MTU＝1500－20＝1480字节</para>
        /// <para>3.传输层：UDP包的首部要占有8字节，所以这里的MTU＝1480－8＝1472字节</para>
        /// <see langword="注意:服务器和客户端的MTU属性的值必须保持一致性,否则分包的数据将解析错误!"/> <see cref="Client.ClientBase.MTU"/>
        /// </summary>
        public int MTU { get; set; } = 1300;
        /// <summary>
        /// （Retransmission TimeOut）重传超时时间。 默认为1秒重传一次
        /// </summary>
        public int RTO { get; set; } = 1000;
        /// <summary>
        /// (Maximum traffic per second) 每秒允许传输最大流量, 默认最大每秒可以传输1m大小
        /// </summary>
        public int MTPS { get; set; } = 1024 * 1024;
        /// <summary>
        /// 流量控制模式，只有Gcp协议可用
        /// </summary>
        public FlowControlMode FlowControl { get; set; } = FlowControlMode.Normal;
        /// <summary>
        /// 并发线程数量, 发送线程和接收处理线程数量
        /// </summary>
        public int MaxThread { get; set; } = Environment.ProcessorCount;
        /// <summary>
        /// 组包数量，如果是一些小数据包，最多可以组合多少个？ 默认是组合1000个后发送
        /// </summary>
        public int PackageLength { get; set; } = 1000;
        /// <summary>
        /// 限制发送队列长度
        /// </summary>
        public int LimitQueueCount { get; set; } = ushort.MaxValue;
        /// <summary>
        /// 同步锁对象
        /// </summary>
        protected readonly object SyncRoot = new object();
        /// <summary>
        /// 断线重连等待时间内, 默认10秒内进行重连成功, 否则断线处理
        /// </summary>
        public uint ReconnectionTimeout { get; set; } = 10000;
        /// <summary>
        /// 数据包适配器
        /// </summary>
        public IPackageAdapter PackageAdapter { get; set; } = new DataAdapter();
        /// <summary>
        /// 服务器线程管理
        /// </summary>
        protected internal Dictionary<string, Thread> ServerThreads = new Dictionary<string, Thread>();
        protected int recvFileTick;
        /// <summary>
        /// 序列化适配器
        /// </summary>
        public ISerializeAdapter SerializeAdapter { get; set; }
        /// <summary>
        /// 版本号
        /// </summary>
        public int Version { get; set; } = 1;
        /// <summary>
        /// 大数据传输缓存最大值, 如果超出则被忽略 (默认最大可缓存10m数据)
        /// </summary>
        public int BigDataCacheLength { get; set; } = 1024 * 1024 * 10;
        private int sendBufferSize = ushort.MaxValue;
        /// <summary>
        /// 设置Socket的发送缓冲区大小, 也叫做窗口大小
        /// </summary>
        public int SendBufferSize
        {
            get => sendBufferSize;
            set
            {
                if (Server != null)
                    Server.SendBufferSize = value;
                sendBufferSize = value;
            }
        }
        private int receiveBufferSize = ushort.MaxValue;
        /// <summary>
        /// 设置Socket的接收缓冲区大小, 也叫做窗口大小
        /// </summary>
        public int ReceiveBufferSize
        {
            get => receiveBufferSize;
            set
            {
                if (Server != null)
                    Server.ReceiveBufferSize = value;
                receiveBufferSize = value;
            }
        }
        protected int performance;
        /// <summary>
        /// 服务器性能模式
        /// </summary>
        public Performance Performance
        {
            get => (Performance)performance;
            set => performance = (int)value;
        }
        #endregion

        #region 服务器事件处理
        /// <summary>
        /// 开始运行服务器事件
        /// </summary>
        public Action OnStartingHandle { get; set; }
        /// <summary>
        /// 服务器启动成功事件
        /// </summary>
        public Action OnStartupCompletedHandle { get; set; }

        /// <summary>
        /// 当统计网络流量时触发
        /// </summary>
        public NetworkDataTraffic OnNetworkDataTraffic { get; set; }

        /// <summary>
        /// 输出日志, 这里是输出全部日志(提示,警告,错误等信息). 如果想只输出指定的日志, 请使用NDebug类进行监听
        /// </summary>
        public Action<string> Log { get; set; }

        /// <summary>
        /// 当添加远程过程调用方法时调用， 参数1：要收集rpc特性的对象，参数2:是否异步收集rpc方法和同步字段与属性？ 参数3：如果服务器的rpc中已经有了这个对象，还可以添加进去？
        /// </summary>
        public Action<object, bool, Action<SyncVarInfo>> OnAddRpcHandle { get; set; }
        /// <summary>
        /// 当移除远程过程调用对象， 参数1：移除此对象的所有rpc方法
        /// </summary>
        public Action<object> OnRemoveRpc { get; set; }

        /// <summary>
        /// 当序列化远程过程调用方法
        /// </summary>
        public SerializeRpcDelegate OnSerializeRPC { get; set; }
        /// <summary>
        /// 当反序列化远程过程调用方法
        /// </summary>
        public DeserializeRpcDelegate OnDeserializeRPC { get; set; }
        /// <summary>
        /// 当序列化远程过程调用操作
        /// </summary>
        public SerializeOptDelegate OnSerializeOPT { get; set; }
        /// <summary>
        /// 当反序列化远程过程调用操作
        /// </summary>
        public Func<ISegment, OperationList> OnDeserializeOPT { get; set; }
        #endregion
    }

    /// <summary>
    /// 网络服务器核心基类 2023.11.13
    /// <para>Player:当有客户端连接服务器就会创建一个Player对象出来, Player对象和XXXClient是对等端, 每当有数据处理都会通知Player对象. </para>
    /// </summary>
    public abstract partial class ServerBase<Player> : ServerBase, IServerHandle<Player> where Player : NetPlayer, new()
    {
        #region 属性
        /// <summary>
        /// 登录的客户端 与<see cref="UIDClients"/>为互助字典 所添加的键值为<see cref="NetPlayer.PlayerID"/>
        /// </summary>
        public ConcurrentDictionary<object, Player> Players { get; private set; } = new ConcurrentDictionary<object, Player>();
        /// <summary>
        /// 登录的客户端 与<see cref="Players"/>为互助字典 所添加的键值为<see cref="NetPlayer.UserID"/>
        /// </summary>
        public ConcurrentDictionary<int, Player> UIDClients { get; private set; } = new ConcurrentDictionary<int, Player>();
        /// <summary>
        /// 所有客户端列表
        /// </summary>
        public ConcurrentDictionary<EndPoint, Player> AllClients { get; private set; } = new ConcurrentDictionary<EndPoint, Player>();
        /// <summary>
        /// 所有在线的客户端
        /// </summary>
        public List<Player> Clients
        {
            get
            {
                var unclients = new List<Player>();
                foreach (var client in AllClients.Values)
                    if (client.Login) unclients.Add(client);
                return unclients;
            }
        }
        /// <summary>
        /// 未知客户端连接 或 刚连接服务器还未登录账号的IP
        /// </summary>
        public List<Player> UnClients
        {
            get
            {
                var unclients = new List<Player>();
                foreach (var client in AllClients.Values)
                    if (!client.Login) unclients.Add(client);
                return unclients;
            }
        }
        /// <summary>
        /// 获取登录游戏的玩家在线人数
        /// </summary>
        public int OnlinePlayers => Players.Count;
        /// <summary>
        /// 获取未登录的玩家在线人数
        /// </summary>
        public int OnlineUnPlayers => AllClients.Count - Players.Count;
        /// <summary>
        /// 网络服务器单例
        /// </summary>
        public static ServerBase<Player> Instance { get; protected set; }
        /// <summary>
        /// 网络线程管线处理池
        /// </summary>
        protected ThreadPipeline<Player> ThreadPool = new ThreadPipeline<Player>();
        /// <summary>
        /// 排队队列
        /// </summary>
        protected ConcurrentQueue<Player> QueueUp = new ConcurrentQueue<Player>();
        #endregion

        #region 服务器事件处理

        /// <summary>
        /// 当前有客户端连接触发事件
        /// </summary>
        public Action<Player> OnHasConnectHandle { get; set; }
        /// <summary>
        /// 当添加客户端到所有在线的玩家集合中触发的事件
        /// </summary>
        public Action<Player> OnAddClientHandle { get; set; }
        /// <summary>
        /// 当接收到自定义的网络指令时处理事件
        /// </summary>
        public virtual RevdBufferHandle<Player> OnRevdBufferHandle { get; set; }
        /// <summary>
        /// 当移除客户端时触发事件
        /// </summary>
        public Action<Player> OnRemoveClientHandle { get; set; }

        /// <summary>
        /// 当客户端在时间帧发送的操作数据， 当使用客户端的<see cref="Client.ClientBase.AddOperation(Operation)"/>方法时调用
        /// </summary>
        public Action<Player, OperationList> OnOperationSyncHandle { get; set; }
        /// <summary>
        /// 当服务器发送可靠数据时, 可监听此事件显示进度值 (NetworkServer,TcpServer类无效)
        /// </summary>
        public virtual Action<Player, BigDataProgress> OnCallProgressHandle { get; set; }

        /// <summary>
        /// ping服务器回调 参数double为延迟毫秒单位 当<see cref="RTOMode"/>=<see cref="RTOMode.Variable"/>可变重传时, 内核将会每秒自动ping一次
        /// </summary>
        public Action<Player, uint> OnPingCallback;
        /// <summary>
        /// 当socket发送失败调用.参数1:玩家对象, 参数2:发送的字节数组, 参数3:发送标志(可靠和不可靠)  ->可通过<see cref="SendByteData"/>方法重新发送
        /// </summary>
        public Action<Player, ISegment> OnSendErrorHandle;

        /// <summary>
        /// 当执行调用远程过程方法时触发
        /// </summary>
        public RPCModelEvent<Player> OnRPCExecute { get; set; }

        /// <summary>
        /// 当开始下载文件时调用, 参数1(Player):下载哪个玩家上传的文件 参数2(string):客户端上传的文件名 返回值(string):开发者指定保存的文件路径(全路径名称)
        /// </summary>
        public Func<Player, string, string> OnDownloadFileHandle { get; set; }
        /// <summary>
        /// 当客户端发送的文件完成, 接收到文件后调用, 返回true:框架内部释放文件流和删除临时文件(默认) false:使用者处理
        /// </summary>
        public Func<Player, BigData, bool> OnReceiveFileHandle { get; set; }
        /// <summary>
        /// 当接收到发送的文件进度
        /// </summary>
        public Action<Player, BigDataProgress> OnRevdFileProgress { get; set; }
        /// <summary>
        /// 当发送的文件进度
        /// </summary>
        public Action<Player, BigDataProgress> OnSendFileProgress { get; set; }
        #endregion

        /// <summary>
        /// 构造网络服务器函数
        /// </summary>
        public ServerBase()
        {
        }

        #region 索引
        /// <summary>
        /// 玩家索引
        /// </summary>
        /// <param name="remotePoint"></param>
        /// <returns></returns>
        public Player this[EndPoint remotePoint] => AllClients[remotePoint];

        /// <summary>
        /// uid索引
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public Player this[int uid] => UIDClients[uid];

        /// <summary>
        /// 获得所有在线的客户端对象
        /// </summary>
        /// <returns></returns>
        public List<Player> GetClients()
        {
            var players = new List<Player>();
            foreach (Player p in AllClients.Values)
                if (p.Login)
                    players.Add(p);
            return players;
        }
        #endregion

        #region 重写方法
        /// <summary>
        /// 当未知客户端发送数据请求，返回<see langword="false"/>，不允许<see langword="unClient"/>进入服务器!，如果返回的是<see langword="true"/>，则允许<see langword="unClient"/>客户端进入服务器
        /// 同时会将<see langword="unClient"/>添加到<see cref="Players"/>和<see cref="UIDClients"/>在线字典中.
        /// <code>客户端玩家的入口点，在这里可以控制客户端是否可以进入服务器与其他客户端进行网络交互</code>
        /// 在这里可以用来判断客户端登录和注册等等进站许可 (默认是允许进入服务器)
        /// </summary>
        /// <param name="unClient">尚未登录的客户端对象</param>
        /// <param name="model">数据模型</param>
        /// <returns></returns>
        protected virtual bool OnUnClientRequest(Player unClient, RPCModel model)
        {
            return true;
        }

        /// <summary>
        /// 当开始启动服务器
        /// </summary>
        protected virtual void OnStarting() { Debug.Log("服务器开始运行..."); }

        /// <summary>
        /// 当服务器启动完毕
        /// </summary>
        protected virtual void OnStartupCompleted() { Debug.Log("服务器启动成功!"); }

        /// <summary>
        /// 当添加玩家到默认场景， 如果不想添加刚登录游戏成功的玩家进入主场景，可重写此方法让其失效
        /// </summary>
        /// <param name="client"></param>
        protected virtual void OnAddPlayerToScene(Player client) { }

        /// <summary>
        /// 当有客户端连接
        /// </summary>
        /// <param name="client">客户端套接字</param>
        protected virtual void OnHasConnect(Player client) => Debug.Log($"[{client}]连接服务器!");

        /// <summary>
        /// 当客户端连接中断, 此时还会等待客户端重连, 如果10秒后没有重连上来就会真的断开
        /// </summary>
        /// <param name="client"></param>
        protected virtual void OnConnectLost(Player client) => Debug.Log($"[{client}]连接中断!");

        /// <summary>
        /// 当断线重连成功触发
        /// </summary>
        /// <param name="client"></param>
        public virtual void OnReconnecting(Player client) => Debug.Log($"[{client}]断线重连成功");

        /// <summary>
        /// 当服务器判定客户端为断线或连接异常时，移除客户端时调用
        /// </summary>
        /// <param name="client">要移除的客户端</param>
        protected virtual void OnRemoveClient(Player client) { Debug.Log($"[{client}]断开连接!"); }

        /// <summary>
        /// 当接收到客户端自定义数据请求,在这里可以使用你自己的网络命令，系列化方式等进行解析网络数据。（你可以在这里使用ProtoBuf或Json来解析网络数据）
        /// </summary>
        /// <param name="client">当前客户端</param>
        /// <param name="model"></param>
        protected virtual void OnReceiveBuffer(Player client, RPCModel model) { }

        /// <summary>
        /// 当接收到客户端发送的文件
        /// </summary>
        /// <param name="client">当前客户端</param>
        /// <param name="fileData">文件数据</param>
        /// <returns>true:框架内部释放文件流和删除临时文件(默认) false:使用者处理</returns>
        protected virtual bool OnReceiveFile(Player client, BigData fileData) { return true; }

        /// <summary>
        /// 当接收到客户端使用<see cref="Client.ClientBase.AddOperation(Operation)"/>方法发送的请求时调用
        /// </summary>
        /// <param name="client">当前客户端</param>
        /// <param name="list">操作列表</param>
        protected virtual void OnOperationSync(Player client, OperationList list) { }

        /// <summary>
        /// 当服务器发送的大数据时, 可监听此事件显示进度值
        /// </summary>
        /// <param name="client"></param>
        /// <param name="progress"></param>
        protected virtual void OnCallProgress(Player client, BigDataProgress progress) { }

        /// <summary>
        /// 当内核序列化远程函数时调用, 如果想改变内核rpc的序列化方式, 可重写定义序列化协议
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        protected virtual void OnSerializeRpc(ISegment segment, RPCModel model) => OnSerializeRPC(segment, model);

        protected internal bool OnSerializeRpcInternal(ISegment segment, RPCModel model) => NetConvert.Serialize(segment, model);

        /// <summary>
        /// 当内核解析远程过程函数时调用, 如果想改变内核rpc的序列化方式, 可重写定义解析协议
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        protected virtual bool OnDeserializeRpc(ISegment segment, RPCModel model) => OnDeserializeRPC(segment, model);

        protected internal bool OnDeserializeRpcInternal(ISegment segment, RPCModel model) => NetConvert.Deserialize(segment, model);

        /// <summary>
        /// 当客户端使用场景转发命令会调用此方法
        /// </summary>
        /// <param name="client"></param>
        /// <param name="model"></param>
        protected virtual void OnSceneRelay(Player client, RPCModel model) { }

        /// <summary>
        /// 当客户端使用公告转发命令会调用此方法
        /// </summary>
        /// <param name="client"></param>
        /// <param name="model"></param>
        protected virtual void OnNoticeRelay(Player client, RPCModel model)
        {
            Multicast(Clients, new RPCModel(cmd: model.cmd, kernel: model.kernel, buffer: model.Buffer, serialize: false, protocol: model.protocol, token: model.token));
        }
        #endregion

        /// <summary>
        /// 运行服务器
        /// </summary>
        /// <param name="port">服务器端口号</param>
        public void Run(ushort port = 9543) => Start(port);

        /// <summary>
        /// 启动服务器
        /// </summary>
        /// <param name="port">端口</param>
        public virtual void Start(ushort port = 9543)
        {
            if (Server != null)//如果服务器套接字已创建
                throw new Exception("服务器已经运行，不可重新启动，请先关闭后在重启服务器");
            Port = port;
            RegisterEvent();
            InitUserID();//之前放最下面会出现bug! 当处于tcp协议时,当关闭服务器重启时直接有客户端连接触发Accept后, uid还没被初始化, 导致uid=0的问题
            Debug.BindLogAll(Log);
            OnStartingHandle();
            CheckInstance();
            AddRpc(this, true, null);
            CreateServerSocket(port);
            IsRunServer = true;
            StartSocketHandler();
            CreateSceneTickThread();
            CreateOtherThread();
            AddLoopEvent();
            SetProcessThreads();
            AddDefaultScene();
            OnStartupCompletedHandle();
#if WINDOWS
            Win32KernelAPI.timeBeginPeriod(1);
#endif
        }

        protected virtual void CheckInstance()
        {
            if (Instance == null)
                Instance = this;
        }

        protected virtual void AddDefaultScene() { }

        protected virtual void AddLoopEvent()
        {
            ThreadManager.Invoke("DataTrafficHandler", 1f, DataTrafficHandler);
            ThreadManager.Invoke("CheckOnLinePlayers", 1000 * 60 * 10, CheckOnLinePlayers);
        }

        protected virtual void CreateSceneTickThread() { }

        protected virtual void CreateOtherThread() { }

        protected virtual void CreateServerSocket(ushort port)
        {
        }

        protected void SetProcessThreads()
        {
            ThreadPool.MaxThread = MaxThread;
            ThreadPool.OnProcess = NetworkProcessing;
            ThreadPool.Init("NetworkProcessing_");
            ThreadPool.Start();
        }

        protected void RegisterEvent()
        {
            OnStartingHandle += OnStarting;
            OnStartupCompletedHandle += OnStartupCompleted;
            OnHasConnectHandle += OnHasConnect;
            OnRemoveClientHandle += OnRemoveClient;
            OnOperationSyncHandle += OnOperationSync;
            OnRevdBufferHandle += OnReceiveBuffer;
            //OnReceiveFileHandle += OnReceiveFile;
            OnCallProgressHandle += OnCallProgress;
            if (OnAddRpcHandle == null) OnAddRpcHandle = AddRpcInternal;//在start之前就要添加你的委托
            if (OnRemoveRpc == null) OnRemoveRpc = RemoveRpcInternal;
            if (OnRPCExecute == null) OnRPCExecute = OnRpcExecuteInternal;
            if (OnSerializeRPC == null) OnSerializeRPC = OnSerializeRpcInternal;
            if (OnDeserializeRPC == null) OnDeserializeRPC = OnDeserializeRpcInternal;
            if (OnSerializeOPT == null) OnSerializeOPT = OnSerializeOptInternal;
            if (OnDeserializeOPT == null) OnDeserializeOPT = OnDeserializeOptInternal;
        }

        /// <summary>
        /// 初始化玩家唯一标识栈
        /// </summary>
        protected void InitUserID()
        {
            UserIDStack.Clear();
            CurrUserID = BeginUserID;
        }

        /// <summary>
        /// 网络场景推动玩家同步更新处理线程, 如果想自己处理场景同步, 可重写此方法让同步失效
        /// </summary>
        protected virtual void SceneUpdateHandle() { }

        /// <summary>
        /// 流量统计线程
        /// </summary>
        protected virtual bool DataTrafficHandler()
        {
            try
            {
                outflowTotal += sendCount;
                inflowTotal += receiveCount;
                var entities = new FPSEntity[ThreadPool.Groups.Count];
                for (int i = 0; i < ThreadPool.Groups.Count; i++)
                    entities[i] = new FPSEntity(ThreadPool.Groups[i].Id, ThreadPool.Groups[i].FPS);
                OnNetworkDataTraffic?.Invoke(new Dataflow()
                {
                    sendCount = sendCount,
                    sendNumber = sendAmount,
                    receiveNumber = receiveAmount,
                    receiveCount = receiveCount,
                    resolveNumber = resolveAmount,
                    outflowTotal = outflowTotal,
                    inflowTotal = inflowTotal,
                    FPSArray = entities,
                });
            }
            catch (Exception ex)
            {
                Debug.LogError("流量统计异常:" + ex);
            }
            finally
            {
                sendCount = 0;
                sendAmount = 0;
                resolveAmount = 0;
                receiveAmount = 0;
                receiveCount = 0;
                for (int i = 0; i < ThreadPool.Groups.Count; i++)
                    ThreadPool.Groups[i].FPS = 0;
            }
            return IsRunServer;
        }

        /// <summary>
        /// 开始接收数据
        /// </summary>
        protected virtual void StartSocketHandler()
        {
        }

        protected virtual void OnIOCompleted(object sender, SocketAsyncEventArgs args)
        {
        }

        protected int GetCurrUserID()
        {
            lock (SyncRoot)
            {
                return CurrUserID++;
            }
        }

        /// <summary>
        /// 业务处理线程组
        /// </summary>
        /// <param name="group"></param>
        protected virtual void NetworkProcessing(ThreadGroup<Player> group)
        {
            var tick = (uint)Environment.TickCount;
            var heartTick = tick + (uint)HeartInterval;
            var checkPerSecondTick = tick + 1000u;
            EndPoint remotePoint = null;
            if (Server != null) //其他协议Server字段不使用
                remotePoint = Server.LocalEndPoint;
            while (IsRunServer)
            {
                try
                {
                    var isSleep = true;
                    ReceiveProcessed(remotePoint, ref isSleep);
                    tick = (uint)Environment.TickCount;
                    var isCheckHeart = false;
                    if (tick >= heartTick)
                    {
                        heartTick = tick + (uint)HeartInterval;
                        isCheckHeart = true;
                    }
                    var isCheckPerSecond = false;
                    if (tick >= checkPerSecondTick)
                    {
                        checkPerSecondTick = tick + 1000u;
                        isCheckPerSecond = true;
                    }
                    for (int i = 0; i < group.Workers.Count; i++)
                    {
                        var client = group.Workers[i];
                        if (client.isDispose)
                            continue;
                        if (!CheckIsConnected(client, tick))
                            continue;
                        if (isCheckHeart) CheckHeart(client, tick);
                        ResolveDataQueue(client, ref isSleep, tick);
                        SendDirect(client);
                        SyncVarHandler(client);
                        OnClientTick(client, tick);
                        if (isCheckPerSecond) OnCheckPerSecond(client);
                    }
                    if (isSleep)
                        Thread.Sleep(1);
                    group.FPS++;
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.ToString());
                }
            }
        }

        protected virtual void OnCheckPerSecond(Player client)
        {
            if (client.CRCError > 0)
            {
                Debug.LogError($"[{client}]CRC校验失败! {client.CRCError}/秒");
                client.CRCError = 0;
            }
            if (client.DataQueueOverflowError > 0)
            {
                Debug.LogError($"[{client}]数据缓存列表超出限制! {client.DataQueueOverflowError}/秒");
                client.DataQueueOverflowError = 0;
                var modelsDict = new MyDictionary<RPCModel, int>();
                foreach (var item in client.RpcModels)
                {
                    if (!modelsDict.ContainsKey(item))
                        modelsDict[item] = 0;
                    modelsDict[item]++;
                }
                Debug.LogError($"[{client}] 以下是堆积了哪些数据:");
                foreach (var item in modelsDict)
                    Debug.LogError($"[{client}] {item.Key} 相同数据量:{item.Value}");
            }
            if (client.BigDataCacheLengthError > 0)
            {
                Debug.LogError($"[{client}]大数据缓存超出限制,在服务器类属性BigDataCacheLength设置大数据缓存最大长度! {client.BigDataCacheLengthError}/秒");
                client.BigDataCacheLengthError = 0;
            }
            if (client.ProtocolError > 0)
            {
                Debug.LogError($"[{client}][可忽略]协议出错! {client.ProtocolError}/秒");
                client.ProtocolError = 0;
            }
        }

        protected virtual bool CheckIsConnected(Player client, uint tick)
        {
            if (!client.Connected)
            {
                if (tick >= client.ReconnectTimeout)
                    RemoveClient(client);
                return false;
            }
            return true;
        }

        protected virtual void ConnectLost(Player client, uint tick)
        {
            client.Connected = false;
            client.ReconnectTimeout = tick + ReconnectionTimeout;
            client.OnConnectLost();
            OnConnectLost(client);
        }

        protected virtual void OnClientTick(Player client, uint tick)
        {
        }

        protected virtual void ResolveDataQueue(Player client, ref bool isSleep, uint tick)
        {
            if (client.RevdQueue.TryDequeue(out var segment))
            {
                DataCRCHandler(client, segment, false);
                BufferPool.Push(segment);
            }
            client.Gcp?.Update();
        }

        protected virtual void ReceiveProcessed(EndPoint remotePoint, ref bool isSleep)
        {
            if (Server.Poll(performance, SelectMode.SelectRead))
            {
                var buffer = BufferPool.Take(ReceiveBufferSize);
                buffer.Count = Server.ReceiveFrom(buffer.Buffer, 0, buffer.Length, SocketFlags.None, ref remotePoint);
                receiveCount += buffer.Count;
                receiveAmount++;
                ReceiveProcessed(remotePoint, buffer, false);
                isSleep = false;
            }
        }

        protected virtual void ReceiveProcessed(EndPoint remotePoint, ISegment buffer, bool tcp_udp)
        {
            if (!AllClients.TryGetValue(remotePoint, out Player client))//在线客户端  得到client对象
                client = AcceptHander(null, remotePoint);
            client.heart = 0;//udp在关闭发送和接收后，客户端还是能给服务器发信息，导致服务器一直提示有客户端连接和断开连接，所以这里给他在服务器逗留，但不处理客户端任何数据，直到客户端自己不发送信息为止
            client.BytesReceived += buffer.Count;
            if (!client.Connected)
            {
                BufferPool.Push(buffer);
                return;
            }
            client.RevdQueue.Enqueue(buffer);
        }

        protected virtual void ReceiveProcessedDirect(EndPoint remotePoint, ISegment segment, bool tcp_udp)
        {
            if (!AllClients.TryGetValue(remotePoint, out Player client))//在线客户端  得到client对象
                client = AcceptHander(null, remotePoint);
            client.heart = 0;//udp在关闭发送和接收后，客户端还是能给服务器发信息，导致服务器一直提示有客户端连接和断开连接，所以这里给他在服务器逗留，但不处理客户端任何数据，直到客户端自己不发送信息为止
            if (!client.Connected)
            {
                BufferPool.Push(segment);
                return;
            }
            DataCRCHandler(client, segment, tcp_udp);
        }

        protected virtual Player AcceptHander(Socket clientSocket, EndPoint remotePoint, params object[] args)
        {
            var client = new Player();
            client.Client = clientSocket;
            client.RemotePoint = remotePoint;
            if (!UserIDStack.TryPop(out int uid))
                uid = GetCurrUserID();
            client.UserID = uid;
            client.Name = uid.ToString();
            client.BufferStream = new MemoryStream(Config.Config.BaseCapacity);
            client.ConnectTime = DateTime.Now;
            client.Connected = true;
            client.Server = this;
            AcceptHander(client, args);
            OnThreadQueueSet(client);
            SetClientIdentity(client);//此处发的identity是连接时的标识, 还不是开发者自定义的标识
            AllClients.TryAdd(remotePoint, client);//之前放在上面, 由于接收线程并行, 还没赋值revdQueue就已经接收到数据, 导致提示内存池泄露
            UIDClients.TryAdd(uid, client);//uid必须在这里添加, 不在登录成功后添加了
            OnHasConnectHandle(client);
            if (AllClients.Count >= OnlineLimit + LineUp)
            {
                Call(client, NetCmd.ServerFull, new byte[1]);
                SendDirect(client);
                client.Connected = false;
                client.QueueUpNo = int.MaxValue;
            }
            else if (AllClients.Count > OnlineLimit)
            {
                QueueUp.Enqueue(client);
                client.QueueUpNo = QueueUp.Count;
                var segment = BufferPool.Take(50);
                segment.Write(QueueUp.Count);
                segment.Write(client.QueueUpNo);
                Call(client, NetCmd.QueueUp, segment.ToArray(true));
            }
            return client;
        }

        protected virtual void OnThreadQueueSet(Player client)
        {
            client.Group = ThreadPool.SelectGroup();
        }

        protected virtual void AcceptHander(Player client, params object[] args)
        {
        }

        protected void SetClientIdentity(Player client)
        {
            var segment = BufferPool.Take(byte.MaxValue);
            segment.Write(client.UserID);
            var adapterType = string.Empty;
            if (SerializeAdapter != null)
                adapterType = SerializeAdapter.GetType().ToString();
            segment.Write(adapterType);
            segment.Write(Version);
            Call(client, NetCmd.Identify, segment.ToArray(true));
        }

        protected virtual void DataCRCHandler(Player client, ISegment buffer, bool isTcp)
        {
            if (!isTcp)
            {
                client.Gcp.Input(buffer);
                while (client.Gcp.Receive(out ISegment buffer1) > 0)
                {
                    ResolveBuffer(client, ref buffer1);
                    BufferPool.Push(buffer1);
                }
                return;
            }
            if (!PackageAdapter.Unpack(buffer, frame, client.UserID))
                return;
            DataHandler(client, buffer);
        }

        protected virtual void DataHandler(Player client, ISegment buffer)
        {
            var count = buffer.Count; //记录总长度，在解析每个rpc时不需要复制count，不要改，否则会导致反序列化rpc错误问题
            while (buffer.Position < count)
            {
                var kernelV = buffer.ReadByte();
                var kernel = kernelV == 68;
                if (!kernel & kernelV != 74)
                {
                    client.ProtocolError++;
                    break;
                }
                var cmd1 = buffer.ReadByte();
                var token = buffer.ReadUInt32();
                var dataCount = (int)buffer.ReadUInt32Fixed();
                if (buffer.Position + dataCount > count)
                    break;
                var position = buffer.Position + dataCount;
                var model = new RPCModel(cmd: cmd1, kernel: kernel, buffer: buffer.Buffer, index: buffer.Position, count: dataCount, token: token);
                if (kernel & cmd1 != NetCmd.Scene & cmd1 != NetCmd.Notice & cmd1 != NetCmd.Local)
                {
                    buffer.Count = dataCount;
                    buffer.Offset = buffer.Position;
                    var complete = OnDeserializeRpc(buffer, model);
                    if (!complete)
                        goto J;
                }
                client.Token = token;
                DataHandler(client, model, buffer);//解析协议完成
            J: buffer.Position = position;
            }
        }

        protected unsafe virtual void ResolveBuffer(Player client, ref ISegment buffer)
        {
            client.heart = 0;
            if (client.stacking > 0)
            {
                client.stacking++;
                client.BufferStream.Seek(client.stackingOffset, SeekOrigin.Begin);
                var size = buffer.Count - buffer.Position;
                client.stackingOffset += size;
                client.BufferStream.Write(buffer.Buffer, buffer.Position, size);
                if (client.stackingOffset < client.stackingCount)
                {
                    InvokeCallProgress(client, client.stackingOffset, client.stackingCount);
                    return;
                }
                var count = (int)client.BufferStream.Position;//.Length; //错误问题,不能用length, 这是文件总长度, 之前可能已经有很大一波数据
                BufferPool.Push(buffer);//要回收掉, 否则会提示内存泄露
                buffer = BufferPool.Take(count);//ref 才不会导致提示内存泄露
                client.BufferStream.Seek(0, SeekOrigin.Begin);
                client.BufferStream.Read(buffer.Buffer, 0, count);
                buffer.Count = count;
            }
            while (buffer.Position < buffer.Count)
            {
                if (buffer.Position + frame > buffer.Count)//流数据偶尔小于frame头部字节
                {
                    var position = buffer.Position;
                    var count = buffer.Count - position;
                    client.stackingOffset = count;
                    client.stackingCount = 0;
                    client.BufferStream.Seek(0, SeekOrigin.Begin);
                    client.BufferStream.Write(buffer.Buffer, position, count);
                    client.stacking++;
                    break;
                }
                var lenBytes = buffer.ReadPtr(4);
                var crcCode = buffer.ReadByte();//CRC检验索引
                var retVal = CRCHelper.CRC8(lenBytes, 0, 4);
                if (crcCode != retVal)
                {
                    client.stacking = 0;
                    client.CRCError++;
                    return;
                }
                var size = *(int*)lenBytes;
                if (size < 0 | size > PackageSize)//如果出现解析的数据包大小有问题，则不处理
                {
                    client.stacking = 0;
                    client.DataSizeError++;
                    return;
                }
                if (buffer.Position + size <= buffer.Count)
                {
                    client.stacking = 0;
                    var count = buffer.Count;//此长度可能会有连续的数据(粘包)
                    buffer.Count = buffer.Position + size;//需要指定一个完整的数据长度给内部解析
                    DataCRCHandler(client, buffer, true);
                    buffer.Count = count;//解析完成后再赋值原来的总长
                }
                else if (client.BufferStream != null) //当RemoveClient后导致stackStream=null后报错问题
                {
                    var position = buffer.Position - frame;
                    var count = buffer.Count - position;
                    client.stackingOffset = count;
                    client.stackingCount = size;
                    client.BufferStream.Seek(0, SeekOrigin.Begin);
                    client.BufferStream.Write(buffer.Buffer, position, count);
                    client.stacking++;
                    break;
                }
            }
        }

        protected virtual bool IsInternalCommand(Player client, RPCModel model)
        {
            if (model.cmd == NetCmd.Connect)
            {
                Call(client, NetCmd.Connect, new byte[1]);
                return true;
            }
            if (model.cmd == NetCmd.Broadcast)
            {
                var hostName = Dns.GetHostName();
                var iPHostEntry = Dns.GetHostEntry(hostName);
                var ipAddress = IPAddress.Any;
                foreach (var ipAdd in iPHostEntry.AddressList)
                    if (ipAdd.AddressFamily == AddressFamily.InterNetwork)
                        ipAddress = ipAdd;
                var buffer = Encoding.Unicode.GetBytes(ipAddress.ToString());
                Server.SendTo(buffer, client.RemotePoint);
                return true;
            }
            return false;
        }

        protected virtual void DataHandler(Player client, RPCModel model, ISegment segment)
        {
            client.heart = 0;
            if (IsInternalCommand(client, model))
                return;
            if (client.Login)
            {
                CommandHandler(client, model, segment);
                return;
            }
            switch (model.cmd)
            {
                case NetCmd.SendHeartbeat:
                    Call(client, NetCmd.RevdHeartbeat, new byte[1]);
                    return;
                case NetCmd.RevdHeartbeat:
                    return;
                case NetCmd.Disconnect:
                    RemoveClient(client);
                    return;
                case NetCmd.Ping:
                    return;
                case NetCmd.PingCallback:
                    return;
                case NetCmd.Identify:
                    SetClientIdentity(client);//此处发的identity是连接时的标识, 还不是开发者自定义的标识
                    break;
                case NetCmd.Download:
                    UploadDataHandler(client, segment.ReadByte(), segment.ReadInt32());
                    break;
                default:
                    if (CheckIsQueueUp(client))
                        return;
                    if (OnUnClientRequest(client, model))//当有客户端连接时,如果允许用户添加此客户端
                        LoginHandler(client);
                    break;
            }
        }

        private bool CheckIsQueueUp(Player client)
        {
            var isQueueUp = client.QueueUpNo > 0;
            if (isQueueUp)
            {
                var segment1 = BufferPool.Take(8);
                segment1.Write(QueueUp.Count);
                segment1.Write(client.QueueUpNo);
                Call(client, NetCmd.QueueUp, segment1.ToArray(true, true));
            }
            return isQueueUp;
        }

        /// <summary>
        /// 主动登录服务器, 类似OnUnClientRequest重写方法的返回值为true
        /// </summary>
        /// <param name="client"></param>
        protected void LoginHandler(Player client)
        {
            if (!client.Login)
            {
                client.Login = true;
                LoginInternal(client);
            }
        }

        private void LoginInternal(Player client)
        {
            //如果一个账号快速登录断开,再登录断开,心跳检查断线会延迟,导致无法移除掉已在游戏的客户端对象
            //如果此账号的玩家已经登录游戏, 则会先进行退出登录, 此客户端才能登录进来
            //注意:如果移除了自己,就不需要调用退出登录处理
            if (Players.TryRemove(client.PlayerID, out var client1) & client1 != client)
                SignOutInternal(client1);
            //当此玩家一直从登录到被退出登录, 再登录后PlayerID被清除了, 如果是这种情况下, 开发者也没有给PlayerID赋值, 那么默认就需要给uid得值
            if (client.PlayerID == DBNull.Value)
                client.PlayerID = client.UserID;
            Players[client.PlayerID] = client;
            client.OnStart();
            OnAddPlayerToScene(client);
            client.AddRpc(client);
            OnAddClientHandle?.Invoke(client);
            SetClientIdentity(client);//将发送登录成功的identity标识, 开发者可赋值, 必须保证是唯一的
        }

        protected virtual void CommandHandler(Player client, RPCModel model, ISegment segment)
        {
            resolveAmount++;
            switch (model.cmd)
            {
                case NetCmd.EntityRpc:
                    client.OnRpcExecute(model);
                    break;
                case NetCmd.CallRpc:
                    OnRpcExecute(client, model);
                    break;
                case NetCmd.SafeCall:
                    OnRpcExecute(client, model);
                    break;
                case NetCmd.Local:
                    client.RpcModels.Enqueue(new RPCModel(cmd: model.cmd, kernel: model.kernel, buffer: model.Buffer, serialize: false, protocol: model.protocol, token: model.token));
                    break;
                case NetCmd.Scene:
                    OnSceneRelay(client, model);
                    break;
                case NetCmd.Notice:
                    OnNoticeRelay(client, model);
                    break;
                case NetCmd.SendHeartbeat:
                    Call(client, NetCmd.RevdHeartbeat, new byte[1]);
                    break;
                case NetCmd.RevdHeartbeat:
                    client.heart = 0;
                    break;
                case NetCmd.Disconnect:
                    RemoveClient(client);
                    break;
                case NetCmd.OperationSync:
                    var operList = OnDeserializeOpt(segment);
                    OnOperationSyncHandle(client, operList);
                    break;
                case NetCmd.Ping:
                    client.RpcModels.Enqueue(new RPCModel(cmd: NetCmd.PingCallback, kernel: model.kernel, buffer: model.Buffer, serialize: false, protocol: model.protocol));
                    break;
                case NetCmd.PingCallback:
                    uint ticks = BitConverter.ToUInt32(model.buffer, model.index);
                    var delayTime = (uint)Environment.TickCount - ticks;
                    OnPingCallback?.Invoke(client, delayTime);
                    break;
                case NetCmd.P2P:
                    int uid = BitConverter.ToInt32(model.buffer, model.index);
                    if (UIDClients.TryGetValue(uid, out Player player))
                    {
                        var endPoint = player.RemotePoint as IPEndPoint;
                        var len = segment.Count;
                        segment.SetPositionLength(0);
                        segment.Write(endPoint.Address.Address);
                        segment.Write(endPoint.Port);
                        Call(client, NetCmd.P2P, segment.ToArray(false));
                        segment.Count = len;
                    }
                    break;
                case NetCmd.SyncVarP2P:
                    SyncVarHelper.SyncVarHandler(client.SyncVarDic, model.Buffer);
                    break;
                case NetCmd.UploadData:
                    DownloadDataHandler(client, segment);
                    break;
                case NetCmd.Download:
                    UploadDataHandler(client, segment.ReadByte(), segment.ReadInt32());
                    break;
                case NetCmd.Identify:
                    SetClientIdentity(client);//此处发的identity是连接时的标识, 还不是开发者自定义的标识
                    break;
                case NetCmd.SyncPropertyData:
                    client.OnSyncPropertyHandler(model);
                    break;
                default:
                    client.OnRevdBufferHandle(model);
                    OnRevdBufferHandle(client, model);
                    break;
            }
        }

        protected virtual void UploadDataHandler(Player client, byte cmd, int id)
        {
            if (client.BigDataDic.TryGetValue(id, out BigData data))
                client.SendFile(cmd, id, data);
        }

        protected virtual void DownloadDataHandler(Player client, ISegment segment)
        {
            var cmd = segment.ReadByte();
            var type = segment.ReadByte();
            var id = segment.ReadInt32();
            var length = segment.ReadInt32();
            var name = segment.ReadString();
            var buffer = segment.ReadByteArray();
            if (length > BigDataCacheLength)
            {
                client.BigDataCacheLengthError++;
                return;
            }
            if (!client.BigDataDic.TryGetValue(id, out BigData data))
            {
                data = new BigData();
                if (type == 0)
                {
                    string path;
                    if (OnDownloadFileHandle != null)
                    {
                        path = OnDownloadFileHandle(client, name);
                        var path1 = Path.GetDirectoryName(path);
                        if (!Directory.Exists(path1))
                        {
                            Debug.LogError($"[{client}]文件不存在! 或者文件路径字符串编码错误! 提示:可以使用Notepad++查看, 编码是ANSI,不是UTF8");
                            return;
                        }
                    }
                    else
                    {
                        int count = 0;
                        var downloadPath = Environment.CurrentDirectory + "/download/";
                        if (!Directory.Exists(downloadPath))
                            Directory.CreateDirectory(downloadPath);
                        do
                        {
                            count++;
                            path = downloadPath + $"{name}{count}.temp";
                        }
                        while (File.Exists(path));
                    }
                    data.Stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                }
                else
                {
                    data.Stream = new MemoryStream(length);
                }
                data.Id = id;
                data.Name = name;
                client.BigDataDic.Add(id, data);
            }
            data.Stream.Write(buffer, 0, buffer.Length);
            data.Length += buffer.Length;
            if (data.Length >= length)
            {
                client.BigDataDic.Remove(id);
                OnRevdFileProgress?.Invoke(client, new BigDataProgress(name, data.Length / (float)length * 100f, BigDataState.Complete));
                data.Stream.Position = 0;
                if (type == 0)
                {
                    var isDelete = true;
                    if (OnReceiveFileHandle != null)
                        isDelete = OnReceiveFileHandle(client, data);
                    else
                        OnReceiveFile(client, data);
                    if (isDelete)
                    {
                        data.Stream.Close();
                        File.Delete((data.Stream as FileStream).Name);
                    }
                }
                else
                {
                    buffer = new byte[length];
                    data.Stream.Read(buffer, 0, length);
                    data.Stream.Dispose();
                    var model = new RPCModel(cmd: cmd, buffer: buffer);
                    client.OnRevdBufferHandle(model);
                    OnRevdBufferHandle(client, model);
                }
            }
            else
            {
                var len = segment.Count;
                segment.SetPositionLength(0);
                segment.Write(cmd);
                segment.Write(id);
                Call(client, NetCmd.Download, segment.ToArray(false));
                segment.Count = len;
                if (Environment.TickCount >= recvFileTick)
                {
                    recvFileTick = Environment.TickCount + 1000;
                    if (type == 0)
                        OnRevdFileProgress?.Invoke(client, new BigDataProgress(name, data.Length / (float)length * 100f, BigDataState.Download));
                }
            }
        }

        protected virtual byte[] OnSerializeOpt(in OperationList list)
        {
            return OnSerializeOPT(list);
        }

        protected internal byte[] OnSerializeOptInternal(in OperationList list)
        {
            return NetConvertFast2.SerializeObject(list).ToArray(true);
        }

        protected virtual OperationList OnDeserializeOpt(ISegment segment)
        {
            return OnDeserializeOPT(segment);
        }

        protected internal OperationList OnDeserializeOptInternal(ISegment segment)
        {
            return NetConvertFast2.DeserializeObject<OperationList>(segment, false);
        }

        protected void InvokeCallProgress(Player client, int currValue, int dataCount)
        {
            float bfb = currValue / (float)dataCount * 100f;
            var progress = new BigDataProgress(bfb, BigDataState.Sending);
            OnCallProgressHandle(client, progress);
        }

        /// <summary>
        /// 立刻发送, 不需要等待内核时间 (当你要强制把客户端下线时,你还希望客户端先发送完数据后,再强制客户端退出游戏用到)
        /// </summary>
        /// <param name="client"></param>
        public virtual void SendDirect(Player client)
        {
            SendDataHandler(client, client.RpcModels);
        }

        protected virtual void SetDataHead(ISegment stream)
        {
            stream.Position = frame + PackageAdapter.HeadCount;
        }

        protected virtual void WriteDataBody(Player client, ref ISegment stream, QueueSafe<RPCModel> rPCModels, int count)
        {
            int index = 0;
            for (int i = 0; i < count; i++)
            {
                if (!rPCModels.TryDequeue(out RPCModel model))
                    continue;
                var startPos = stream.Position;
                stream.WriteByte((byte)(model.kernel ? 68 : 74));
                stream.WriteByte(model.cmd);
                stream.Write(model.token);
                var dataSizePos = stream.Position;
                stream.Position += 4;
                if (model.kernel & model.serialize)
                {
                    var completed = OnSerializeRPC(stream, model);
                    if (!completed)
                    {
                        stream.Position = startPos;
                        continue;
                    }
                }
                else if (model.buffer.Length > 0)
                {
                    var len = stream.Position + model.buffer.Length + frame;
                    if (len >= stream.Length)
                    {
                        var buffer = stream.ToArray(true);
                        stream = BufferPool.Take(len);
                        stream.Write(buffer, false);
                    }
                    stream.Write(model.buffer, false);
                }
                var currPos = stream.Position;
                stream.Position = dataSizePos;
                stream.WriteFixed((uint)(currPos - dataSizePos - 4));
                stream.Position = currPos;
                if (++index >= PackageLength | currPos + 10240 >= BufferPool.Size)
                    break;
            }
        }

        /// <summary>
        /// 重置头部数据大小, 在小数据达到<see cref="PackageLength"/>以上时会将这部分的数据先发送, 发送后还有连带的数据, 需要重置头部数据,装入大货车
        /// </summary>
        /// <param name="stream"></param>
        protected virtual void ResetDataHead(ISegment stream)
        {
            stream.SetPositionLength(frame + PackageAdapter.HeadCount);
        }

        protected virtual void SendDataHandler(Player client, QueueSafe<RPCModel> rPCModels)
        {
            var count = rPCModels.Count;//源码中Count执行也不少, 所以优化一下   这里已经取出要处理的长度
            if (count <= 0)
                return;
            var stream = BufferPool.Take(SendBufferSize);
            SetDataHead(stream);
            WriteDataBody(client, ref stream, rPCModels, count);
            PackData(stream);
            SendByteData(client, stream);
            BufferPool.Push(stream);
        }

        protected virtual void PackData(ISegment stream)
        {
            stream.Flush(false);
            SetDataHead(stream);
            PackageAdapter.Pack(stream);
            var len = stream.Count - frame;
            var lenBytes = BitConverter.GetBytes(len);
            var crc = CRCHelper.CRC8(lenBytes, 0, lenBytes.Length);
            stream.Position = 0;
            stream.Write(lenBytes, 0, 4);
            stream.WriteByte(crc);
            stream.Position += len;
        }

        protected virtual void SendByteData(Player client, ISegment buffer)
        {
            if (buffer.Count <= frame)//解决长度==5的问题(没有数据)
                return;
            sendAmount++;
            sendCount += buffer.Count;
            client.Gcp.Send(buffer);
        }

        /// <summary>
        /// 当执行Rpc(远程过程调用函数)时调用, 如果想提升服务器Rpc调用性能(默认反射调用), 可以重写此方法, 指定要调用的方法
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="model">数据模型</param>
        protected virtual void OnRpcExecute(Player client, RPCModel model)
        {
            OnRPCExecute(client, model);
        }

        protected internal void OnRpcExecuteInternal(Player client, RPCModel model) => RpcHelper.Invoke(this, client, model, AddRpcWorkQueue, RpcLog);

        private void RpcLog(int log, NetPlayer client, RPCModel model)
        {
            switch (log)
            {
                case 0:
                    Debug.LogWarning($"{client} [protocol:{model.protocol}]的远程方法未被收集!请定义[Rpc(hash = {model.protocol})] void xx方法和参数, 并使用server.AddRpc方法收集rpc方法!");
                    break;
                case 1:
                    Debug.LogWarning($"{client} [protocol={model.protocol}]服务器响应的Token={model.token}没有进行设置!");
                    break;
                case 2:
                    Debug.LogWarning($"{client} {model}的远程方法未被收集!请定义[Rpc]void xx方法和参数, 并使用server.AddRpc方法收集rpc方法!");
                    break;
            }
        }

        private void AddRpcWorkQueue(MyDictionary<object, IRPCMethod> methods, NetPlayer client, RPCModel model)
        {
            foreach (RPCMethod method in methods.Values)
            {
                try
                {
                    switch (method.cmd)
                    {
                        case NetCmd.SafeCall:
                            InvokeSafeMethod(client, method, model.pars);
                            break;
                        case NetCmd.SingleCall:
                            ThreadManager.Event.AddEvent(0f, (state) => InvokeSafeMethod(client, method, (object[])state), model.pars);
                            break;
                        case NetCmd.SafeCallAsync:
                            var workCallback = new RpcWorkParameter(client, method, model.pars);
                            global::System.Threading.ThreadPool.UnsafeQueueUserWorkItem(workCallback.RpcWorkCallback, workCallback);
                            break;
                        default:
                            method.Invoke(model.pars);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"方法:{method.method} {model} 详细信息:{ex}");
                }
            }
        }

        protected void InvokeSafeMethod(NetPlayer client, IRPCMethod method, object[] pars)
        {
            var len = pars.Length;
            var array = new object[len + 1];
            array[0] = client;
            Array.Copy(pars, 0, array, 1, len);
            method.Invoke(array);
        }

        /// <summary>
        /// 检查心跳
        /// </summary>
        /// <param name="client"></param>
        protected virtual void CheckHeart(Player client, uint tick)
        {
            if (client.heart++ < HeartLimit)
            {
                Call(client, NetCmd.SendHeartbeat, new byte[1]);
                return;
            }
            RemoveClient(client);
        }

        /// <summary>
        /// 从所有在线玩家字典中删除(移除)玩家实体
        /// </summary>
        /// <param name="player"></param>
        public void DeletePlayer(Player player) => RemoveClient(player);

        /// <summary>
        /// 从所有在线玩家字典中移除玩家实体
        /// </summary>
        /// <param name="player"></param>
        public void RemovePlayer(Player player) => RemoveClient(player);

        /// <summary>
        /// 从客户端字典中移除客户端
        /// </summary>
        /// <param name="client"></param>
        public virtual void RemoveClient(Player client)
        {
            if (!AllClients.TryRemove(client.RemotePoint, out _))//防止两次进入
                return;
            Players.TryRemove(client.PlayerID, out _);
            UIDClients.TryRemove(client.UserID, out _);
            OnRemoveClientHandle(client);
            client.OnRemoveClient();
            ExitSceneHandler(client);
            client.Dispose();
            if (client.UserID > 0)
                UserIDStack.Push(client.UserID);
            if (client.IsQueueUp)
                return;
            J: if (QueueUp.TryDequeue(out var client1))
            {
                if (client1.isDispose)
                    goto J;
                if (!client1.Connected)
                    goto J;
                client1.QueueUpNo = 0;
                Call(client1, NetCmd.QueueCancellation, new byte[1]);
            }
        }

        /// <summary>
        /// 关闭服务器
        /// </summary>
        public virtual void Close()
        {
            IsRunServer = false;
            foreach (Player client in AllClients.Values)
                client.Dispose();
            AllClients.Clear();
            Players.Clear();
            UIDClients.Clear();
            Thread.Sleep(50);//等待线程退出后再关闭套接字, 解决在revd方法出错
            if (Server != null)
            {
                Server.Dispose();
                Server.Close();
                Server = null;
            }
            if (ServerArgs != null)
            {
                ServerArgs.Dispose();
                ServerArgs = null;
                #region 清除iocp完成端口绑定事件池缓存
                var overlappedType = typeof(Overlapped);
                var overlappedDataCacheField = overlappedType.GetField("s_overlappedDataCache", BindingFlags.NonPublic | BindingFlags.Static);
                var s_overlappedDataCache = overlappedDataCacheField.GetValue(null);
                var overlappedDataCacheType = s_overlappedDataCache.GetType();
                var m_FreeListField = overlappedDataCacheType.GetField("m_FreeList", BindingFlags.NonPublic | BindingFlags.Instance);
                var m_FreeList = m_FreeListField.GetValue(s_overlappedDataCache) as ConcurrentStack<object>;
                m_FreeList.Clear();
                var m_NotGen2Field = overlappedDataCacheType.GetField("m_NotGen2", BindingFlags.NonPublic | BindingFlags.Instance);
                var m_NotGen2 = m_NotGen2Field.GetValue(s_overlappedDataCache) as List<object>;
                m_NotGen2.Clear();
                #endregion
            }
            FreeInstance();
            foreach (var item in ServerThreads)
                item.Value.Interrupt();
            ServerThreads.Clear();
            ThreadPool.Dispose();
            OnStartingHandle -= OnStarting;
            OnStartupCompletedHandle -= OnStartupCompleted;
            OnHasConnectHandle -= OnHasConnect;
            OnRemoveClientHandle -= OnRemoveClient;
            OnOperationSyncHandle -= OnOperationSync;
            OnRevdBufferHandle -= OnReceiveBuffer;
            //OnReceiveFileHandle -= OnReceiveFile;
            OnCallProgressHandle -= OnCallProgress;
            Debug.Log("服务器已关闭！");//先打印在移除事件
            Thread.Sleep(100);
            Debug.LogHandle -= Log;
            Debug.LogWarningHandle -= Log;
            Debug.LogErrorHandle -= Log;
        }

        protected virtual void FreeInstance()
        {
            if (this == Instance)//有多个服务器实例, 需要
                Instance = null;
        }

        public void Call(Player client, uint protocol, params object[] pars)
            => Call(client, NetCmd.CallRpc, protocol, true, false, 0, null, pars);
        public void Call(Player client, byte cmd, uint protocol, params object[] pars)
            => Call(client, cmd, protocol, true, false, 0, null, pars);
        public void Response(Player client, uint protocol, bool serialize, uint token, params object[] pars)
            => Call(client, NetCmd.CallRpc, protocol, true, serialize, token, null, pars);
        public void Response(Player client, uint protocol, uint token, params object[] pars)
            => Call(client, NetCmd.CallRpc, protocol, true, false, token, null, pars);
        public void Response(Player client, byte cmd, uint protocol, uint token, params object[] pars)
            => Call(client, cmd, protocol, true, false, token, null, pars);

        public void Call(Player client, string func, params object[] pars)
            => Call(client, NetCmd.CallRpc, func.CRCU32(), true, false, 0, null, pars);
        public void Call(Player client, byte cmd, string func, params object[] pars)
            => Call(client, cmd, func.CRCU32(), true, false, 0, null, pars);
        public void Response(Player client, string func, bool serialize, uint token, params object[] pars)
            => Call(client, NetCmd.CallRpc, func.CRCU32(), true, serialize, token, null, pars);
        public void Response(Player client, string func, uint token, params object[] pars)
            => Call(client, NetCmd.CallRpc, func.CRCU32(), true, false, token, null, pars);
        public void Response(Player client, byte cmd, string func, uint token, params object[] pars)
            => Call(client, cmd, func.CRCU32(), true, false, token, null, pars);

        public void Call(Player client, byte cmd, uint protocol, bool serialize, uint token, params object[] pars)
            => Call(client, cmd, protocol, true, serialize, token, null, pars);

        public void Call(Player client, byte[] buffer) => Call(client, NetCmd.OtherCmd, 0, false, false, 0, buffer);
        public void Call(Player client, byte cmd, byte[] buffer) => Call(client, cmd, 0, false, false, 0, buffer);

        public void Call(Player client, byte cmd, byte[] buffer, bool kernel, bool serialize) => Call(client, cmd, 0, kernel, serialize, 0, buffer);
        public void Call(Player client, byte cmd, uint protocol, bool kernel, bool serialize, uint token, byte[] buffer, params object[] pars)
            => client.Call(cmd, protocol, kernel, serialize, token, buffer, pars);

        /// <inheritdoc/>
        public void Multicast(IList<Player> clients, byte[] buffer)
            => Multicast(clients, NetCmd.OtherCmd, buffer);
        /// <inheritdoc/>
        public void Multicast(IList<Player> clients, byte cmd, byte[] buffer)
            => Multicast(clients, new RPCModel(cmd: cmd, kernel: false, buffer: buffer, serialize: false));
        /// <inheritdoc/>
        public void Multicast(IList<Player> clients, byte cmd, byte[] buffer, bool kernel, bool serialize)
            => Multicast(clients, new RPCModel(cmd: cmd, kernel: kernel, buffer: buffer, serialize: serialize));
        /// <inheritdoc/>
        public void Multicast(IList<Player> clients, uint protocol, params object[] pars)
            => Multicast(clients, NetCmd.CallRpc, protocol, pars);
        /// <inheritdoc/>
        public void Multicast(IList<Player> clients, byte cmd, uint protocol, params object[] pars)
            => Multicast(clients, new RPCModel(cmd: cmd, kernel: true, protocol: protocol, pars: pars));
        public void Multicast(IList<Player> clients, string func, params object[] pars)
            => Multicast(clients, NetCmd.CallRpc, func, pars);
        public void Multicast(IList<Player> clients, byte cmd, string func, params object[] pars)
            => Multicast(clients, new RPCModel(cmd: cmd, kernel: true, protocol: func.CRCU32(), pars: pars));
        public virtual void Multicast(IList<Player> clients, RPCModel model)
        {
            if (model.buffer == null)
            {
                model.serialize = false;
                var segment = BufferPool.Take();
                OnSerializeRpc(segment, model);
                model.buffer = segment.ToArray(true);
            }
            if (model.buffer.Length / MTU > LimitQueueCount)
            {
                Debug.LogError("Multocast数据太大，请分块发送!");
                return;
            }
            for (int i = 0; i < clients.Count; i++)
            {
                var client = clients[i];
                if (client == null)
                    continue;
                if (!client.Connected)
                    continue;
                if (client.RpcModels.Count >= LimitQueueCount)
                {
                    OnDataQueueOverflow(client);
                    continue;
                }
                client.RpcModels.Enqueue(model);
            }
        }

        /// <summary>
        /// 添加Rpc
        /// </summary>
        /// <param name="target">注册的对象实例</param>
        /// <param name="append">一个Rpc方法是否可以多次添加到Rpcs里面？</param>
        /// <param name="onSyncVarCollect"></param>
        public void AddRpc(object target, bool append = false, Action<SyncVarInfo> onSyncVarCollect = null)
        {
            AddRpcHandle(target, append, onSyncVarCollect);
        }

        /// <summary>
        /// 添加网络Rpc(注册远程方法)
        /// </summary>
        /// <param name="target">注册的对象实例</param>
        public void AddRpcHandle(object target)
        {
            AddRpcHandle(target, false);
        }

        /// <summary>
        /// 添加网络Rpc(注册远程方法)
        /// </summary>
        /// <param name="target">注册的对象实例</param>
        /// <param name="append">一个Rpc方法是否可以多次添加到Rpcs里面？</param>
        /// <param name="onSyncVarCollect"></param>
        public void AddRpcHandle(object target, bool append, Action<SyncVarInfo> onSyncVarCollect = null)
        {
            if (OnAddRpcHandle == null)
                OnAddRpcHandle = AddRpcInternal;
            OnAddRpcHandle(target, append, onSyncVarCollect);
        }

        protected void AddRpcInternal(object target, bool append, Action<SyncVarInfo> onSyncVarCollect = null)
        {
            RpcHelper.AddRpc(this, target, append, onSyncVarCollect);
        }

        /// <summary>
        /// 移除对象的Rpc注册
        /// </summary>
        /// <param name="target">将此对象的所有带有RPC特性的函数移除</param>
        public void RemoveRpc(object target)
        {
            if (OnRemoveRpc == null)
                OnRemoveRpc = RemoveRpcInternal;
            OnRemoveRpc(target);
        }

        protected void RemoveRpcInternal(object target)
        {
            RpcHelper.RemoveRpc(this, target);
        }

        /// <summary>
        /// playerID玩家是否在线?
        /// </summary>
        /// <param name="playerID"></param>
        /// <returns></returns>
        public virtual bool IsOnline(object playerID)
        {
            return IsOnline(playerID, out _);
        }

        /// <summary>
        /// playerID玩家是否在线? 并且如果在线则out 在线玩家的对象
        /// </summary>
        /// <param name="playerID"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public virtual bool IsOnline(object playerID, out Player client)
        {
            return Players.TryGetValue(playerID, out client);
        }

        /// <summary>
		/// 强制下线处理, 将client客户端从在线字段<see cref="Players"/>和<see cref="UIDClients"/>和<see cref="AllClients"/>字段中移除
		/// </summary>
		/// <param name="client"></param>
		public virtual void OfflineHandle(Player client)
        {
            SendDirect(client);
            RemoveClient(client);
            Debug.Log("[" + client.Name + "]被强制下线...!");
        }

        /// <summary>
        /// 退出登录, 将client客户端从在线字段<see cref="Players"/>和<see cref="UIDClients"/>字段中移除
        /// </summary>
        /// <param name="client"></param>
        public virtual void SignOut(Player client)
        {
            if (!client.Login)
                return;
            SignOutInternal(client);
        }

        protected void SignOutInternal(Player client)
        {
            SendDirect(client);
            ExitSceneHandler(client);
            OnSignOut(client);
            client.OnSignOut();
            client.Login = false;
            client.PlayerID = DBNull.Value;//此处必须清除,要不然当移除断线的账号后, 就会移除掉新登录的此账号在线字段Players
            Debug.Log("[" + client.Name + "]退出登录...!");
        }

        /// <summary>
        /// 当客户端退出登录, 如果两个账号同时登录或者在心跳时间还没到检测时另外一个玩家也登录了相同的账号, 则会强制退出上一个账号的登录
        /// </summary>
        /// <param name="client"></param>
        public virtual void OnSignOut(Player client)
        {
        }

        /// <summary>
        /// 设置心跳时间
        /// </summary>
        /// <param name="timeoutLimit">心跳检测次数, 默认检测5次</param>
        /// <param name="interval">心跳时间间隔, 每interval毫秒会检测一次</param>
        public void SetHeartTime(byte timeoutLimit, int interval)
        {
            HeartLimit = timeoutLimit;
            HeartInterval = interval;
        }

        /// <summary>
        /// ping测试网络延迟, 通过<see cref="OnPingCallback"/>事件回调
        /// </summary>
        /// <param name="client"></param>
        public void Ping(Player client)
        {
            uint tick = (uint)Environment.TickCount;
            Call(client, NetCmd.Ping, BitConverter.GetBytes(tick));
        }

        /// <summary>
        /// ping测试网络延迟, 此方法帮你监听<see cref="OnPingCallback"/>事件, 如果不使用的时候必须保证能移除委托, 建议不要用框名函数, 那样会无法移除委托
        /// </summary>
        /// <param name="client"></param>
        /// <param name="callback"></param>
        public void Ping(Player client, Action<Player, uint> callback)
        {
            uint tick = (uint)Environment.TickCount;
            Call(client, NetCmd.Ping, BitConverter.GetBytes(tick));
            OnPingCallback += callback;
        }

        internal void SetRAC(int length)
        {
            receiveCount += length;
            receiveAmount++;
        }

        /// <summary>
        /// 添加适配器
        /// </summary>
        /// <param name="adapter"></param>
        public void AddAdapter(IAdapter adapter)
        {
            if (adapter is ISerializeAdapter ser)
                AddAdapter(AdapterType.Serialize, ser);
            else if (adapter is IRPCAdapter<Player> rpc)
                AddAdapter(AdapterType.RPC, rpc);
            else if (adapter is IPackageAdapter package)
                AddAdapter(AdapterType.Package, package);
            else throw new Exception("无法识别的适配器!， 注意: IRPCAdapter<Player>是服务器的RPC适配器，IRPCAdapter是客户端适配器！");
        }

        /// <summary>
        /// 添加适配器
        /// </summary>
        /// <param name="type"></param>
        /// <param name="adapter"></param>
        public void AddAdapter(AdapterType type, IAdapter adapter)
        {
            switch (type)
            {
                case AdapterType.Serialize:
                    SerializeAdapter = (ISerializeAdapter)adapter;
                    OnSerializeRPC = SerializeAdapter.OnSerializeRpc;
                    OnDeserializeRPC = SerializeAdapter.OnDeserializeRpc;
                    OnSerializeOPT = SerializeAdapter.OnSerializeOpt;
                    OnDeserializeOPT = SerializeAdapter.OnDeserializeOpt;
                    break;
                case AdapterType.RPC:
                    var rpc = (IRPCAdapter<Player>)adapter;
                    OnAddRpcHandle = rpc.AddRpc;
                    OnRPCExecute = rpc.OnRpcExecute;
                    OnRemoveRpc = rpc.RemoveRpc;
                    break;
                case AdapterType.Package:
                    PackageAdapter = (IPackageAdapter)adapter;
                    break;
            }
        }

        /// <summary>
        /// 字段,属性同步线程
        /// </summary>
        protected virtual void SyncVarHandler(Player client)
        {
            var buffer = SyncVarHelper.CheckSyncVar(true, client.SyncVarDic);
            if (buffer != null)
                Call(client, NetCmd.SyncVarP2P, buffer);
        }

        /// <summary>
        /// 发送文件, 客户端可以使用事件<see cref="Client.ClientBase.OnReceiveFileHandle"/>来监听并处理
        /// </summary>
        /// <param name="client"></param>
        /// <param name="filePath"></param>
        /// <param name="bufferSize">每次发送数据大小</param>
        /// <returns></returns>
        public bool SendFile(Player client, string filePath, int bufferSize = 50000) => client.SendFile(filePath, bufferSize);

        /// <summary>
        /// 检查send方法的发送队列是否已到达极限, 到达极限则不允许新的数据放入发送队列, 需要等待队列消耗后才能放入新的发送数据
        /// </summary>
        /// <returns>是否可发送数据</returns>
        public bool CheckCall(Player client) => client.CheckCall();

        /// <summary>
        /// 设置攻击防护(SYN-ACK攻击)
        /// </summary>
        /// <param name="synAttackProtect">0:不开启 1:系统通过减少重传次数和延迟未连接时路由缓冲项(route cache entry)防范SYN攻击 2:(Microsoft推荐使用此值)</param>
        /// <param name="tcpMaxConnectResponseRetransmissions">确定 TCP 重新传输未应答的 SYN-ACK（连接请求确认）的次数</param>
        /// <param name="tcpMaxHalfOpen">服务器可以保持多少个连接处于半开（SYN-RCVD）状态</param>
        /// <param name="tcpMaxHalfOpenRetried">确定服务器可以在半打开 (SYN-RCVD) 状态下保持多少连接, 此条目的值应小于TCPMaxHalfOpen条目的值</param>
        /// <param name="tcpMaxPortsExhausted">指定触发 SYN 洪水攻击保护所必须超过的 TCP 连接请求数的阈值。</param>
        public virtual void SetAttackProtect(int synAttackProtect = 1, int tcpMaxConnectResponseRetransmissions = 2, int tcpMaxHalfOpen = 500, int tcpMaxHalfOpenRetried = 400, int tcpMaxPortsExhausted = 5)
        {
#if WINDOWS
            RegistryKey hklm = Registry.LocalMachine;
            RegistryKey tcpParams = hklm.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", true);
            tcpParams.SetValue("SynAttackProtect", synAttackProtect, RegistryValueKind.DWord);
            tcpParams.SetValue("TcpMaxConnectResponseRetransmissions", tcpMaxConnectResponseRetransmissions, RegistryValueKind.DWord);
            tcpParams.SetValue("TcpMaxHalfOpen", tcpMaxHalfOpen, RegistryValueKind.DWord);
            tcpParams.SetValue("TcpMaxHalfOpenRetried", tcpMaxHalfOpenRetried, RegistryValueKind.DWord);
            tcpParams.SetValue("TcpMaxPortsExhausted", tcpMaxPortsExhausted, RegistryValueKind.DWord);
            hklm.Close();
            tcpParams.Close();
#endif
        }

        /// <summary>
        /// 检查在线人数，当服务器长时间运行，显示的在线人数不对时，默认是十分钟检查一次
        /// </summary>
        protected virtual bool CheckOnLinePlayers()
        {
            foreach (var item in Players)
            {
                if (item.Value.isDispose | !item.Value.Login)
                {
                    Players.TryRemove(item.Key, out _);
                }
            }
            return IsRunServer;
        }

        /// <summary>
        /// 当数据堆积超过<see cref="LimitQueueCount"/>后触发的提示
        /// </summary>
        /// <param name="client"></param>
        protected virtual void OnDataQueueOverflow(Player client)
        {
            client.DataQueueOverflowError++;
        }

        /// <summary>
        /// 退出场景处理
        /// </summary>
        protected virtual void ExitSceneHandler(Player client) { }

        public override string ToString()
        {
            return $"{Name} {AreaName}";
        }
    }

    /// <summary>
    /// 网络服务器核心基类 2019.11.22
    /// <para>Player:当有客户端连接服务器就会创建一个Player对象出来, Player对象和XXXClient是对等端, 每当有数据处理都会通知Player对象. </para>
    /// <para>Scene:你可以定义自己的场景类型, 比如帧同步场景处理, mmorpg场景什么处理, 可以重写Scene的Update等等方法实现每个场景的更新和处理. </para>
    /// </summary>
    public abstract class ServerBase<Player, Scene> : ServerBase<Player>, IServerHandle<Player, Scene>
        where Player : NetPlayer, new()
        where Scene : NetScene<Player>, new()
    {
        #region 属性
        /// <summary>
        /// 服务器场景，key是场景名或房间名，关卡名。 value是(场景或房间，关卡等)对象
        /// </summary>
        public ConcurrentDictionary<string, Scene> Scenes { get; set; } = new ConcurrentDictionary<string, Scene>();
        /// <summary>
        /// 网络服务器单例
        /// </summary>
        public new static ServerBase<Player, Scene> Instance { get; protected set; }
        #endregion

        /// <summary>
        /// 构造网络服务器函数
        /// </summary>
        public ServerBase()
        {
        }

        #region 索引
        /// <summary>
        /// 场景索引
        /// </summary>
        /// <param name="sceneID"></param>
        /// <returns></returns>
        public Scene this[string sceneID] => Scenes[sceneID];

        /// <summary>
        /// 获得所有服务器场景
        /// </summary>
        /// <returns></returns>
        public List<Scene> GetScenes()
        {
            return new List<Scene>(Scenes.Values);
        }
        #endregion

        #region 重写方法
        /// <summary>
        /// 当添加默认网络场景，服务器初始化后会默认创建一个主场景，供所有玩家刚登陆成功分配的临时场景，默认初始化场景人数为1000人
        /// </summary>
        /// <returns>返回值string：网络玩家所在的场景名称 , 返回值NetScene：网络玩家的场景对象</returns>
        protected virtual Scene OnAddDefaultScene()
        {
            return new Scene { Name = MainSceneName, sceneCapacity = 1000 };
        }

        /// <summary>
        /// 当添加玩家到默认场景， 如果不想添加刚登录游戏成功的玩家进入主场景，可重写此方法让其失效
        /// </summary>
        /// <param name="client"></param>
        protected override void OnAddPlayerToScene(Player client)
        {
            if (Scenes.TryGetValue(MainSceneName, out Scene scene))
            {
                scene.AddPlayer(client);//将网络玩家添加到主场景集合中
            }
        }

        /// <summary>
        /// 当接收到客户端使用<see cref="Client.ClientBase.AddOperation(Operation)"/>方法发送的请求时调用
        /// </summary>
        /// <param name="client">当前客户端</param>
        /// <param name="list">操作列表</param>
        protected override void OnOperationSync(Player client, OperationList list)
        {
            if (client.OnOperationSync(list))
                return;
            if (client.Scene is Scene scene)
                scene.OnOperationSync(client, list);
        }

        /// <summary>
        /// 当客户端使用场景转发命令会调用此方法
        /// </summary>
        /// <param name="client"></param>
        /// <param name="model"></param>
        protected override void OnSceneRelay(Player client, RPCModel model)
        {
            if (!(client.Scene is Scene scene))
            {
                client.RpcModels.Enqueue(new RPCModel(cmd: model.cmd, kernel: model.kernel, buffer: model.Buffer, serialize: false, protocol: model.protocol));
                return;
            }
            Multicast(scene.Players, new RPCModel(cmd: model.cmd, kernel: model.kernel, buffer: model.Buffer, serialize: false, protocol: model.protocol));
        }
        #endregion

        protected override void CheckInstance()
        {
            base.CheckInstance();
            if (Instance == null)
                Instance = this;
        }

        protected override void FreeInstance()
        {
            base.FreeInstance();
            if (Instance == this)
                Instance = null;
        }

        protected override void AddDefaultScene()
        {
            var scene = OnAddDefaultScene();
            if (scene != null)
            {
                MainSceneName = scene.Name;
                CreateScene(scene);
            }
        }

        protected override void CreateSceneTickThread()
        {
            var thread = new Thread(SceneUpdateHandle) { IsBackground = true, Name = "SceneTickHandle" };
            thread.Start();
            ServerThreads.Add("SceneTickHandle", thread);
        }

        /// <summary>
        /// 网络场景推动玩家同步更新处理线程, 如果想自己处理场景同步, 可重写此方法让同步失效
        /// </summary>
        protected override void SceneUpdateHandle()
        {
            var timer = new TimerTick();
            uint tick = (uint)Environment.TickCount;
            uint fpsTick = tick + 1000;
            while (IsRunServer)
            {
                try
                {
                    tick = (uint)Environment.TickCount;
                    if (timer.CheckTimeout(tick, (uint)SyncSceneTime, true))
                    {
                        var result = Parallel.ForEach(Scenes.Values, scene =>
                        {
                            try
                            {
                                scene.UpdateLock(this, NetCmd.OperationSync);
                                scene.currFps++;
                            }
                            catch (Exception ex)
                            {
                                Debug.LogError("场景轮询异常:" + ex);
                            }
                        });
                        while (!result.IsCompleted)
                        {
                            Thread.Sleep(1);
                        }
                    }
                    if (tick >= fpsTick)
                    {
                        foreach (var scene in Scenes.Values)
                        {
                            scene.preFps = scene.currFps;
                            scene.currFps = 0;
                        }
                        fpsTick = tick + 1000;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("场景更新异常:" + ex);
                }
            }
        }

        /// <summary>
        /// 检查断线重连处理
        /// </summary>
        /// <param name="client"></param>
        /// <param name="segment"></param>
        /// <param name="args"></param>
        protected virtual Player CheckReconnect(Socket client, ISegment segment, params object[] args)
        {
            client.ReceiveTimeout = 0;
            var userID = segment.ReadInt32();
            if (UIDClients.TryGetValue(userID, out var session))
            {
                session.ReconnectTimeout = (uint)Environment.TickCount + ReconnectionTimeout;
                var newRemotePoint = client.RemoteEndPoint as IPEndPoint;
                var oldRemotePoint = session.RemotePoint as IPEndPoint;
                //防止出错或者假冒的客户端设置, 导致直接替换真实的客户端
                //如果是新的IP和旧的IP相同，是可以进行替换的，这样就不会发生假冒客户端问题
                //Equals里面有重写IPAddress.Equals，所以能判断出来
                if (!session.Connected | !session.Client.Connected | Equals(oldRemotePoint.Address, newRemotePoint.Address))
                {
                    session.Client = client;
                    session.Connected = true;
                    AcceptHander(session, args); //需要重新设置断线重连的新套接字对象
                    SetClientIdentity(session);
                    session.OnReconnecting();
                    OnReconnecting(session);
                    return session;
                }
            }
            return AcceptHander(client, client.RemoteEndPoint, args);//如果取出的客户端不断线, 那说明是客户端有问题或者错乱, 给他个新的连接
        }

        #region 场景API
        /// <summary>
        /// 创建网络场景, 退出当前场景,进入所创建的场景 - 创建场景成功返回场景对象， 创建失败返回null
        /// </summary>
        /// <param name="player">创建网络场景的玩家实体</param>
        /// <param name="name">要创建的场景号或场景名称</param>
        /// <returns></returns>
        public Scene CreateScene(Player player, string name)
        {
            return CreateScene(player, name, new Scene() { Name = name });
        }

        /// <summary>
        /// 创建网络场景, 退出当前场景并加入所创建的场景 - 创建场景成功返回场景对象， 创建失败返回null
        /// </summary>
        /// <param name="player">创建网络场景的玩家实体</param>
        /// <param name="scene">创建场景的实体</param>
        /// <returns></returns>
        public Scene CreateScene(Player player, Scene scene)
        {
            return CreateScene(player, scene.Name, scene);
        }

        /// <summary>
        /// 创建网络场景, 退出当前场景并加入所创建的场景 - 创建场景成功返回场景对象， 创建失败返回null
        /// </summary>
        /// <param name="player">创建网络场景的玩家实体</param>
        /// <param name="name">要创建的场景号或场景名称</param>
        /// <param name="scene">创建场景的实体</param>
        /// <returns></returns>
        public Scene CreateScene(Player player, string name, Scene scene)
        {
            scene.Name = name;
            return CreateScene(player, scene, out _);
        }

        public Scene CreateScene(Player player, Scene scene, out Scene oldScene)
        {
            oldScene = player.Scene as Scene;
            if (string.IsNullOrEmpty(scene.Name))
                return null;
            if (Scenes.TryAdd(scene.Name, scene))
            {
                if (oldScene != null)
                    oldScene.Remove(player);
                OnSceneGroupSet(scene);
                scene.AddPlayer(player);
                scene.onSerializeOpt = OnSerializeOpt;
                scene.onSerializeRpc = OnSerializeRPC;
                return scene;
            }
            return null;
        }

        /// <summary>
        /// 创建一个场景, 成功则返回场景对象, 创建失败则返回null
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Scene CreateScene(string name)
        {
            return CreateScene(name, new Scene());
        }

        /// <summary>
        /// 创建一个场景, 成功则返回场景对象, 创建失败则返回null
        /// </summary>
        /// <param name="name"></param>
        /// <param name="scene"></param>
        /// <returns></returns>
        public Scene CreateScene(string name, Scene scene)
        {
            scene.Name = name;
            return CreateScene(scene);
        }

        /// <summary>
        /// 创建一个场景, 成功则返回场景对象, 创建失败则返回null
        /// </summary>
        /// <param name="scene"></param>
        /// <returns></returns>
        public Scene CreateScene(Scene scene)
        {
            if (string.IsNullOrEmpty(scene.Name))
            {
                Debug.LogError("创建的场景必须给名称,场景名称必须是唯一的!");
                return null;
            }
            if (Scenes.TryAdd(scene.Name, scene))
            {
                OnSceneGroupSet(scene);
                scene.onSerializeOpt = OnSerializeOpt;
                scene.onSerializeRpc = OnSerializeRPC;
                return scene;
            }
            return null;
        }

        protected virtual void OnSceneGroupSet(Scene scene)
        {
        }

        /// <summary>
        /// 退出当前场景,加入指定的场景 - 成功进入返回场景对象，进入失败返回null
        /// </summary>
        /// <param name="player">要进入sceneID场景的玩家实体</param>
        /// <param name="name">场景ID，要切换到的场景号或场景名称</param>
        /// <returns></returns>
        public Scene JoinScene(Player player, string name) => SwitchScene(player, name);

        public Scene JoinScene(Player player, Scene scene) => SwitchScene(player, scene);

        /// <summary>
        /// 进入场景 - 成功进入返回true，进入失败返回false
        /// </summary>
        /// <param name="player">要进入sceneID场景的玩家实体</param>
        /// <param name="name">场景ID，要切换到的场景号或场景名称</param>
        /// <returns></returns>
        public Scene EnterScene(Player player, string name) => SwitchScene(player, name);

        public Scene EnterScene(Player player, Scene scene) => SwitchScene(player, scene);

        /// <summary>
        /// 切换场景
        /// </summary>
        /// <param name="player">要操作的玩家</param>
        /// <param name="name">场景名称</param>
        /// <returns>进入的场景,如果查询的场景不存在则为null</returns>
        public Scene SwitchScene(Player player, string name)
        {
            return SwitchScene(player, name, out _);
        }

        /// <summary>
        /// 切换场景
        /// </summary>
        /// <param name="player">要操作的玩家</param>
        /// <param name="name">场景名称</param>
        /// <param name="oldScene">上次所在的场景</param>
        /// <returns>进入的场景,如果查询的场景不存在则为null</returns>
        public Scene SwitchScene(Player player, string name, out Scene oldScene)
        {
            oldScene = player.Scene as Scene;
            if (string.IsNullOrEmpty(name))
                return null;
            if (oldScene != null)
                if (oldScene.Name == name) //如果已经在这个场景, 直接返回对象
                    return oldScene;
            if (Scenes.TryGetValue(name, out Scene scene1))
                return SwitchScene(player, scene1, out _);
            return null;
        }

        /// <summary>
        /// 切换场景
        /// </summary>
        /// <param name="player">要操作的玩家</param>
        /// <param name="enterScene">要进入的场景</param>
        /// <returns>进入的场景</returns>
        public Scene SwitchScene(Player player, Scene enterScene)
        {
            return SwitchScene(player, enterScene, out _);
        }

        /// <summary>
        /// 切换场景
        /// </summary>
        /// <param name="player">要操作的玩家</param>
        /// <param name="enterScene">要进入的场景</param>
        /// <param name="oldScene">上次所在的场景</param>
        /// <returns>进入的场景</returns>
        public Scene SwitchScene(Player player, Scene enterScene, out Scene oldScene)
        {
            oldScene = player.Scene as Scene;
            if (oldScene != null)
                oldScene.Remove(player);
            enterScene.AddPlayer(player);
            return enterScene;
        }

        /// <summary>
        /// 退出场景 exitCurrentSceneCall回调时已经不包含player对象
        /// </summary>
        /// <param name="player"></param>
        /// <param name="isEntMain">退出当前场景是否进入主场景: 默认进入主场景</param>
        /// <param name="exitCurrentSceneCall">即将退出当前场景的处理委托函数: 如果你需要对即将退出的场景进行一些事后处理, 则在此委托函数执行! 如:退出当前场景通知当前场景内的其他客户端将你的玩家对象移除等功能</param>
        public void ExitScene(Player player, bool isEntMain = true, Action<Scene> exitCurrentSceneCall = null)
        {
            RemoveScenePlayer(player, isEntMain, exitCurrentSceneCall);
        }

        protected override void ExitSceneHandler(Player client)
        {
            ExitScene(client, false);
        }

        /// <summary>
        /// 移除服务器场景. 从服务器总场景字典中移除指定的场景: 当你移除指定场景后,如果场景内有其他玩家在内, 则把其他玩家添加到主场景内
        /// </summary>
        /// <param name="name">要移除的场景id</param>
        /// <param name="addToMainScene">允许即将移除的场景内的玩家添加到主场景?</param>
        /// <param name="exitCurrentSceneCall">即将退出当前场景的处理委托函数: 如果你需要对即将退出的场景进行一些事后处理, 则在此委托函数执行! 如:退出当前场景通知当前场景内的其他客户端将你的玩家对象移除等功能</param>
        /// <returns></returns>
        public bool RemoveScene(string name, bool addToMainScene = true, Action<Scene> exitCurrentSceneCall = null)
        {
            if (Scenes.TryRemove(name, out Scene scene))
            {
                exitCurrentSceneCall?.Invoke(scene);
                if (addToMainScene)
                {
                    var mainScene = Scenes[MainSceneName];
                    if (mainScene != null)
                        mainScene.AddPlayers(scene.Players);
                }
                scene.RemoveScene();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 将玩家从当前所在的场景移除掉， 移除之后此客户端将会进入默认主场景 call回调时已经不包含player对象
        /// </summary>
        /// <param name="player">要执行的玩家实体</param>
        /// <param name="isEntMain">退出当前场景是否进入主场景: 默认进入主场景</param>
        /// <param name="exitCurrentSceneCall">即将退出当前场景的处理委托函数: 如果你需要对即将退出的场景进行一些事后处理, 则在此委托函数执行! 如:退出当前场景通知当前场景内的其他客户端将你的玩家对象移除等功能</param>
        /// <returns></returns>
        public bool RemoveScenePlayer(Player player, bool isEntMain = true, Action<Scene> exitCurrentSceneCall = null)
        {
            if (string.IsNullOrEmpty(player.SceneName))
                return false;
            if (Scenes.TryGetValue(player.SceneName, out Scene scene))
            {
                scene.Remove(player);
                exitCurrentSceneCall?.Invoke(scene);
                if (isEntMain)
                {
                    Scene mainScene = Scenes[MainSceneName];
                    mainScene.AddPlayer(player);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 场景是否存在?
        /// </summary>
        /// <param name="sceneID"></param>
        /// <returns></returns>
        public bool IsHasScene(string sceneID)
        {
            return Scenes.ContainsKey(sceneID);
        }
        #endregion
    }
}