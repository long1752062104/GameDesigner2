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
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Collections.Concurrent;
using Cysharp.Threading.Tasks;
using Net.Event;
using Net.Share;
using Net.System;
using Net.Serialize;
using Net.Helper;
using Net.Server;
using Net.Adapter;
using Net.Common;
using Net.Config;

namespace Net.Client
{
    /// <summary>
    /// 网络客户端核心基类 2019.3.3
    /// </summary>
    public abstract class ClientBase : INetClient, ISendHandle, IRpcHandler, IDisposable
    {
        /// <summary>
        /// 客户端套接字
        /// </summary>
        public Socket Client { get; protected set; }
        /// <summary>
        /// IP地址
        /// </summary>
        public string host = "127.0.0.1";
        /// <summary>
        /// 端口号
        /// </summary>
        public int port = 9543;
        /// <summary>
        /// 远程模型缓冲区
        /// </summary>
        protected QueueSafe<RPCModel> RpcModels = new QueueSafe<RPCModel>();
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
        /// 网络独立执行线程
        /// </summary>
#if !UNITY_WEBGL
        protected Thread NetworkThread;
#endif
        /// <summary>
        /// 网络状态
        /// </summary>
        public NetworkState NetworkState { get; protected set; } = NetworkState.None;
        /// <summary>
        /// 服务器与客户端是否是连接状态
        /// </summary>
        public bool Connected { get; protected set; }
        /// <summary>
        /// 网络客户端实例
        /// </summary>
        public static ClientBase Instance { get; set; }
        /// <summary>
        /// 是否使用unity主线程进行每一帧更新？  
        /// True：使用unity的Update等方法进行更新，unity的组建可以在Rpc函数内进行调用。
        /// False：使用多线程进行网络更新，使用多线程更新后unity的组件将不能在rpc函数内进行赋值设置等操作，否则会出现错误问题!
        /// </summary>
        public bool UseUnityThread { get; set; }
        /// <summary>
        /// 每秒发送数据长度
        /// </summary>
        protected int sendCount;
        /// <summary>
        /// 每秒发送数据次数
        /// </summary>
        protected int sendAmount;
        /// <summary>
        /// 每秒解析rpc函数次数
        /// </summary>
        protected int resolveAmount;
        /// <summary>
        /// 每秒接收网络数据次数
        /// </summary>
        protected int receiveAmount;
        /// <summary>
        /// 每秒接收网络数据大小
        /// </summary>
        protected int receiveCount;
        /// <summary>
        /// 网络FPS
        /// </summary>
        public int FPS { get; private set; }
        /// <summary>
        /// 从启动到现在总流出的数据流量
        /// </summary>
        protected long outflowTotal;
        /// <summary>
        /// 从启动到现在总流入的数据流量
        /// </summary>
        protected long inflowTotal;
        /// <summary>
        /// 心跳次数
        /// </summary>
        protected int heart = 0;
        /// <summary>
        /// 当前客户端是否打开(运行)
        /// </summary>
        protected bool openClient;
        /// <summary>
        /// 客户端是否处于打开状态
        /// </summary>
        public bool IsOpenClient => openClient;
        /// <summary>
        /// 输出调用网络函数
        /// </summary>
        public bool LogRpc { get; set; }
        #region 网络事件
        /// <summary>
        /// 当连接服务器成功事件
        /// </summary>
        public Action OnConnectedHandle { get; set; }
        /// <summary>
        /// 当连接失败事件
        /// </summary>
        public Action OnConnectFailedHandle { get; set; }
        /// <summary>
        /// 当尝试连接服务器事件
        /// </summary>
        public Action OnTryToConnectHandle { get; set; }
        /// <summary>
        /// 当尝试连接失败
        /// </summary>
        public Action TryToConnectFailedHandle { get; set; }
        /// <summary>
        /// 当连接中断 (异常) 事件
        /// </summary>
        public Action OnConnectLostHandle { get; set; }
        /// <summary>
        /// 当断开连接事件
        /// </summary>
        public Action OnDisconnectHandle { get; set; }
        /// <summary>
        /// 当接收到自定义的cmd指令时调用事件
        /// </summary>
        public RPCModelEvent OnReceiveDataHandle { get; set; }
        /// <summary>
        /// 当断线重连成功触发事件
        /// </summary>
        public Action OnReconnectHandle { get; set; }
        /// <summary>
        /// 当关闭连接事件
        /// </summary>
        public Action OnCloseConnectHandle { get; set; }
        /// <summary>
        /// 当统计网络流量时触发
        /// </summary>
        public NetworkDataTraffic OnNetworkDataTraffic { get; set; }
        /// <summary>
        /// 当使用服务器的NetScene.AddOperation方法时调用， 场景内的所有演员行为同步
        /// </summary>
        public OnOperationSyncEvent OnOperationSync { get; set; }
        /// <summary>
        /// 当服务器发送的大数据时, 可监听此事件显示进度值
        /// </summary>
        public virtual Action<BigDataProgress> OnRevdBigDataProgress { get; set; }
        /// <summary>
        /// 当客户端发送可靠数据时, 可监听此事件显示进度值 (GcpClient有效)
        /// </summary>
        public virtual Action<BigDataProgress> OnCallProgress { get; set; }
        /// <summary>
        /// 当添加远程过程调用方法时调用， 参数1：要收集rpc特性的对象，参数2:是否异步收集rpc方法和同步字段与属性？ 参数3：如果客户端的rpc列表中已经有了这个对象，还可以添加进去？
        /// </summary>
        public Action<object, bool, Action<SyncVarInfo>> OnAddRpcHandle { get; set; }
        /// <summary>
        /// 当移除远程过程调用对象， 参数1：移除此对象的所有rpc方法
        /// </summary>
        public Action<object> OnRemoveRpc { get; set; }
        /// <summary>
        /// 当执行调用远程过程方法时触发
        /// </summary>
        public RPCModelEvent OnRPCExecute { get; set; }
        /// <summary>
        /// 当内核序列化远程函数时调用, 如果想改变内核rpc的序列化方式, 可重写定义序列化协议 (只允许一个委托, 例子:OnSerializeRpcHandle = (model)=>{return new byte[0];};)
        /// </summary>
        public SerializeRpcDelegate OnSerializeRPC { get; set; }
        /// <summary>
        /// 当内核解析远程过程函数时调用, 如果想改变内核rpc的序列化方式, 可重写定义解析协议 (只允许一个委托, 例子:OnDeserializeRpcHandle = (buffer)=>{return new FuncData();};)
        /// </summary>
        public DeserializeRpcDelegate OnDeserializeRPC { get; set; }
        /// <summary>
        /// 当内部序列化帧操作列表时调用, 即将发送数据  !!!!!!!只允许一个委托
        /// </summary>
        public SerializeOptDelegate OnSerializeOPT { get; set; }
        /// <summary>
        /// 当内部解析帧操作列表时调用  !!!!!只允许一个委托
        /// </summary>
        public Func<ISegment, OperationList> OnDeserializeOPT { get; set; }
        /// <summary>
        /// ping服务器回调 参数double为延迟毫秒单位 当RTOMode属性为可变重传时, 内核将会每秒自动ping一次
        /// </summary>
        public Action<uint> OnPingCallback { get; set; }
        /// <summary>
        /// 当socket发送失败调用. 参数1:发送的字节数组  ->可通过SendByteData方法重新发送
        /// </summary>
        public Action<ISegment> OnSendErrorHandle { get; set; }
        /// <summary>
        /// 当从服务器获取的客户端地址点对点
        /// </summary>
        public Action<IPEndPoint> OnP2PCallback { get; set; }
        /// <summary>
        /// 当开始下载文件时调用, 参数1(string):服务器发送的文件名 返回值(string):开发者指定保存的文件路径(全路径名称)
        /// </summary>
        public Func<string, string> OnDownloadFileHandle { get; set; }
        /// <summary>
        /// 当服务器发送的文件完成, 接收到文件后调用, 返回true:框架内部释放文件流和删除临时文件(默认) false:使用者处理
        /// </summary>
        public Func<BigData, bool> OnReceiveFileHandle { get; set; }
        /// <summary>
        /// 当接收到发送的文件进度
        /// </summary>
        public Action<BigDataProgress> OnRevdFileProgress { get; set; }
        /// <summary>
        /// 当发送的文件进度
        /// </summary>
        public Action<BigDataProgress> OnSendFileProgress { get; set; }
        /// <summary>
        /// 当排队等待中
        /// </summary>
        public Action<int, int> OnWhenQueuing { get; set; }
        /// <summary>
        /// 当排队解除调用
        /// </summary>
        public Action OnQueueCancellation { get; set; }
        /// <summary>
        /// 当服务器爆满，服务器积极拒绝客户端请求
        /// </summary>
        public Action OnServerFull { get; set; }
        /// <summary>
        /// 当更新版本(参数:服务器的版本号)-- 当服务器版本和客户端版本不一致时, 会调用此事件
        /// </summary>
        public Action<int> OnUpdateVersion { get; set; }
        /// <summary>
        /// 当属性同步-- 当MysqlBuild生成的类属性在服务器被修改后同步下来会调用此事件
        /// </summary>
        public RPCModelEvent OnSyncPropertyHandle { get; set; }
        /// <summary>
        /// 当数据超出<see cref="LimitQueueCount"/>限制后触发的事件
        /// </summary>
        public Action OnDataQueueOverflow { get; set; }
        #endregion
        /// <summary>
        /// 4个字节记录数据长度 + 1CRC校验
        /// </summary>
        protected virtual int Frame { get; set; } = 5;
        /// <summary>
        /// 心跳时间间隔, 默认每1秒检查一次玩家是否离线, 玩家心跳确认为5次, 如果超出5次 则移除玩家客户端. 确认玩家离线总用时5秒, 
        /// 如果设置的值越小, 确认的速度也会越快. 值太小有可能出现直接中断问题, 设置的最小值在100以上
        /// </summary>
        public virtual int HeartInterval { get; set; } = 1000;
        /// <summary>
        /// <para>心跳检测次数, 默认为5次检测, 如果5次发送心跳给客户端或服务器, 没有收到回应的心跳包, 则进入断开连接处理</para>
        /// <para>当一直有数据往来时是不会发送心跳数据的, 只有当没有数据往来了, 才会进入发送心跳数据</para>
        /// </summary>
        public virtual byte HeartLimit { get; set; } = 5;
        /// <summary>
        /// 客户端唯一标识, 当登录游戏后, 服务器下发下来的唯一标识, 这个标识就是你的玩家名称, 是<see cref="Server.NetPlayer.PlayerID"/>值
        /// </summary>
        public int Identify => UID;
        /// <summary>
        /// 用户唯一标识, 对应服务器的<see cref="Server.NetPlayer.UserID"/>
        /// </summary>
        public int UID { get; protected set; }
        /// <summary>
        /// 上次的用户id, 断线重连时用到
        /// </summary>
        protected int PreUserId { get; set; }
        /// <summary>
        /// 跨线程调用任务队列
        /// </summary>
        public JobQueueHelper WorkerQueue { get; set; } = new JobQueueHelper();
        /// <summary>
        /// 接收缓存最大的数据长度 默认可缓存5242880(5M)的数据长度
        /// </summary>
        public int PackageSize { get; set; } = 1024 * 1024 * 5;
        /// <summary>
        /// TCP叠包值， 0:正常 >1:叠包次数 > StackNumberMax :清空叠包缓存流
        /// </summary>
        protected int stacking;
        protected int stackingOffset;
        protected int stackingCount;
        /// <summary>
        /// 数据缓冲流
        /// </summary>
        protected MemoryStream BufferStream { get; set; }
        /// <summary>
        /// 待发送的操作列表
        /// </summary>
        private readonly ListSafe<Operation> operations = new ListSafe<Operation>();
        /// <summary>
        /// <para>（Maxium Transmission Unit）最大传输单元, 最大传输单元为1500字节, 这里默认为50000, 如果数据超过50000,则是该框架进行分片. 传输层则需要分片为50000/1472=34个数据片</para>
        /// <para>------ 局域网可以设置为50000, 公网需要设置为1300 或 1400, 如果设置为1400还是发送失败, 则需要设置为1300或以下进行测试 ------</para>
        /// <para>1.链路层：以太网的数据帧的长度为(64+18)~(1500+18)字节，其中18是数据帧的帧头和帧尾，所以数据帧的内容最大为1500字节（不包括帧头和帧尾），即MUT为1500字节</para>
        /// <para>2.网络层：IP包的首部要占用20字节，所以这里的MTU＝1500－20＝1480字节</para>
        /// <para>3.传输层：UDP包的首部要占有8字节，所以这里的MTU＝1480－8＝1472字节</para>
        /// <see langword="注意:服务器和客户端的MTU属性的值必须保持一致性,否则分包的数据将解析错误!"/> <see cref="Server.ServerBase{Player, Scene}.MTU"/>
        /// </summary>
        public virtual int MTU { get; set; } = 1300;
        /// <summary>
        /// （Retransmission TimeOut）重传超时时间。 默认为1秒重传一次
        /// </summary>
        public virtual int RTO { get; set; } = 1000;
        /// <summary>
        /// (Maximum traffic per second) 每秒允许传输最大流量, 默认最大每秒可以传输1m大小
        /// </summary>
        public virtual int MTPS { get; set; } = 1024 * 1024;
        /// <summary>
        /// 流量控制模式，只有Gcp协议可用
        /// </summary>
        public virtual FlowControlMode FlowControl { get; set; } = FlowControlMode.Normal;
        /// <summary>
        /// 客户端端口
        /// </summary>
        protected int localPort;
        /// <summary>
        /// 组包数量，如果是一些小数据包，最多可以组合多少个？ 默认是组合1000个后发送
        /// </summary>
        public int PackageLength { get; set; } = 1000;
        /// <summary>
        /// 限制发送队列长度
        /// </summary>
        public int LimitQueueCount { get; set; } = ushort.MaxValue;
        private readonly MyDictionary<int, BigData> BigDataDic = new MyDictionary<int, BigData>();
        protected int singleThreadHandlerID, networkTickID, networkFlowHandlerID, heartHandlerID;
        private int sendFileTick, recvFileTick;
        /// <summary>
        /// 自动断线重新连接, 默认是true
        /// </summary>
        public bool AutoReconnecting { get; set; } = true;
        /// <summary>
        /// 当前尝试重连次数
        /// </summary>
        public int CurrReconnect { get; protected set; }
        /// <summary>
        /// 断线重连次数, 默认会重新连接10次，如果连接10次都失败，则会关闭客户端并释放占用的资源
        /// </summary>
        public int ReconnectCount { get; set; } = 10;
        /// <summary>
        /// 断线重连间隔, 默认间隔2秒
        /// </summary>
        public int ReconnectInterval { get; set; } = 2000;
        /// <summary>
        /// GCP协议接口
        /// </summary>
        public IGcp Gcp { get; set; }
        /// <summary>
        /// 网络更新方式
        /// </summary>
        public NetworkUpdateMode UpdateMode { get; set; } = NetworkUpdateMode.Thread;
        /// <summary>
        /// 序列化适配器
        /// </summary>
        public ISerializeAdapter SerializeAdapter { get; set; }
        /// <summary>
        /// 数据包适配器
        /// </summary>
        public IPackageAdapter PackageAdapter { get; set; } = new DataAdapter();
        /// <summary>
        /// 版本号
        /// </summary>
        public int Version { get; set; } = 1;
        /// <summary>
        /// 网络循环事件处理
        /// </summary>
        protected TimerEvent LoopEvent = new TimerEvent();
        protected uint tokenCount = 1;
        protected FastLocking tokenLock = new FastLocking();
        private int sendBufferSize = ushort.MaxValue;
        /// <summary>
        /// 设置Socket的发送缓冲区大小, 也叫做窗口大小
        /// </summary>
        public int SendBufferSize
        {
            get => sendBufferSize;
            set
            {
                if (Client != null)
                    Client.SendBufferSize = value;
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
                if (Client != null)
                    Client.ReceiveBufferSize = value;
                receiveBufferSize = value;
            }
        }
        protected int performance;
        /// <summary>
        /// 客户端性能模式
        /// </summary>
        public Performance Performance
        {
            get => (Performance)performance;
            set => performance = (int)value;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public ClientBase()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="useUnityThread">
        /// 是否使用unity主线程进行每一帧更新？  
        /// True：使用unity的Update等方法进行更新，unity的组建可以在Rpc函数内进行调用。
        /// False：使用多线程进行网络更新，使用多线程更新后unity的组件将不能在rpc函数内进行赋值设置等操作，否则会出错!
        /// </param>
        public ClientBase(bool useUnityThread) : this()
        {
            UseUnityThread = useUnityThread;
        }

        ~ClientBase()
        {
#if !UNITY_EDITOR || BUILT_UNITY
            Close();
#elif UNITY_EDITOR
            Close(true, 100);
#endif
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
        /// 添加网络Rpc
        /// </summary>
        /// <param name="target">注册的对象实例</param>
        public void AddRpcHandle(object target)
        {
            AddRpcHandle(target, false);
        }

        /// <summary>
        /// 添加网络Rpc
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

        private void AddRpcInternal(object target, bool append, Action<SyncVarInfo> onSyncVarCollect) => RpcHelper.AddRpc(this, target, append, onSyncVarCollect);

        /// <summary>
        /// 移除客户端的Rpc方法
        /// </summary>
        /// <param name="target">将此对象的所有带有Rpc特性的方法移除</param>
        public void RemoveRpc(object target)
        {
            if (OnRemoveRpc == null)
                OnRemoveRpc = RemoveRpcInternal;
            OnRemoveRpc(target);
        }

        private void RemoveRpcInternal(object target) => RpcHelper.RemoveRpc(this, target);

        /// <summary>
        /// 绑定Rpc函数
        /// </summary>
        /// <param name="target">注册的对象实例</param>
        public void BindRpc(object target) => AddRpcHandle(target);

        /// <summary>
        /// 绑定网络状态处理接口
        /// </summary>
        /// <param name="network"></param>
        public void BindNetworkHandle(INetworkHandle network)
        {
            OnConnectedHandle += network.OnConnected;
            OnConnectFailedHandle += network.OnConnectFailed;
            OnConnectLostHandle += network.OnConnectLost;
            OnDisconnectHandle += network.OnDisconnect;
            OnReconnectHandle += network.OnReconnect;
            OnTryToConnectHandle += network.OnTryToConnect;
            OnCloseConnectHandle += network.OnCloseConnect;
            OnWhenQueuing += network.OnWhenQueuing;
            OnQueueCancellation += network.OnQueueCancellation;
            OnServerFull += network.OnServerFull;
        }

        /// <summary>
        /// 移除网络状态处理接口
        /// </summary>
        /// <param name="network"></param>
        public void RemoveNetworkHandle(INetworkHandle network)
        {
            OnConnectedHandle -= network.OnConnected;
            OnConnectFailedHandle -= network.OnConnectFailed;
            OnConnectLostHandle -= network.OnConnectLost;
            OnDisconnectHandle -= network.OnDisconnect;
            OnReconnectHandle -= network.OnReconnect;
            OnTryToConnectHandle -= network.OnTryToConnect;
            OnCloseConnectHandle -= network.OnCloseConnect;
            OnWhenQueuing -= network.OnWhenQueuing;
            OnQueueCancellation -= network.OnQueueCancellation;
            OnServerFull -= network.OnServerFull;
        }

        /// <summary>
        /// 派发给所有被收集的Rpc方法
        /// </summary>
        /// <param name="func"></param>
        /// <param name="pars"></param>
        public void DispatchRpc(string func, params object[] pars)
        {
            PushRpcData(new RPCModel(cmd: 0, kernel: true, protocol: func.CRCU32(), pars: pars));
        }

        /// <summary>
        /// 派发给所有被收集的Rpc方法
        /// </summary>
        /// <param name="hash"></param>
        /// <param name="pars"></param>
        public void DispatchRpc(uint hash, params object[] pars)
        {
            PushRpcData(new RPCModel(cmd: 0, kernel: true, protocol: hash, pars: pars));
        }

        /// <summary>
        /// 压入远程过程调用方法(RPC)， 将在NetworkEventUpdate线程调用
        /// </summary>
        /// <param name="model"></param>
        public void PushRpcData(RPCModel model) => RpcHelper.Invoke(this, null, model, AddRpcWorkQueue, RpcLog);

        private void RpcLog(int log, NetPlayer client, RPCModel model)
        {
            switch (log)
            {
                case 0:
                    NDebug.LogWarning($"[protocol:{model.protocol}]的远程方法未被收集!请定义[Rpc(hash = {model.protocol})] void xx方法和参数, 并使用client.AddRpc方法收集rpc方法!");
                    break;
                case 1:
                    NDebug.LogWarning($"[protocol={model.protocol}]服务器响应的Token={model.token}没有进行设置!");
                    break;
                case 2:
                    NDebug.LogWarning($"{model}的远程方法未被收集!请定义[Rpc]void xx方法和参数, 并使用client.AddRpc方法收集rpc方法!");
                    break;
            }
        }

        private void AddRpcWorkQueue(MyDictionary<object, IRPCMethod> methods, NetPlayer client, RPCModel model)
        {
            foreach (RPCMethod rpc in methods.Values)
            {
                if (rpc.cmd == NetCmd.ThreadRpc)
                {
                    rpc.Invoke(model.pars);
                }
                else
                {
                    WorkerQueue.Call(new RpcInvokeArgs(LogRpc, rpc.target, rpc.method, model.pars));
                }
            }
        }

        /// <summary>
        /// 开启线程
        /// </summary>
        /// <param name="threadName">线程名称</param>
        /// <param name="start">线程函数</param>
        public void StartThread(string threadName, ThreadStart start)
        {
#if !UNITY_WEBGL
            try { NetworkThread?.Interrupt(); } catch { }
            NetworkThread = new Thread(start)
            {
                Name = threadName,
                IsBackground = true
            };
            NetworkThread.Start();
#endif
        }

        /// <summary>
        /// 结束所有线程
        /// </summary>
        public void AbortedThread()
        {
#if !UNITY_WEBGL
            try { NetworkThread?.Interrupt(); } catch { }
#endif
            ThreadManager.Event.RemoveEvent(singleThreadHandlerID);
            LoopEvent.Clear();
        }

        /// <summary>
        /// 网络数据更新
        /// </summary>
        public void NetworkUpdate()
        {
            WorkerQueue.Execute();
        }

        /// <summary>
        /// 连接服务器
        /// </summary>
        public UniTask<bool> Connect()
        {
            return Connect(connected => { });
        }

        /// <summary>
        /// 连接服务器
        /// </summary>
        /// <param name="result">连接结果</param>
        /// <returns></returns>
        public UniTask<bool> Connect(Action<bool> result)
        {
            return Connect(host, port, result);
        }

        /// <summary>
        /// 连接服务器
        /// </summary>
        /// <param name="host">IP地址</param>
        /// <param name="port">端口号</param>
        public virtual UniTask<bool> Connect(string host, int port)
        {
            return Connect(host, port, result => { });
        }

        /// <summary>
        /// 连接服务器
        /// </summary>
        /// <param name="host">IP地址</param>
        /// <param name="port">端口号</param>
        /// <param name="result">连接结果</param>
        public virtual UniTask<bool> Connect(string host, int port, Action<bool> result)
        {
            return Connect(host, port, -1, result);
        }

        /// <summary>
        /// 连接服务器
        /// </summary>
        /// <param name="host">IP地址</param>
        /// <param name="port">端口号</param>
        /// <param name="localPort">设置自身端口号,如果不设置自身端口则值为-1</param>
        /// <param name="result">连接结果</param>
        public virtual UniTask<bool> Connect(string host, int port, int localPort, Action<bool> result)
        {
            if (NetworkState == NetworkState.Connection)
            {
                NDebug.Log("连接服务器中,请稍等...");
                return UniTask.FromResult(false);
            }
            if (openClient)
            {
                Close();
                NDebug.Log("连接服务器中,请稍等...");
            }
            openClient = true;
            NetworkState = NetworkState.Connection;
            if (Instance == null)
                Instance = this;
            if (OnAddRpcHandle == null) OnAddRpcHandle = AddRpcInternal;
            if (OnRPCExecute == null) OnRPCExecute = PushRpcData;
            if (OnRemoveRpc == null) OnRemoveRpc = RemoveRpcInternal;
            if (OnSerializeRPC == null) OnSerializeRPC = OnSerializeRpcInternal;
            if (OnDeserializeRPC == null) OnDeserializeRPC = OnDeserializeRpcInternal;
            if (OnSerializeOPT == null) OnSerializeOPT = OnSerializeOptInternal;
            if (OnDeserializeOPT == null) OnDeserializeOPT = OnDeserializeOptInternal;
            if (OnDataQueueOverflow == null) OnDataQueueOverflow = OnDataQueueOverflowInternal;
            AddRpcHandle(this, false);
            if (Client == null) //如果套接字为空则说明没有连接上服务器
            {
                this.host = host;
                this.port = port;
                return ConnectResult(host, port, localPort, isConnected => OnConnected(isConnected, result));
            }
            else if (!Connected)
            {
                Client.Close();
                Client = null;
                NetworkState = NetworkState.ConnectLost;
                InvokeInMainThread(OnConnectLostHandle);
                NDebug.LogError("服务器连接中断!");
                AbortedThread();
                result(false);
            }
            else
            {
                result(true);
            }
            return UniTask.FromResult(Connected);
        }

        /// <summary>
        /// 连接服务器
        /// </summary>
        /// <param name="host">连接的服务器主机IP地址</param>
        /// <param name="port">连接的服务器主机端口号</param>
        /// <param name="localPort">设置自身端口号,如果不设置自身端口则值为-1</param>
        /// <param name="result">连接结果</param>
        protected virtual UniTask<bool> ConnectResult(string host, int port, int localPort, Action<bool> result)
        {
            try
            {
                Client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
                {
                    SendBufferSize = SendBufferSize,
                    ReceiveBufferSize = ReceiveBufferSize
                };
                this.localPort = localPort;
                if (localPort != -1)
                    Client.Bind(new IPEndPoint(IPAddress.Any, localPort));
                return CheckIdentity(() => Client.Connect(host, port), result);
            }
            catch (Exception ex)
            {
                NDebug.LogError("连接错误:" + ex);
                result(false);
                return UniTask.FromResult(false);
            }
        }

        protected async virtual UniTask<bool> CheckIdentity(Action begin, Action<bool> result)
        {
            await UniTask.SwitchToThreadPool();
            try
            {
                begin?.Invoke();
                var tick = DateTimeHelper.GetTickCount64();
                await UniTaskNetExtensions.Wait(8000, (state) =>
                {
                    if (DateTimeHelper.GetTickCount64() >= tick)
                    {
                        tick = DateTimeHelper.GetTickCount64() + 1000;
                        var segment = BufferPool.Take(SendBufferSize);
                        segment.Write(PreUserId);
                        RpcModels.Enqueue(new RPCModel(cmd: NetCmd.Identify, buffer: segment.ToArray(true)));
                    }
                    NetworkTick();
                    return UID != 0 | !openClient; //如果在爆满事件关闭客户端就需要判断一下
                }, DBNull.Value);
                if (UID == 0 && openClient)
                    throw new Exception("连接握手失败!");
                if (UID == 0 && !openClient)
                    throw new Exception("客户端调用Close!");
                StartupThread();
                await UniTask.Yield(); //切换到线程池中, 不要由事件线程去往下执行, 如果有耗时就会卡死事件线程, 在unity会切换到unity线程去执行，解决unity组件访问错误问题
                result(true);
                return true;
            }
            catch (Exception ex)
            {
                NDebug.LogError("连接失败原因:" + ex.ToString());
                Connected = false;
                Client?.Close();
                Client = null;
                await UniTask.Yield(); //在unity会切换到unity主线程去执行，解决unity组件访问错误问题
                result(false);
                return false;
            }
        }

        protected void InvokeInMainThread(Action action)
        {
            WorkerQueue.Call(action);
        }

        protected void InvokeInMainThread<T>(Action<T> action, T arg)
        {
            WorkerQueue.Call(action, arg);
        }

        protected void InvokeInMainThread<T, T1>(Action<T, T1> action, T arg1, T1 arg2)
        {
            WorkerQueue.Call(action, arg1, arg2);
        }

        protected void InvokeInMainThread(RPCModelEvent action, RPCModel model)
        {
            WorkerQueue.Call(action, model);
        }

        protected void InvokeInMainThread(OnOperationSyncEvent action, in OperationList operList)
        {
            WorkerQueue.Call(action, operList);
        }

        /// <summary>
        /// 局域网广播寻找服务器主机, 如果找到则通过 result 参数调用, 如果成功获取到主机, 那么result的第一个参数为true, 并且result的第二个参数为服务器IP
        /// </summary>
        /// <param name="result">连接结果</param>
        public Task Broadcast(Action<bool, string> result = null)
        {
            return Broadcast(port, result);
        }

        /// <summary>
        /// 局域网广播寻找服务器主机, 如果找到则通过 result 参数调用, 如果成功获取到主机, 那么result的第一个参数为true, 并且result的第二个参数为服务器IP
        /// </summary>
        /// <param name="port">广播到服务器的端口号</param>
        /// <param name="result">连接结果</param>
        public Task Broadcast(int port = 9543, Action<bool, string> result = null)
        {
            var client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            var ipEndPoint = new IPEndPoint(IPAddress.Broadcast, port);
            client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 5000);
            bool isDone = false;
            Task.Run(() =>
            {
                while (!isDone)
                {
                    BroadcastSend(client, ipEndPoint);
                    Thread.Sleep(1000);
                }
            });
            return Task.Run(() =>
            {
                try
                {
                    Thread.Sleep(10);//先让上面Task跑起来执行SendTo后再执行下面的Receive,如果还没SendTo就Receive就会出现错误!
                    byte[] buffer = new byte[1024];
                    int count = client.Receive(buffer);
                    string ip = Encoding.Unicode.GetString(buffer, 0, count);
                    isDone = true;
                    client?.Close();
                    client = null;
                    InvokeInMainThread(result, true, ip);
                }
                catch (Exception ex)
                {
                    isDone = true;
                    client?.Close();
                    client = null;
                    InvokeInMainThread(result, false, ex.ToString());
                }
            });
        }

        protected virtual void BroadcastSend(Socket client, IPEndPoint ipEndPoint)
        {
            client.SendTo(new byte[] { 6, 0, 0, 0, 0, 0x2d, 74, NetCmd.Broadcast, 0, 0, 0, 0 }, ipEndPoint);
        }

        /// <summary>
        /// 连接成功处理
        /// </summary>
        protected virtual void StartupThread()
        {
            AbortedThread();//断线重连处理
            Connected = true;
#if !UNITY_WEBGL
            switch (UpdateMode)
            {
                case NetworkUpdateMode.Thread:
                    StartThread("NetworkProcessing", NetworkProcessing);
                    break;
                case NetworkUpdateMode.SingleThread:
                    singleThreadHandlerID = ThreadManager.Invoke("SingleNetworkProcessing", SingleNetworkProcessing);
                    break;
                case NetworkUpdateMode.CustomExecution:
                    break;
            }
#else
            singleThreadHandlerID = ThreadManager.Invoke("SingleNetworkProcessing", SingleNetworkProcessing);
#endif
            networkTickID = LoopEvent.AddEvent("NetworkTick", 0, NetworkTick);
            networkFlowHandlerID = LoopEvent.AddEvent("NetworkFlowHandler", 1f, NetworkFlowHandler);
            heartHandlerID = LoopEvent.AddEvent("HeartHandler", HeartInterval, HeartHandler);
        }

        /// <summary>
        /// 连接结果处理
        /// </summary>
        /// <param name="isConnect"></param>
        /// <param name="action">结果</param>
        protected virtual void OnConnected(bool isConnect, Action<bool> action)
        {
            if (isConnect)
            {
                NetworkState = NetworkState.Connected;
                InvokeInMainThread(action, true);
                InvokeInMainThread(OnConnectedHandle);
                NDebug.Log("成功连接服务器...");
            }
            else
            {
                NetworkState = NetworkState.ConnectFailed;
                InvokeInMainThread(action, false);
                InvokeInMainThread(OnConnectFailedHandle);
                NDebug.LogError("服务器尚未开启或连接IP端口错误!");
                if (!UseUnityThread)
                    NetworkUpdate();
            }
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="reuseSocket">断开连接后还能重新使用？</param>
        /// <param name="invokeSocketDisconnect"></param>
        public virtual void Disconnect(bool reuseSocket, bool invokeSocketDisconnect = true)
        {
            NetworkState = NetworkState.Disconnect;
            Call(NetCmd.Disconnect, new byte[1]);
            SendDirect();
            Connected = false;
            if (Client != null && Client.ProtocolType == ProtocolType.Tcp && invokeSocketDisconnect)
                Client.Disconnect(reuseSocket);
            InvokeInMainThread(OnDisconnectHandle);
        }

        /// <summary>
        /// 调式输出网络流量信息
        /// </summary>
        protected virtual bool NetworkFlowHandler()
        {
            try
            {
                outflowTotal += sendCount;
                inflowTotal += receiveCount;
                OnNetworkDataTraffic?.Invoke(new Dataflow()
                {
                    sendCount = sendCount,
                    sendNumber = sendAmount,
                    receiveNumber = receiveAmount,
                    receiveCount = receiveCount,
                    resolveNumber = resolveAmount,
                    FPS = FPS,
                    outflowTotal = outflowTotal,
                    inflowTotal = inflowTotal,
                });
            }
            catch (Exception ex)
            {
                NDebug.LogError(ex.ToString());
            }
            finally
            {
                sendCount = 0;
                sendAmount = 0;
                resolveAmount = 0;
                receiveAmount = 0;
                receiveCount = 0;
                FPS = 0;
            }
            return Connected;
        }

        /// <summary>
        /// 当游戏操作行为封包数据时调用
        /// </summary>
        /// <param name="count"></param>
        protected virtual void OnOptPacket(int count)
        {
            var operations1 = operations.GetRemoveRange(0, count);
            var list = new OperationList(operations1);
            var buffer = OnSerializeOPT(list);
            RpcModels.Enqueue(new RPCModel(cmd: NetCmd.OperationSync, kernel: false, buffer: buffer, serialize: false));
        }

        protected internal virtual byte[] OnSerializeOptInternal(in OperationList list)
        {
            return NetConvertFast2.SerializeObject(list).ToArray(true);
        }

        protected internal virtual OperationList OnDeserializeOptInternal(ISegment segment)
        {
            return NetConvertFast2.DeserializeObject<OperationList>(segment, false);
        }

        /// <summary>
        /// 立刻发送, 不需要等待帧时间 (当你要强制把客户端下线时,你还希望客户端先发送完数据后,再强制客户端退出游戏用到)
        /// </summary>
        public virtual void SendDirect()
        {
            SendOperations();
            SendDataHandler(RpcModels);
        }

        /// <summary>
        /// 打包操作同步马上要发送了
        /// </summary>
        protected virtual void SendOperations()
        {
            int count = operations.Count;
            if (count > 0)
            {
                while (count > 500)
                {
                    OnOptPacket(500);
                    count -= 500;
                }
                if (count > 0)
                {
                    OnOptPacket(count);
                }
            }
        }

        protected virtual void SetDataHead(ISegment stream)
        {
            stream.Position = Frame + PackageAdapter.HeadCount;
        }

        protected virtual void WriteDataBody(ref ISegment stream, QueueSafe<RPCModel> rPCModels, int count)
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
                    var len = stream.Position + model.buffer.Length + Frame;
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
            stream.SetPositionLength(Frame + PackageAdapter.HeadCount);
        }

        /// <summary>
        /// 发送处理
        /// </summary>
        protected virtual void SendDataHandler(QueueSafe<RPCModel> rPCModels)
        {
            var count = rPCModels.Count;
            if (count <= 0)
                return;
            var stream = BufferPool.Take(SendBufferSize);
            SetDataHead(stream);
            WriteDataBody(ref stream, rPCModels, count);
            PackData(stream);
            SendByteData(stream);
            BufferPool.Push(stream);
        }

        protected virtual void PackData(ISegment stream)
        {
            stream.Flush(false);
            SetDataHead(stream);
            PackageAdapter.Pack(stream);
            var len = stream.Count - Frame;
            var lenBytes = BitConverter.GetBytes(len);
            var crc = CRCHelper.CRC8(lenBytes, 0, lenBytes.Length);
            stream.Position = 0;
            stream.Write(lenBytes, 0, 4);
            stream.WriteByte(crc);
            stream.Position += len;
        }

        protected virtual void SendByteData(ISegment buffer)
        {
            if (buffer.Count <= Frame)//解决长度==5的问题(没有数据)
                return;
            sendAmount++;
            sendCount += buffer.Count;
            Gcp.Send(buffer);
        }

        /// <summary>
        /// 当内核序列化远程函数时调用, 如果想改变内核rpc的序列化方式, 可重写定义序列化协议
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        protected internal virtual bool OnSerializeRpcInternal(ISegment segment, RPCModel model) => NetConvert.Serialize(segment, model);
        /// <summary>
        /// 当内核解析远程过程函数时调用, 如果想改变内核rpc的序列化方式, 可重写定义解析协议
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        protected internal virtual bool OnDeserializeRpcInternal(ISegment segment, RPCModel model) { return NetConvert.Deserialize(segment, model); }

        /// <summary>
        /// 网络处理线程
        /// </summary>
        protected virtual void NetworkProcessing()
        {
            while (NetworkProcessing(true)) { }
        }

        /// <summary>
        /// 单线程网络处理
        /// </summary>
        /// <returns></returns>
        public bool SingleNetworkProcessing() => NetworkProcessing(false);

        protected bool NetworkProcessing(bool sleep)
        {
            try
            {
                LoopEvent.UpdateEventFixed(1, sleep);
            }
            catch (ThreadAbortException) //线程结束不提示异常
            {
                return false; //如果这里不返回false, 则在ilcpp编译后执行Abort无法结束线程, 导致错误
            }
            catch (ThreadInterruptedException)
            {
                return false; //使用Thread.Interrupt结束线程触发的异常
            }
            catch (Exception ex)
            {
                NDebug.LogError("网络异常:" + ex);
            }
            return Connected | openClient | CurrReconnect < ReconnectCount; //当连接中断时不能结束线程, 还有尝试重连
        }

        protected virtual bool NetworkTick()
        {
            try
            {
                ReceiveHandler();
                SyncVarHandler();
                SendDirect();
                OnNetworkTick();
                if (!UseUnityThread)
                    NetworkUpdate();
                FPS++;
            }
            catch (Exception ex)
            {
                NetworkException(ex);
            }
            return Connected;
        }

        public virtual void ReceiveHandler()
        {
            if (Client.Poll(performance, SelectMode.SelectRead))
            {
                var segment = BufferPool.Take(ReceiveBufferSize);
                segment.Count = Client.Receive(segment.Buffer, 0, segment.Length, SocketFlags.None, out SocketError error);
                if (segment.Count == 0 | error != SocketError.Success)
                {
                    BufferPool.Push(segment);
                    if (Connected & openClient) //导致的问题是当调用Client.Close时, 线程并行到这里导致已经关闭客户端, 但是还是提示连接中断
                        throw new SocketException((int)error); //这个如果直接执行会触发两处连接中断事件
                    return;
                }
                receiveAmount++;
                receiveCount += segment.Count;
                heart = 0;
                ResolveBuffer(ref segment, false);
                BufferPool.Push(segment);
            }
            Gcp?.Update();
        }

        public virtual void OnNetworkTick()
        {
        }

        /// <summary>
        /// 网络异常处理
        /// </summary>
        /// <param name="ex"></param>
        protected void NetworkException(Exception ex)
        {
            if (ex is SocketException)
            {
                Connected = false;
                NetworkState = NetworkState.ConnectLost;
                RpcModels = new QueueSafe<RPCModel>();
                heart = HeartLimit + 1; //心跳时间直接到达最大值
                SetHeartInterval(ReconnectInterval); //断线后, 会改变心跳时间为断线重连间隔时间
                InvokeInMainThread(OnConnectLostHandle);
                NDebug.LogError("连接中断!" + ex);
                if (!UseUnityThread)
                    NetworkUpdate();
            }
            else if (ex is ObjectDisposedException)
            {
                Close();
                NDebug.LogError("客户端已被释放!" + ex);
            }
            else if (ex is ThreadAbortException)
            {
                //线程Abort时, 线程还在Thread.Sleep就会出现这个错误, 所以在这里忽略掉
            }
            else if (Connected)
            {
                NDebug.LogError("网络异常:" + ex);
            }
        }

        /// <summary>
        /// 解析网络数据包
        /// </summary>
        protected virtual void ResolveBuffer(ref ISegment stream, bool isTcp)
        {
            if (!isTcp)
            {
                Gcp.Input(stream);
                while (Gcp.Receive(out ISegment buffer1) > 0)
                {
                    ResolveBuffer(ref buffer1);
                    BufferPool.Push(buffer1);
                }
                return;
            }
            if (!PackageAdapter.Unpack(stream, Frame, UID))
                return;
            DataHandle(stream);
        }

        protected unsafe void ResolveBuffer(ref ISegment buffer)
        {
            heart = 0;
            if (stacking > 0)
            {
                stacking++;
                BufferStream.Seek(stackingOffset, SeekOrigin.Begin);
                int size = buffer.Count - buffer.Position;
                stackingOffset += size;
                BufferStream.Write(buffer.Buffer, buffer.Position, size);
                if (stackingOffset < stackingCount)
                {
                    InvokeRevdBigDataProgress(stackingOffset, stackingCount);
                    return;
                }
                var count = (int)BufferStream.Position;//.Length; //错误问题,不能用length, 这是文件总长度, 之前可能已经有很大一波数据
                BufferPool.Push(buffer);//要回收掉, 否则会提示内存泄露
                buffer = BufferPool.Take(count);//ref 才不会导致提示内存泄露
                BufferStream.Seek(0, SeekOrigin.Begin);
                BufferStream.Read(buffer.Buffer, 0, count);
                buffer.Count = count;
            }
            while (buffer.Position < buffer.Count)
            {
                if (buffer.Position + Frame > buffer.Count)//流数据偶尔小于frame头部字节
                {
                    var position = buffer.Position;
                    var count = buffer.Count - position;
                    stackingOffset = count;
                    stackingCount = 0;
                    BufferStream.Seek(0, SeekOrigin.Begin);
                    BufferStream.Write(buffer.Buffer, position, count);
                    stacking++;
                    break;
                }
                var lenBytes = buffer.ReadPtr(4);
                var crcCode = buffer.ReadByte();//CRC检验索引
                var retVal = CRCHelper.CRC8(lenBytes, 0, 4);
                if (crcCode != retVal)
                {
                    stacking = 0;
                    NDebug.LogError($"[{UID}]CRC校验失败!");
                    return;
                }
                var size = *(int*)lenBytes;
                if (size < 0 | size > PackageSize)//如果出现解析的数据包大小有问题，则不处理
                {
                    stacking = 0;
                    NDebug.LogError($"[{UID}]数据被拦截修改或数据量太大: size:{size}，如果想传输大数据，请设置PackageSize属性");
                    return;
                }
                if (buffer.Position + size <= buffer.Count)
                {
                    stacking = 0;
                    var count = buffer.Count;//此长度可能会有连续的数据(粘包)
                    buffer.Count = buffer.Position + size;//需要指定一个完整的数据长度给内部解析
                    ResolveBuffer(ref buffer, true);
                    buffer.Count = count;//解析完成后再赋值原来的总长
                }
                else
                {
                    var position = buffer.Position - Frame;
                    var count = buffer.Count - position;
                    stackingOffset = count;
                    stackingCount = size;
                    BufferStream.Seek(0, SeekOrigin.Begin);
                    BufferStream.Write(buffer.Buffer, position, count);
                    stacking++;
                    break;
                }
            }
        }

        protected void DataHandle(ISegment buffer)
        {
            var count = buffer.Count; //记录总长度，在解析每个rpc时不需要复制count，不要改，否则会导致反序列化rpc错误问题
            while (buffer.Position < count)
            {
                var kernelV = buffer.ReadByte();
                var kernel = kernelV == 68;
                if (!kernel & kernelV != 74)
                {
                    NDebug.LogError($"[{UID}][可忽略]协议出错!");
                    break;
                }
                var cmd1 = buffer.ReadByte();
                var token = buffer.ReadUInt32();
                var dataCount = (int)buffer.ReadUInt32Fixed();
                if (buffer.Position + dataCount > count)
                    break;
                var position = buffer.Position + dataCount;
                var model = new RPCModel(cmd: cmd1, kernel: kernel, buffer: buffer.Buffer, index: buffer.Position, count: dataCount, token: token);
                if (kernel)
                {
                    buffer.Count = dataCount;
                    buffer.Offset = buffer.Position;
                    var complete = OnDeserializeRPC(buffer, model);
                    if (!complete)
                        goto J;
                }
                CommandHandler(model, buffer);//解析协议完成
            J: buffer.Position = position;
            }
        }

        protected virtual void CommandHandler(RPCModel model, ISegment segment)
        {
            resolveAmount++;
            switch (model.cmd)
            {
                case NetCmd.RevdHeartbeat:
                    heart = 0;
                    break;
                case NetCmd.SendHeartbeat:
                    Call(NetCmd.RevdHeartbeat, new byte[1]);
                    break;
                case NetCmd.CallRpc:
                    if (model.kernel)
                        OnRPCExecute(model);
                    else
                        InvokeInMainThread(model);
                    break;
                case NetCmd.Local:
                    if (model.kernel)
                        OnRPCExecute(model);
                    else
                        InvokeInMainThread(model);
                    break;
                case NetCmd.Scene:
                    if (model.kernel)
                        OnRPCExecute(model);
                    else
                        InvokeInMainThread(model);
                    break;
                case NetCmd.Notice:
                    if (model.kernel)
                        OnRPCExecute(model);
                    else
                        InvokeInMainThread(model);
                    break;
                case NetCmd.ThreadRpc:
                    if (model.kernel)
                        OnRPCExecute(model);
                    else
                        OnReceiveDataHandle?.Invoke(model); //这里是多线程调用,不切换到主线程了
                    break;
                case NetCmd.Connect:
                    Connected = true;
                    break;
                case NetCmd.Identify:
                    UID = PreUserId = segment.ReadInt32();
                    if (segment.Position >= segment.Count) //此代码是兼容旧版本写法
                        return;
                    var adapterType = segment.ReadString();
                    var version = segment.ReadInt32();
                    if (version != Version)
                        InvokeInMainThread(OnUpdateVersion, version);
                    if (string.IsNullOrEmpty(adapterType))
                        return;
                    var type = AssemblyHelper.GetType(adapterType);
                    var adapter = (ISerializeAdapter)Activator.CreateInstance(type);
                    AddAdapter(adapter);
                    break;
                case NetCmd.OperationSync:
                    var operList = OnDeserializeOPT(segment);
                    InvokeInMainThread(OnOperationSync, operList);
                    break;
                case NetCmd.Ping:
                    RpcModels.Enqueue(new RPCModel(cmd: NetCmd.PingCallback, kernel: model.kernel, buffer: model.Buffer, serialize: false));
                    break;
                case NetCmd.PingCallback:
                    uint ticks = BitConverter.ToUInt32(model.buffer, model.index);
                    var delayTime = (uint)Environment.TickCount - ticks;
                    InvokeInMainThread(OnPingCallback, delayTime);
                    break;
                case NetCmd.P2P:
                    {
                        var address = segment.ReadInt64();
                        var port = segment.ReadInt32();
                        var endPoint = new IPEndPoint(address, port);
                        InvokeInMainThread(OnP2PCallback, endPoint);
                    }
                    break;
                case NetCmd.SyncVarP2P:
                    SyncVarHelper.SyncVarHandler(SyncVarDic, model.Buffer);
                    break;
                case NetCmd.UploadData:
                    DownloadDataHandler(segment);
                    break;
                case NetCmd.Download:
                    UploadDataHandler(segment.ReadByte(), segment.ReadInt32());
                    break;
                case NetCmd.QueueUp:
                    {
                        var totalCount = segment.ReadInt32();
                        var queueUpCount = segment.ReadInt32();
                        InvokeInMainThread(OnWhenQueuing, totalCount, queueUpCount);
                    }
                    break;
                case NetCmd.QueueCancellation:
                    InvokeInMainThread(OnQueueCancellation);
                    break;
                case NetCmd.ServerFull:
                    InvokeInMainThread(OnServerFull);
                    break;
                case NetCmd.SyncPropertyData:
                    InvokeInMainThread(OnSyncPropertyHandle, model); //属性同步没有用到Buffer
                    break;
                default:
                    InvokeInMainThread(model);
                    break;
            }
        }

        public virtual void UploadDataHandler(byte cmd, int fileId)
        {
            if (BigDataDic.TryGetValue(fileId, out BigData data))
                SendFile(cmd, fileId, data);
        }

        public virtual void DownloadDataHandler(ISegment segment)
        {
            var cmd = segment.ReadByte();
            var type = segment.ReadByte();
            var id = segment.ReadInt32();
            var length = segment.ReadInt32();
            var name = segment.ReadString();
            var buffer = segment.ReadByteArray();
            if (!BigDataDic.TryGetValue(id, out BigData data))
            {
                data = new BigData();
                if (type == 0)
                {
                    string path;
                    if (OnDownloadFileHandle != null)
                    {
                        path = OnDownloadFileHandle(name);
                        var path1 = Path.GetDirectoryName(path);
                        if (!Directory.Exists(path1))
                        {
                            NDebug.LogError("文件不存在! 或者文件路径字符串编码错误! 提示:可以使用Notepad++查看, 编码是ANSI,不是UTF8");
                            return;
                        }
                    }
                    else
                    {
                        int count = 0;
                        string downloadPath;
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
                        downloadPath = Net.Config.Config.BasePath + "/download/";
#else
                        downloadPath = Environment.CurrentDirectory + "/download/";
#endif
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
                BigDataDic.Add(id, data);
            }
            data.Stream.Write(buffer, 0, buffer.Length);
            data.Length += buffer.Length;
            if (data.Length >= length)
            {
                BigDataDic.Remove(id);
                data.Stream.Position = 0;
                InvokeInMainThread(() =>
                {
                    if (type == 0)
                    {
                        OnRevdFileProgress?.Invoke(new BigDataProgress(name, data.Length / (float)length * 100f, BigDataState.Complete));
                        var isDelete = true;
                        if (OnReceiveFileHandle != null)
                            isDelete = OnReceiveFileHandle(data);
                        data.Stream.Close();
                        if (isDelete)
                            File.Delete((data.Stream as FileStream).Name);
                    }
                    else
                    {
                        buffer = new byte[length];
                        data.Stream.Read(buffer, 0, length);
                        data.Stream.Dispose();
                        var model = new RPCModel(cmd: cmd, buffer: buffer);
                        OnReceiveDataHandle?.Invoke(model);
                    }
                });
            }
            else
            {
                var len = segment.Count;
                segment.SetPositionLength(0);
                segment.Write(cmd);
                segment.Write(id);
                Call(NetCmd.Download, segment.ToArray(false));
                segment.Count = len;
                if (Environment.TickCount >= recvFileTick)
                {
                    recvFileTick = Environment.TickCount + 1000;
                    InvokeInMainThread(OnRevdFileProgress, new BigDataProgress(name, data.Length / (float)length * 100f, BigDataState.Download));
                }
            }
        }

        protected virtual void InvokeInMainThread(RPCModel model)
        {
            model.Flush(); //先缓存起来, 当切换到主线程后才能得到正确的数据
            InvokeInMainThread(OnReceiveDataHandle, model);
        }

        protected void InvokeRevdBigDataProgress(int currValue, int dataCount)
        {
            float bfb = currValue / (float)dataCount * 100f;
            var progress = new BigDataProgress(bfb, BigDataState.Sending);
            InvokeInMainThread(OnRevdBigDataProgress, progress);
        }

        /// <summary>
        /// 添加操作, 跟Send方法类似，区别在于AddOperation方法是将所有要发送的数据收集成一堆数据后，等待时间间隔进行发送。
        /// 而Send则是直接发送
        /// </summary>
        /// <param name="func"></param>
        /// <param name="pars"></param>
        [Obsolete("此方法尽量少用,此方法有可能产生较大的数据，不要频繁发送!", false)]
        public void AddOperation(string func, params object[] pars)
        {
            AddOperation(NetCmd.CallRpc, func, pars);
        }

        /// <summary>
        /// 添加操作, 跟Send方法类似，区别在于AddOperation方法是将所有要发送的数据收集成一堆数据后，等待时间间隔进行发送。
        /// 而Send则是直接发送
        /// </summary>
        /// <param name="func"></param>
        /// <param name="pars"></param>
        [Obsolete("此方法尽量少用,此方法有可能产生较大的数据，不要频繁发送!", false)]
        public void AddOperation(int func, params object[] pars)
        {
            AddOperation(NetCmd.CallRpc, func, pars);
        }

        /// <summary>
        /// 添加操作, 跟Send方法类似，区别在于AddOperation方法是将所有要发送的数据收集成一堆数据后，等待时间间隔进行发送。
        /// 而Send则是直接发送
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="func"></param>
        /// <param name="pars"></param>
        [Obsolete("此方法尽量少用,此方法有可能产生较大的数据，不要频繁发送!", false)]
        public void AddOperation(byte cmd, string func, params object[] pars)
        {
            var segment = BufferPool.Take();
            OnSerializeRPC(segment, new RPCModel(cmd: cmd, kernel: true, protocol: func.CRCU32(), pars: pars));
            var opt = new Operation(cmd, segment.ToArray(true));
            AddOperation(opt);
        }

        /// <summary>
        /// 添加操作, 跟Send方法类似，区别在于AddOperation方法是将所有要发送的数据收集成一堆数据后，等待时间间隔进行发送。
        /// 而Send则是直接发送
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="func"></param>
        /// <param name="pars"></param>
        [Obsolete("此方法尽量少用,此方法有可能产生较大的数据，不要频繁发送!", false)]
        public void AddOperation(byte cmd, uint func, params object[] pars)
        {
            var segment = BufferPool.Take();
            OnSerializeRPC(segment, new RPCModel(cmd: cmd, kernel: true, protocol: func, pars: pars));
            var opt = new Operation(cmd, segment.ToArray(true));
            AddOperation(opt);
        }

        /// <summary>
        /// 添加操作, 跟Send方法类似，区别在于AddOperation方法是将所有要发送的数据收集成一堆数据后，等待时间间隔进行发送。
        /// 而Send则是直接发送
        /// </summary>
        /// <param name="opt"></param>
        public void AddOperation(in Operation opt)
        {
            operations.Add(opt);
        }

        /// <summary>
        /// 添加操作, 跟Send方法类似，区别在于AddOperation方法是将所有要发送的数据收集成一堆数据后，等待时间间隔进行发送。
        /// 而Send则是直接发送
        /// </summary>
        /// <param name="opts"></param>
        public void AddOperations(ICollection<Operation> opts)
        {
            foreach (Operation opt in opts)
                AddOperation(opt);
        }

        /// <summary>
        /// 后台线程发送心跳包
        /// </summary>
        protected virtual bool HeartHandler()
        {
            try
            {
                if (!Connected)
                    InternalReconnection();//尝试连接执行
                else if (heart++ < HeartLimit)
                    Call(NetCmd.SendHeartbeat, new byte[1]);
                else//连接中断事件执行
                    NetworkException(new SocketException((int)SocketError.Disconnecting));
            }
            catch { }
            return openClient & CurrReconnect < ReconnectCount;
        }

        /// <summary>
        /// 测试服务器网络情况
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool PingServer(string ip)
        {
            var ping = new Ping();
            var options = new PingOptions { DontFragment = true };
            var data = "Test";
            var buffer = Encoding.ASCII.GetBytes(data);
            var timeout = 1000;
            var reply = ping.Send(ip, timeout, buffer, options);
            if (reply.Status == IPStatus.Success)
                return true;
            return false;
        }

        /// <summary>
        /// 内部断线重新连接
        /// </summary>
        protected virtual void InternalReconnection()
        {
            if (!AutoReconnecting)
                return;
            Reconnection();
        }

        /// <summary>
        /// 断线重新连接
        /// </summary>
        public void Reconnection()
        {
            if (NetworkState == NetworkState.Connection | NetworkState == NetworkState.TryToConnect |
                NetworkState == NetworkState.ConnectClosed | NetworkState == NetworkState.Reconnect)
                return;
            if (CurrReconnect >= ReconnectCount)//如果想断线不需要重连,则直接返回
            {
                NetworkState = NetworkState.ConnectFailed;
                InvokeInMainThread(OnConnectFailedHandle);
                Close();
                NDebug.LogError($"连接失败!请检查网络是否异常(无重连次数)");
                return;
            }
            Client?.Close();
            UID = 0;
            CurrReconnect++;
            NetworkState = NetworkState.TryToConnect;
            SetHeartInterval(ReconnectInterval);
            InvokeInMainThread(OnTryToConnectHandle);
            NDebug.Log($"尝试重连:{CurrReconnect}...");
            ConnectResult(host, port, localPort, OnReconnectConnected);
        }

        private void OnReconnectConnected(bool isConnect)
        {
            if (!openClient)
                return;
            if (isConnect)
            {
                CurrReconnect = 0;
                heart = 0;
                NetworkState = NetworkState.Reconnect;
                RpcModels = new QueueSafe<RPCModel>();
                SetHeartInterval(HeartInterval);
                InvokeInMainThread(OnReconnectHandle);
                NDebug.Log("重连成功...");
            }
            else if (CurrReconnect >= ReconnectCount)//尝试maxFrequency次重连，如果失败则退出线程
            {
                NetworkState = NetworkState.ConnectFailed;
                InvokeInMainThread(OnConnectFailedHandle);
                Close();
                NDebug.LogError($"连接失败!请检查网络是否异常");
            }
            else
            {
                NetworkState = NetworkState.TryToConnectFailed;
                InvokeInMainThread(TryToConnectFailedHandle);
            }
        }

        /// <summary>
        /// 关闭连接,释放线程以及所占资源
        /// </summary>
        /// <param name="isWait">true:等待内部1秒结束所有线程再关闭? false:直接关闭</param>
        /// <param name="millisecondsTimeout">等待毫秒数</param>
        public virtual void Close(bool isWait = true, int millisecondsTimeout = 100)
        {
            var isDispose = openClient;
            if (Connected & openClient & NetworkState == NetworkState.Connected)
                Disconnect(false, false);
            Connected = false;
            openClient = false;
            NetworkState = NetworkState.ConnectClosed;
            InvokeInMainThread(OnCloseConnectHandle);
            if (isWait) Thread.Sleep(millisecondsTimeout);//给update线程一秒的时间处理关闭事件
            AbortedThread();
            Client?.Close();
            Client = null;
            RpcModels = new QueueSafe<RPCModel>();
            BufferStream?.Close();
            BufferStream = null;
            stacking = 0;
            stackingOffset = 0;
            stackingCount = 0;
            UID = 0;
            PreUserId = 0;
            CurrReconnect = 0;
            if (Instance == this) Instance = null;
            if (Gcp != null) Gcp.Dispose();
            if (isDispose) NDebug.Log("客户端关闭成功!"); //只有打开状态下才会提示
        }

        #region 远程过程调用
        public void Call(uint protocol, params object[] pars) => Call(NetCmd.CallRpc, protocol, null, pars);
        public void Call(byte cmd, uint protocol, params object[] pars) => Call(cmd, protocol, null, pars);
        public void Call(byte[] buffer) => Call(NetCmd.OtherCmd, 0U, buffer);
        public void Call(byte cmd, byte[] buffer) => Call(cmd, 0U, buffer);
        public void Call(string func, params object[] pars) => Call(NetCmd.CallRpc, func.CRCU32(), null, pars);
        public void Call(byte cmd, string func, params object[] pars) => Call(cmd, func.CRCU32(), null, pars);

        public void Call(byte cmd, uint protocol, byte[] buffer, params object[] pars)
        {
            if (buffer != null)
                Call(new RPCModel(cmd: cmd, kernel: false, protocol: protocol, buffer: buffer, serialize: false));
            else
                Call(new RPCModel(cmd: cmd, kernel: true, protocol: protocol, pars: pars, serialize: true));
        }

        public virtual void Call(RPCModel model)
        {
            if (!Connected)
                return;
            if (RpcModels.Count >= LimitQueueCount)
            {
                OnDataQueueOverflow?.Invoke();
                return;
            }
            RpcModels.Enqueue(model);
        }
        #endregion

        #region 同步远程调用, 跟Http协议一样, 请求必须有回应 请求和回应方法都是相同的, 都是根据protocol请求和回应
        public UniTask<RPCModelTask> Request(uint protocol, params object[] pars)
            => Request(NetCmd.CallRpc, protocol, 5000U, true, false, null, pars);
        public UniTask<RPCModelTask> Request(uint protocol, uint timeoutMilliseconds, params object[] pars)
            => Request(NetCmd.CallRpc, protocol, timeoutMilliseconds, true, false, null, pars);
        public UniTask<RPCModelTask> Request(uint protocol, uint timeoutMilliseconds, bool intercept, params object[] pars)
            => Request(NetCmd.CallRpc, protocol, timeoutMilliseconds, intercept, false, null, pars);
        public UniTask<RPCModelTask> Request(byte cmd, uint protocol, uint timeoutMilliseconds, params object[] pars)
            => Request(cmd, protocol, timeoutMilliseconds, true, false, null, pars);
        public UniTask<RPCModelTask> Request(byte cmd, uint protocol, uint timeoutMilliseconds, bool intercept, params object[] pars)
            => Request(cmd, protocol, timeoutMilliseconds, intercept, false, null, pars);
        #endregion

        public async UniTask<RPCModelTask> Request(byte cmd, uint protocol, uint timeoutMilliseconds, bool intercept, bool serialize, byte[] buffer, params object[] pars)
        {
            tokenLock.Enter();
            var requestTask = new RPCModelTask();
            if (!RpcCollectDic.TryGetValue(protocol, out RPCMethodBody body))
                RpcCollectDic.Add(protocol, body = new RPCMethodBody()); //并行发起导致问题
            J: uint token = tokenCount++;
            if (token == 0)
                goto J;
            if (buffer != null)
            {
                Call(new RPCModel(cmd: cmd, kernel: false, buffer: buffer, serialize: serialize, protocol: protocol, token: token));
            }
            else
            {
                var model = new RPCModel(cmd: cmd, kernel: true, protocol: protocol, pars: pars, serialize: !serialize, token: token);
                if (serialize)
                {
                    var segment = BufferPool.Take();
                    OnSerializeRPC(segment, model);
                    model.buffer = segment.ToArray(true);
                }
                Call(model);
            }
            if (timeoutMilliseconds == 0)
                timeoutMilliseconds = 5000U;
            requestTask.intercept = intercept;
            var addResult = body.RequestDict.TryAdd(token, requestTask);
            if (!addResult)
                NDebug.LogError($"请求Token添加失败! token:{token}");
            tokenLock.Exit();
            await UniTaskNetExtensions.WaitCallback(LoopEvent, (int)timeoutMilliseconds, requestTask);
            if (!requestTask.IsCompleted)
                body.RequestDict.Remove(token);
#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
            await UniTaskNetExtensions.SwitchToMainThread(WorkerQueue);
#endif
            return requestTask;
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
            SetHeartInterval(interval);
        }

        protected void SetHeartInterval(int interval)
        {
            LoopEvent.ResetTimeInterval(heartHandlerID, interval, true);
        }

        /// <summary>
        /// ping测试网络延迟, 通过<see cref="OnPingCallback"/>事件回调
        /// </summary>
        public void Ping()
        {
            uint tick = (uint)Environment.TickCount;
            Call(NetCmd.Ping, BitConverter.GetBytes(tick));
        }

        /// <summary>
        /// ping测试网络延迟, 此方法帮你监听<see cref="OnPingCallback"/>事件, 如果不使用的时候必须保证能移除委托, 建议不要用框名函数, 那样会无法移除委托
        /// </summary>
        /// <param name="callback"></param>
        public void Ping(Action<uint> callback)
        {
            uint tick = (uint)Environment.TickCount;
            Call(NetCmd.Ping, BitConverter.GetBytes(tick));
            OnPingCallback += callback;
        }

        /// <summary>
        /// 添加适配器
        /// </summary>
        /// <param name="adapter"></param>
        public void AddAdapter(IAdapter adapter)
        {
            if (adapter is ISerializeAdapter ser)
                AddAdapter(AdapterType.Serialize, ser);
            else if (adapter is IRPCAdapter rpc)
                AddAdapter(AdapterType.RPC, rpc);
            else if (adapter is INetworkEvtAdapter evt)
                AddAdapter(AdapterType.NetworkEvt, evt);
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
                    var rpc = (IRPCAdapter)adapter;
                    OnAddRpcHandle = rpc.AddRpc;
                    OnRPCExecute = rpc.OnRpcExecute;
                    OnRemoveRpc = rpc.RemoveRpc;
                    break;
                case AdapterType.NetworkEvt:
                    BindNetworkHandle((INetworkHandle)adapter);
                    break;
                case AdapterType.Package:
                    PackageAdapter = (IPackageAdapter)adapter;
                    break;
            }
        }

        /// <summary>
        /// 添加网络状态事件处理
        /// </summary>
        /// <param name="listen">要监听的网络状态</param>
        /// <param name="action">监听网络状态的回调方法</param>
        public void AddStateHandler(NetworkState listen, Action action)
        {
            switch (listen)
            {
                case NetworkState.Connected:
                    OnConnectedHandle += action;
                    break;
                case NetworkState.ConnectFailed:
                    OnConnectFailedHandle += action;
                    break;
                case NetworkState.ConnectLost:
                    OnConnectLostHandle += action;
                    break;
                case NetworkState.Reconnect:
                    OnReconnectHandle += action;
                    break;
                case NetworkState.ConnectClosed:
                    OnCloseConnectHandle += action;
                    break;
                case NetworkState.Disconnect:
                    OnDisconnectHandle += action;
                    break;
                case NetworkState.TryToConnect:
                    OnTryToConnectHandle += action;
                    break;
                case NetworkState.OnWhenQueuing:

                    break;
                case NetworkState.OnQueueCancellation:
                    OnQueueCancellation += action;
                    break;
            }
        }

        /// <summary>
        /// 字段,属性同步处理线程
        /// </summary>
        protected virtual void SyncVarHandler()
        {
            var buffer = SyncVarHelper.CheckSyncVar(true, SyncVarDic);
            if (buffer != null)
                Call(NetCmd.SyncVarP2P, buffer);
        }

        /// <summary>
        /// 发送文件, 服务器可以通过重写<see cref="Server.ServerBase{Player, Scene}.OnReceiveFile"/>方法来接收 或 使用事件<see cref="Server.ServerBase{Player, Scene}.OnReceiveFileHandle"/>来监听并处理
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="bufferSize">每次发送数据大小，如果想最大化发送，你可以设置bufferSize参数为PackageSize - 2048</param>
        /// <returns></returns>
        public bool SendFile(string filePath, int bufferSize = 50000)
        {
            var path1 = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(path1))
            {
                NDebug.LogError("文件不存在! 或者文件路径字符串编码错误! 提示:可以使用Notepad++查看, 编码是ANSI,不是UTF8");
                return false;
            }
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            SendFile(NetCmd.UploadData, fileStream, Path.GetFileName(filePath), bufferSize);
            return true;
        }

        private void SendFile(byte cmd, Stream stream, string name, int bufferSize = 50000)
        {
            var data = new BigData
            {
                Id = stream.GetHashCode(),
                Name = name,
                Stream = stream,
                bufferSize = bufferSize
            };
            BigDataDic.Add(data.Id, data);
            SendFile(cmd, data.Id, data);
        }

        private void SendFile(byte cmd, int fileId, BigData fileData)
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
            segment.Write((int)fileData.Stream.Length);
            segment.Write(fileData.Name);
            segment.Write(buffer);
            Call(NetCmd.UploadData, segment.ToArray(true));
            if (complete)
            {
                if (OnSendFileProgress != null & type == 0)
                    InvokeInMainThread(OnSendFileProgress, new BigDataProgress(fileData.Name, stream.Position / (float)stream.Length * 100f, BigDataState.Complete));
                BigDataDic.Remove(fileId);
                fileData.Stream.Close();
            }
            else if (Environment.TickCount >= sendFileTick)
            {
                sendFileTick = Environment.TickCount + 1000;
                if (OnSendFileProgress != null & type == 0)
                    InvokeInMainThread(OnSendFileProgress, new BigDataProgress(fileData.Name, stream.Position / (float)stream.Length * 100f, BigDataState.Sending));
            }
        }

