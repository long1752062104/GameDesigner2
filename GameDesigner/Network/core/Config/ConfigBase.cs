using Net.Share;

namespace Net.Config
{
    /// <summary>
    /// 网络配置基类
    /// </summary>
    public class ConfigBase
    {
        /// <summary>
        /// 心跳时间间隔, 默认每1秒检查一次玩家是否离线, 玩家心跳确认为5次, 如果超出5次 则移除玩家客户端. 确认玩家离线总用时5秒, 
        /// 如果设置的值越小, 确认的速度也会越快. 值太小有可能出现直接中断问题, 设置的最小值在100以上
        /// </summary>
        public int HeartInterval { get; set; } = 1000;
        /// <summary>
        /// <para>心跳检测次数, 默认为5次检测, 如果5次发送心跳给客户端或服务器, 没有收到回应的心跳包, 则进入断开连接处理</para>
        /// <para>当一直有数据往来时是不会发送心跳数据的, 只有当没有数据往来了, 才会进入发送心跳数据</para>
        /// </summary>
        public byte HeartLimit { get; set; } = 5;
        /// <summary>
        /// 接收缓存最大的数据长度 默认可缓存5242880(5M)的数据长度
        /// </summary>
        public int PackageSize { get; set; } = 1024 * 1024 * 5;
        /// <summary>
        /// <para>（Maxium Transmission Unit）最大传输单元, 最大传输单元为1500字节, 这里默认为50000, 如果数据超过50000,则是该框架进行分片. 传输层则需要分片为50000/1472=34个数据片</para>
        /// <para>------ 局域网可以设置为50000, 公网需要设置为1300 或 1400, 如果设置为1400还是发送失败, 则需要设置为1300或以下进行测试 ------</para>
        /// <para>1.链路层：以太网的数据帧的长度为(64+18)~(1500+18)字节，其中18是数据帧的帧头和帧尾，所以数据帧的内容最大为1500字节（不包括帧头和帧尾），即MUT为1500字节</para>
        /// <para>2.网络层：IP包的首部要占用20字节，所以这里的MTU＝1500－20＝1480字节</para>
        /// <para>3.传输层：UDP包的首部要占有8字节，所以这里的MTU＝1480－8＝1472字节</para>
        /// <see langword="注意:服务器和客户端的MTU属性的值必须保持一致性,否则分包的数据将解析错误!"/> <see cref="Server.ServerBase{Player, Scene}.MTU"/>
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
        /// 组包数量，如果是一些小数据包，最多可以组合多少个？ 默认是组合1000个后发送
        /// </summary>
        public int PackageLength { get; set; } = 1000;
        /// <summary>
        /// 限制发送队列长度
        /// </summary>
        public int LimitQueueCount { get; set; } = ushort.MaxValue;
        /// <summary>
        /// 自动断线重新连接, 默认是true
        /// </summary>
        public bool AutoReconnecting { get; set; } = true;
        /// <summary>
        /// 断线重连次数, 默认会重新连接10次，如果连接10次都失败，则会关闭客户端并释放占用的资源
        /// </summary>
        public int ReconnectCount { get; set; } = 10;
        /// <summary>
        /// 断线重连间隔, 默认间隔2秒
        /// </summary>
        public int ReconnectInterval { get; set; } = 2000;
        /// <summary>
        /// 每次发送数据间隔，每秒大概执行1000次
        /// </summary>
        public int SendInterval { get; set; } = 1;
        /// <summary>
        /// 设置Socket的发送缓冲区大小, 也叫做窗口大小
        /// </summary>
        public int SendBufferSize { get; set; } = ushort.MaxValue;
        /// <summary>
        /// 设置Socket的接收缓冲区大小, 也叫做窗口大小
        /// </summary>
        public int ReceiveBufferSize { get; set; } = ushort.MaxValue;
    }
}
