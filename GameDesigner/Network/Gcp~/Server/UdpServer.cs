namespace Net.Server
{
    using global::System;
    using global::System.Net;
    using global::System.Net.Sockets;
    using Net.Share;
    using Net.System;

    /// <summary>
    /// Udp网络服务器
    /// <para>Player:当有客户端连接服务器就会创建一个Player对象出来, Player对象和XXXClient是对等端, 每当有数据处理都会通知Player对象. </para>
    /// <para>Scene:你可以定义自己的场景类型, 比如帧同步场景处理, mmorpg场景什么处理, 可以重写Scene的Update等等方法实现每个场景的更新和处理. </para>
    /// </summary>
    public class UdpServer<Player, Scene> : ServerBase<Player, Scene> where Player : NetPlayer, new() where Scene : NetScene<Player>, new()
    {
        protected override void CreateServerSocket(ushort port)
        {
            Server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Server.SendBufferSize = SendBufferSize;
            Server.ReceiveBufferSize = ReceiveBufferSize;
            var address = new IPEndPoint(IPAddress.Any, port);
            Server.Bind(address);
#if !UNITY_ANDROID && WINDOWS//在安卓启动服务器时忽略此错误
            uint IOC_IN = 0x80000000;
            uint IOC_VENDOR = 0x18000000;
            uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
            Server.IOControl((int)SIO_UDP_CONNRESET, new byte[] { global::System.Convert.ToByte(false) }, null);//udp远程关闭现有连接方案
#endif
        }

        protected override void AcceptHander(Player client, params object[] args)
        {
            client.Gcp = new Plugins.GcpKernel();
            client.Gcp.MTU = (ushort)MTU;
            client.Gcp.RTO = RTO;
            client.Gcp.MTPS = MTPS;
            client.Gcp.FlowControl = FlowControl;
            client.Gcp.RemotePoint = client.RemotePoint;
            client.Gcp.OnSender += (remotePoint, segment) =>
            {
                Server.SendTo(segment.Buffer, segment.Offset, segment.Count, SocketFlags.None, remotePoint);
            };
        }

        protected override void DataCRCHandler(Player client, ISegment buffer, bool isTcp)
        {
            base.DataCRCHandler(client, buffer, isTcp);
        }
    }

    /// <summary>
    /// 默认udp服务器，当不需要处理Player对象和Scene对象时可使用
    /// </summary>
    public class UdpServer : UdpServer<NetPlayer, DefaultScene> { }

    /// <summary>
    /// Gcp网络服务器
    /// <para>Player:当有客户端连接服务器就会创建一个Player对象出来, Player对象和XXXClient是对等端, 每当有数据处理都会通知Player对象. </para>
    /// <para>Scene:你可以定义自己的场景类型, 比如帧同步场景处理, mmorpg场景什么处理, 可以重写Scene的Update等等方法实现每个场景的更新和处理. </para>
    /// </summary>
    public class GcpServer<Player, Scene> : UdpServer<Player, Scene> where Player : NetPlayer, new() where Scene : NetScene<Player>, new() { }

    /// <summary>
    /// 默认gcp服务器，当不需要处理Player对象和Scene对象时可使用
    /// </summary>
    public class GcpServer : GcpServer<NetPlayer, DefaultScene> { }
}