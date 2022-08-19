namespace Net.Client
{
    using Net.Share;
    using global::System;
    using global::System.IO;
    using global::System.Net.Sockets;
    using global::System.Runtime.InteropServices;
    using global::System.Threading;
    using global::System.Threading.Tasks;
    using Kcp;
    using static Kcp.KcpLib;
    using global::System.Net;
    using global::System.Reflection;
    using Net.System;

    /// <summary>
    /// kcp客户端
    /// </summary>
    [Serializable]
    public unsafe class KcpClient : ClientBase
    {
        private readonly IntPtr kcp;
        private readonly outputCallback output;

        public KcpClient() : base()
        {
            kcp = ikcp_create(1400, (IntPtr)1);
            output = new outputCallback(Output);
            IntPtr outputPtr = Marshal.GetFunctionPointerForDelegate(output);
            ikcp_setoutput(kcp, outputPtr);
            ikcp_wndsize(kcp, ushort.MaxValue, ushort.MaxValue);
            ikcp_nodelay(kcp, 1, 10, 2, 1);
        }

        public KcpClient(bool useUnityThread) : this()
        {
            UseUnityThread = useUnityThread;
        }

        private byte[] addressBuffer;
        internal byte[] RemoteAddressBuffer()
        {
            if (addressBuffer != null)
                return addressBuffer;
            SocketAddress socketAddress = Client.RemoteEndPoint.Serialize();
            addressBuffer = (byte[])socketAddress.GetType().GetField("m_Buffer", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(socketAddress);
            return addressBuffer;
        }

        public unsafe int Output(byte* buf, int len, IntPtr kcp, IntPtr user)
        {
#if WINDOWS
            Win32KernelAPI.sendto(Client.Handle, buf, len, SocketFlags.None, RemoteAddressBuffer(), 16);
#else
            byte[] buff = new byte[len];
            Marshal.Copy(new IntPtr(buf), buff, 0, len);
            Client.Send(buff, 0, len, SocketFlags.None);
#endif
            return 0;
        }

        public override void Receive()
        {
            if (Client.Poll(1, SelectMode.SelectRead))
            {
                var segment = BufferPool.Take(65507);
                segment.Count = Client.Receive(segment, 0, segment.Length, SocketFlags.None, out SocketError error);
                if (error != SocketError.Success)
                {
                    BufferPool.Push(segment);
                    return;
                }
                if (segment.Count == 0)
                {
                    BufferPool.Push(segment);
                    return;
                }
                receiveCount += segment.Count;
                receiveAmount++;
                heart = 0;
                fixed (byte* p = &segment.Buffer[0])
                    ikcp_input(kcp, p, segment.Count);
                ikcp_update(kcp, (uint)Environment.TickCount);
                int len;
                while ((len = ikcp_peeksize(kcp)) > 0)
                {
                    segment.SetPositionLength(0);
                    segment.Count = len;
                    fixed (byte* p1 = &segment.Buffer[0])
                    {
                        ikcp_recv(kcp, p1, len);
                        ResolveBuffer(ref segment, false);
                    }
                    revdLoopNum++;
                }
                BufferPool.Push(segment);
            }
            else
            {
                Thread.Sleep(1);
            }
        }

        protected override void SendByteData(byte[] buffer, bool reliable)
        {
            sendCount += buffer.Length;
            sendAmount++;
            fixed (byte* p = &buffer[0])
            {
                int count = ikcp_send(kcp, p, buffer.Length);
                ikcp_update(kcp, (uint)Environment.TickCount);
                if (count < 0)
                    OnSendErrorHandle?.Invoke(buffer, reliable);
            }
        }

        protected override void SendRTDataHandle()
        {
            SendDataHandle(rtRPCModels, true);
        }

        public override void Close(bool await = true, int millisecondsTimeout = 1000)
        {
            base.Close(await);
            addressBuffer = null;
        }

        private class KcpCallback
        {
            public Socket Client;

            public KcpCallback(Socket Client)
            {
                this.Client = Client;
            }

            public unsafe int Output(byte* buf, int len, IntPtr kcp, IntPtr user)
            {
                byte[] buff = new byte[len];
                Marshal.Copy(new IntPtr(buf), buff, 0, len);
                Client.Send(buff, 0, len, SocketFlags.None);
                return 0;
            }
        }

        /// <summary>
        /// udp压力测试
        /// </summary>
        /// <param name="ip">服务器ip</param>
        /// <param name="port">服务器端口</param>
        /// <param name="clientLen">测试客户端数量</param>
        /// <param name="dataLen">每个客户端数据大小</param>
        public static CancellationTokenSource Testing(string ip, int port, int clientLen, int dataLen)
        {
            var cts = new CancellationTokenSource();
            Task.Run(() =>
            {
                var kcps = new IntPtr[clientLen];
                var outputs = new outputCallback[clientLen];
                var sockets = new Socket[clientLen];
                for (int i = 0; i < clientLen; i++)
                {
                    Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    socket.Connect(ip, port);
                    KcpCallback callback = new KcpCallback(socket);
                    IntPtr kcp = ikcp_create(1400, (IntPtr)1);
                    outputCallback output = new outputCallback(callback.Output);
                    IntPtr outputPtr = Marshal.GetFunctionPointerForDelegate(output);
                    ikcp_setoutput(kcp, outputPtr);
                    GC.KeepAlive(output);
                    outputs[i] = output;
                    ikcp_wndsize(kcp, 128, 128);
                    ikcp_nodelay(kcp, 1, 10, 2, 1);
                    kcps[i] = kcp;
                    sockets[i] = socket;
                }
                var buffer = new byte[dataLen];
                using (MemoryStream stream = new MemoryStream(512))
                {
                    int crcIndex = 0;
                    byte crcCode = 0x2d;
                    stream.Write(new byte[4], 0, 4);
                    stream.WriteByte((byte)crcIndex);
                    stream.WriteByte(crcCode);
                    RPCModel rPCModel = new RPCModel(NetCmd.Local, buffer);
                    stream.WriteByte((byte)(rPCModel.kernel ? 68 : 74));
                    stream.WriteByte(rPCModel.cmd);
                    stream.Write(BitConverter.GetBytes(rPCModel.buffer.Length), 0, 4);
                    stream.Write(rPCModel.buffer, 0, rPCModel.buffer.Length);
                    stream.Position = 0;
                    int len = (int)stream.Length - 6;
                    stream.Write(BitConverter.GetBytes(len), 0, 4);
                    stream.Position = len + 6;
                    buffer = stream.ToArray();
                }
                var buffer1 = new byte[65507];
                while (!cts.IsCancellationRequested)
                {
                    Thread.Sleep(31);
                    for (int i = 0; i < kcps.Length; i++)
                    {
                        fixed (byte* p = &buffer[0])
                        {
                            int count = ikcp_send(kcps[i], p, buffer.Length);
                            ikcp_update(kcps[i], (uint)Environment.TickCount);
                        }
                        if (sockets[i].Poll(0, SelectMode.SelectRead))
                        {
                            int count = sockets[i].Receive(buffer1);
                            fixed (byte* p = &buffer1[0])
                            {
                                ikcp_input(kcps[i], p, count);
                            }
                            ikcp_update(kcps[i], (uint)Environment.TickCount);
                            int len;
                            while ((len = ikcp_peeksize(kcps[i])) > 0)
                            {
                                var buffer2 = new byte[len];
                                fixed (byte* p1 = &buffer1[0])
                                {
                                    int kcnt = ikcp_recv(kcps[i], p1, buffer2.Length);
                                }
                            }
                        }
                    }
                }
                for (int i = 0; i < kcps.Length; i++)
                    ikcp_release(kcps[i]);
            }, cts.Token);
            return cts;
        }
    }
}
