﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kcp;
using System.Runtime.InteropServices;
using Net.Share;
using System.Net.Sockets;

namespace Net.Server
{
    /// <summary>
    /// kcp客户端对象
    /// </summary>
    public unsafe class KcpPlayer : NetPlayer
    {
        /// <summary>
        /// kcp对象
        /// </summary>
        public IntPtr Kcp { get; set; }
        internal outputCallback output;
        internal Socket Server;

        public KcpPlayer() 
        {
            output = new outputCallback(Output);
        }

        public unsafe int Output(byte* buf, int len, IntPtr kcp, IntPtr user)
        {
            byte[] buff = new byte[len];
            Marshal.Copy(new IntPtr(buf), buff, 0, len);
            //sendQueue.Enqueue(new SendDataBuffer(this, buff));
            Server.SendTo(buff, 0, buff.Length, SocketFlags.None, RemotePoint);
            return 0;
        }

        ~KcpPlayer()
        {
            if (Kcp == IntPtr.Zero)
                return;
            KcpLib.ikcp_release(Kcp);
            Kcp = IntPtr.Zero;
            output = null;
        }
    }
}
