namespace Net.Client
{
    using global::System;
    using Net.Share;
    using Net.Plugins;
    using Net.System;
    using global::System.Net.Sockets;

    /// <summary>
    /// Udp网络客户端
    /// 在安卓端必须设置可以后台运行, 如果不设置,当你按下home键后,app的所有线程将会被暂停,这会影响网络心跳检测线程,导致网络中断
    /// 解决方法 : 在android项目AndroidManifest.xml文件中的activity中添加如下内容：
    /// android:configChanges="fontScale|keyboard|keyboardHidden|locale|mnc|mcc|navigation|orientation|screenLayout|screenSize|smallestScreenSize|uiMode|touchscreen" 
    /// 详情请看此博文:https://www.cnblogs.com/nanwei/p/9125316.html
    /// 或这个博文: http://www.voidcn.com/article/p-yakpcmce-bpk.html
    /// </summary>
    [Serializable]
    public class UdpClient : ClientBase
    {
        public override int MTU { get => Gcp.MTU; set => Gcp.MTU = (ushort)value; }
        public override int RTO { get => Gcp.RTO; set => Gcp.RTO = value; }
        public override int MTPS { get => Gcp.MTPS; set => Gcp.MTPS = value; }
        public override FlowControlMode FlowControl { get => Gcp.FlowControl; set => Gcp.FlowControl = value; }
        public override Action<BigDataProgress> OnRevdRTProgress { get => Gcp.OnRevdProgress; set => Gcp.OnRevdProgress = value; }
        public override Action<BigDataProgress> OnCallProgress { get => Gcp.OnSendProgress; set => Gcp.OnSendProgress = value; }
        /// <summary>
        /// 构造udp可靠客户端
        /// </summary>
        public UdpClient()
        {
            Gcp = new GcpKernel();
            Gcp.OnSender += (remotePoint, segment) =>
            {
                Client?.Send(segment.Buffer, segment.Offset, segment.Count, SocketFlags.None);
            };
        }

        /// <summary>
        /// 构造udp可靠客户端
        /// </summary>
        /// <param name="useUnityThread">使用unity多线程?</param>
        public UdpClient(bool useUnityThread) : this()
        {
            UseUnityThread = useUnityThread;
        }

        /// <summary>
        /// 获取p2p IP和端口, 通过client.OnP2PCallback事件回调
        /// </summary>
        /// <param name="uid"></param>
        public void GetP2P(int uid)
        {
            Call(NetCmd.P2P, BitConverter.GetBytes(uid));
        }
    }

    /// <summary>
    /// Gcp协议
    /// </summary>
    public class GcpClient : UdpClient
    {
        /// <summary>
        /// 构造gdp可靠客户端
        /// </summary>
        public GcpClient() : base() { }
        /// <summary>
        /// 构造gdp可靠客户端
        /// </summary>
        /// <param name="useUnityThread">使用unity多线程?</param>
        public GcpClient(bool useUnityThread) : base(useUnityThread) { }
    }
}