        /// <summary>
        /// 检查send方法的发送队列是否已到达极限, 到达极限则不允许新的数据放入发送队列, 需要等待队列消耗后才能放入新的发送数据
        /// </summary>
        /// <returns>是否可发送数据</returns>
        public bool CheckCall()
        {
            return RpcModels.Count < LimitQueueCount;
        }

        private void OnDataQueueOverflowInternal()
        {
            NDebug.LogError("数据缓存列表超出限制!");
        }

        public void Dispose()
        {
            Close();
        }

        /// <summary>
        /// 当设置配置信息
        /// </summary>
        /// <param name="args"></param>
        public virtual void OnSetConfigInfo(params object[] args)
        {
        }

        /// <summary>
        /// 设置配置
        /// </summary>
        /// <param name="config"></param>
        public void SetConfig(ClientConfig config)
        {
            if (config == null)
                return;
            HeartInterval = config.HeartInterval;
            HeartLimit = config.HeartLimit;
            PackageSize = config.PackageSize;
            MTU = config.MTU;
            RTO = config.RTO;
            MTPS = config.MTPS;
            FlowControl = config.FlowControl;
            PackageLength = config.PackageLength;
            LimitQueueCount = config.LimitQueueCount;
            AutoReconnecting = config.AutoReconnecting;
            ReconnectCount = config.ReconnectCount;
            ReconnectInterval = config.ReconnectInterval;
            SendBufferSize = config.SendBufferSize;
            ReceiveBufferSize = config.ReceiveBufferSize;
        }
    }
}