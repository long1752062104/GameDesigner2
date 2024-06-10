namespace Net.Server
{
    using global::System;
    using global::System.IO;
    using global::System.Net;
    using global::System.Runtime.InteropServices;
    using global::System.Threading;
    using global::System.Net.Sockets;
    using Kcp;
    using Net.Share;
    using Net.System;
    using Net.Helper;
    using static Kcp.KcpLib;
    using Net.Event;

    /// <summary>
    /// kcp服务器
    /// <para>Player:当有客户端连接服务器就会创建一个Player对象出来, Player对象和XXXClient是对等端, 每当有数据处理都会通知Player对象. </para>
    /// <para>Scene:你可以定义自己的场景类型, 比如帧同步场景处理, mmorpg场景什么处理, 可以重写Scene的Update等等方法实现每个场景的更新和处理. </para>
    /// </summary>
    /// <typeparam name="Player"></typeparam>
    /// <typeparam name="Scene"></typeparam>
    public class KcpServer<Player, Scene> : ServerBase<Player, Scene> where Player : KcpPlayer, new() where Scene : NetScene<Player>, new()
    {
        private ikcp_malloc_hook ikcp_Malloc;
        private ikcp_free_hook ikcp_Free;

        public override void Start(ushort port = 9543)
        {
#if SERVICE
            var path = AppDomain.CurrentDomain.BaseDirectory;
            if (!File.Exists(PathHelper.Combine(path, "kcp.dll")))
                throw new FileNotFoundException($"kcp.dll没有在程序根目录中! 请从GameDesigner文件夹下找到kcp.dll复制到{path}目录下.");
#endif
            base.Start(port);
            ikcp_Malloc = Ikcp_malloc_hook;
            ikcp_Free = Ikcp_Free;
            var mallocPtr = Marshal.GetFunctionPointerForDelegate(ikcp_Malloc);
            var freePtr = Marshal.GetFunctionPointerForDelegate(ikcp_Free);
            ikcp_allocator(mallocPtr, freePtr);
        }

        protected override void CreateServerSocket(ushort port)
        {
            Server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
            {
                SendBufferSize = SendBufferSize,
                ReceiveBufferSize = ReceiveBufferSize
            };
            var address = new IPEndPoint(IPAddress.Any, port);
            Server.Bind(address);
            try //在安卓启动服务器时忽略此错误, 或者其他平台错误
            {
                uint IOC_IN = 0x80000000;
                uint IOC_VENDOR = 0x18000000;
                uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
                Server.IOControl((int)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);//udp远程关闭现有连接方案
            }
            catch (Exception ex)
            {
                NDebug.LogWarning(ex);
            }
        }

        private unsafe IntPtr Ikcp_malloc_hook(int size)
        {
            return Marshal.AllocHGlobal(size);
        }

        private unsafe void Ikcp_Free(IntPtr ptr)
        {
            Marshal.FreeHGlobal(ptr);
        }

        protected override void AcceptHander(Player client, params object[] args)
        {
            client.Server = Server;
            var kcp = ikcp_create(MTU, (IntPtr)1);
            var output = Marshal.GetFunctionPointerForDelegate(client.output);
            ikcp_setoutput(kcp, output);
            ikcp_wndsize(kcp, SendBufferSize, ReceiveBufferSize);
            ikcp_nodelay(kcp, 1, 10, 2, 1);
            client.Kcp = kcp;
        }

        protected unsafe override void ResolveDataQueue(Player client, ref bool isSleep, uint tick)
        {
            if (client.RevdQueue.TryDequeue(out var segment))
            {
                fixed (byte* p = &segment.Buffer[0])
                    ikcp_input(client.Kcp, p, segment.Count);
                BufferPool.Push(segment);
                client.heart = 0;
            }
            int len;
            if ((len = ikcp_peeksize(client.Kcp)) > 0)
            {
                var segment1 = BufferPool.Take(len);
                fixed (byte* p1 = &segment1.Buffer[0])
                {
                    segment1.Count = ikcp_recv(client.Kcp, p1, len);
                    ResolveBuffer(client, ref segment1);
                    BufferPool.Push(segment1);
                }
            }
        }

        protected override void OnClientTick(Player client, uint tick)
        {
            ikcp_update(client.Kcp, tick);
        }

        protected unsafe override void SendByteData(Player client, ISegment buffer)
        {
            if (!client.Connected)
                return;
            if (buffer.Count <= frame)//解决长度==6的问题(没有数据)
                return;
            sendAmount++;
            sendCount += buffer.Count;
            fixed (byte* p = &buffer.Buffer[0])
            {
                int count = ikcp_send(client.Kcp, p, buffer.Count);
                if (count < 0)
                    OnSendErrorHandle?.Invoke(client, buffer);
            }
        }
    }

    /// <summary>
    /// 默认kcp服务器，当不需要处理Player对象和Scene对象时可使用
    /// </summary>
    public class KcpServer : KcpServer<KcpPlayer, NetScene<KcpPlayer>>
    {
    }
}
