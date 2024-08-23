namespace Net.Client
{
    using global::System;
    using global::System.IO;
    using global::System.Net.Sockets;
    using global::System.Runtime.InteropServices;
    using global::System.Threading;
    using global::System.Threading.Tasks;
    using global::System.Net;
    using global::System.Reflection;
    using global::System.Collections.Generic;
    using Kcp;
    using AOT;
    using Net.Share;
    using Net.System;
    using Net.Event;
    using Cysharp.Threading.Tasks;
    using static Kcp.KcpLib;

    /// <summary>
    /// kcp客户端
    /// </summary>
    [Serializable]
    public unsafe class KcpClient : ClientBase
    {
        private IntPtr kcp;
        private IntPtr user;
        private outputCallback output;

        public KcpClient() : base()
        {
        }

        public KcpClient(bool useUnityThread) : this()
        {
            UseUnityThread = useUnityThread;
        }

        ~KcpClient()
        {
            ReleaseKcp();
        }

        protected override UniTask<bool> ConnectResult(string host, int port, int localPort, Action<bool> result)
        {
            ReleaseKcp();
            user = Marshal.GetIUnknownForObject(this);
            kcp = ikcp_create(MTU, user);
            output = new outputCallback(Output);
            var outputPtr = Marshal.GetFunctionPointerForDelegate(output);
            ikcp_setoutput(kcp, outputPtr);
            ikcp_wndsize(kcp, SendBufferSize, ReceiveBufferSize);
            ikcp_nodelay(kcp, 1, 10, 2, 1);
            return base.ConnectResult(host, port, localPort, result);
        }

        private byte[] addressBuffer;
        internal byte[] RemoteAddressBuffer()
        {
            if (addressBuffer != null)
                return addressBuffer;
            var socketAddress = Client.RemoteEndPoint.Serialize();
            addressBuffer = (byte[])socketAddress.GetType().GetField("m_Buffer", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(socketAddress);
            return addressBuffer;
        }

        //IL2CPP使用Marshal.GetFunctionPointerForDelegate需要设置委托方法为静态方法，并且要添加上特性MonoPInvokeCallback
        [MonoPInvokeCallback(typeof(outputCallback))]
        public static unsafe int Output(IntPtr buf, int len, IntPtr kcp, IntPtr user)
        {
            var client = Marshal.GetObjectForIUnknown(user) as KcpClient;
            client.sendCount += len;
            client.sendAmount++;
#if WINDOWS
            return Win32KernelAPI.sendto(client.Client.Handle, (byte*)buf, len, SocketFlags.None, client.RemoteAddressBuffer(), 16);
#else
            var buff = new byte[len];
            Marshal.Copy(buf, buff, 0, len);
            return client.Client.Send(buff, 0, len, SocketFlags.None);
#endif
        }

        public override void ReceiveHandler()
        {
            if (Client.Poll(performance, SelectMode.SelectRead))
            {
                var segment = BufferPool.Take(ReceiveBufferSize);
                segment.Count = Client.Receive(segment.Buffer, 0, segment.Length, SocketFlags.None, out SocketError error);
                if (error != SocketError.Success | segment.Count == 0)
                {
                    BufferPool.Push(segment);
                    return;
                }
                receiveCount += segment.Count;
                receiveAmount++;
                heart = 0;
                fixed (byte* p = &segment.Buffer[0])
                    ikcp_input(kcp, p, segment.Count);
                BufferPool.Push(segment);
            }
            int len;
            if ((len = ikcp_peeksize(kcp)) > 0)
            {
                var segment1 = BufferPool.Take(len);
                fixed (byte* p1 = &segment1.Buffer[0])
                {
                    segment1.Count = ikcp_recv(kcp, p1, len);
                    ResolveBuffer(ref segment1);
                    BufferPool.Push(segment1);
                }
            }
        }

        public override void OnNetworkTick()
        {
            ikcp_update(kcp, (uint)Environment.TickCount);
        }

        protected override void SendByteData(ISegment buffer)
        {
            if (buffer.Count <= Frame)//解决长度==5的问题(没有数据)
                return;
            fixed (byte* p = &buffer.Buffer[0])
            {
                int count = ikcp_send(kcp, p, buffer.Count);
                if (count < 0)
                    OnSendErrorHandle?.Invoke(buffer);
            }
        }

        public override void Close(bool await = true, int millisecondsTimeout = 100)
        {
            base.Close(await, millisecondsTimeout);
            addressBuffer = null;
            ReleaseKcp();
        }

        public override void Disconnect(bool reuseSocket, bool invokeSocketDisconnect = true)
        {
            base.Disconnect(reuseSocket, false);
            ikcp_update(kcp, (uint)Environment.TickCount + 1000);
        }

        private void ReleaseKcp()
        {
            if (kcp != IntPtr.Zero)
            {
                ikcp_release(kcp);
                kcp = IntPtr.Zero;
            }
            if (user != IntPtr.Zero)
            {
                Marshal.Release(user);
                user = IntPtr.Zero;
            }
        }
    }
}
