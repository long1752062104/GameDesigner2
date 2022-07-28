using Net.System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Net.Plugins
{
    class Cmd
    {
        /// <summary>
        /// 网络帧
        /// </summary>
        internal const byte Frame = 2;
        /// <summary>
        /// 网络帧确认
        /// </summary>
        internal const byte Ack = 4;
    }

    class RTODataFrame
    {
        internal byte[] buffer;
        internal uint tick;
        public RTODataFrame()
        {
        }
        public RTODataFrame(byte[] buffer)
        {
            this.buffer = buffer;
        }
    }

    /// <summary>
    /// Gcp可靠协议核心类
    /// </summary>
    public class GcpKernel : IDisposable
    {
        private uint senderFrameLocal;
        private uint revderFrameLocal;
        private uint senderFrame;
        private uint revderFrame;
        private byte[] revderBuffer;
        private int revderFrameEnd;
        private byte[] revderHash;
        private readonly Queue<byte[]> senderQueue = new Queue<byte[]>();
        private readonly Queue<byte[]> revderQueue = new Queue<byte[]>();
        public ushort MTU { get; set; } = 1300;
        public event Action<byte[]> OnSender;
        private readonly MyDictionary<uint, RTODataFrame> senderDict = new MyDictionary<uint, RTODataFrame>();
        private byte[] current;
        public EndPoint RemotePoint { get; set; }
        public int WinSize { get; set; } = ushort.MaxValue;
        public uint RTO = 250;
        private readonly object SyncRoot = new object();
        private uint tick;
        private uint flowTick;
        private int currFlow;
        private int revderHashCount;
        public int MTPS { get; set; } = 1024 * 1024 * 2;

        public void Update()
        {
            lock (SyncRoot)
            {
                CheckNextSend();
                tick += 17;
                if (tick >= flowTick)
                {
                    flowTick = tick + 1000;
                    currFlow = 0;
                }
                foreach (var sender in senderDict.Values)
                {
                    if (tick >= sender.tick & currFlow < MTPS)
                    {
                        sender.tick = tick + RTO;
                        OnSender(sender.buffer);
                        currFlow += sender.buffer.Length;
                    }
                }
            }
        }
        public int Receive(out byte[] buffer)
        {
            lock (SyncRoot)
            {
                if (revderQueue.Count <= 0)
                {
                    buffer = null;
                    return 0;
                }
                buffer = revderQueue.Dequeue();
                return buffer.Length;
            }
        }
        public void Send(byte[] buffer)
        {
            lock (SyncRoot)
            {
                senderQueue.Enqueue(buffer);
            }
        }
        private void CheckNextSend()
        {
            if (senderQueue.Count > 0 & current == null)
            {
                current = senderQueue.Dequeue();
                var count = current.Length;
                var frameEnd = (int)Math.Ceiling(count / (float)MTU);
                using (var segment = BufferPool.Take())
                {
                    for (int i = 0; i < frameEnd; i++)
                    {
                        segment.SetPositionLength(0);
                        segment.WriteByte(Cmd.Frame);
                        segment.Write(senderFrame);
                        segment.Write(count);
                        var offset = i * MTU;
                        segment.Write(current, offset, offset + MTU >= count ? count - offset : MTU);
                        senderDict.TryAdd(senderFrame, new RTODataFrame(segment.ToArray()));
                        senderFrame++;
                    }
                }
                current = new byte[0];
            }
        }
        public void Input(byte[] buffer)
        {
            lock (SyncRoot)
            {
                var segment = new Segment(buffer, false);
                var flags = segment.ReadByte();
                if (flags == Cmd.Frame)
                {
                    var frame = segment.ReadUInt32();
                    var dataLen = segment.ReadUInt32();
                    if (frame < revderFrameLocal)//如果数据已经结束完成，但客户端rto还没有被确定，再次发送了之前的数据，就回ack确认即可，什么都不要做
                        goto J;
                    var frame1 = frame - revderFrameLocal;
                    if (revderBuffer == null)
                    {
                        revderBuffer = new byte[dataLen];
                        revderFrameEnd = (int)Math.Ceiling(dataLen / (float)MTU);
                        revderHash = new byte[revderFrameEnd];
                    }
                    if (revderHash[frame1] == 0)
                    {
                        revderHashCount++;
                        revderHash[frame1] = 1;
                        Buffer.BlockCopy(segment, segment.Position, revderBuffer, (int)(frame1 * MTU), segment.Count - segment.Position);
                        if (revderHashCount >= revderFrameEnd)
                        {
                            revderFrame += (uint)revderFrameEnd;
                            revderFrameLocal = revderFrame;
                            revderQueue.Enqueue(revderBuffer);
                            revderBuffer = null;
                            revderHash = null;
                            revderHashCount = 0;
                        }
                    }
                J: segment = BufferPool.Take();
                    segment.WriteByte(Cmd.Ack);
                    segment.Write(frame);
                    var bytes = segment.ToArray(true);
                    OnSender(bytes);
                }
                else if (flags == Cmd.Ack)
                {
                    if (current == null)
                        return;
                    var frame = segment.ReadUInt32();
                    if (senderDict.Remove(frame))
                    {
                        if (senderDict.Count <= 0)
                        {
                            current = null;
                            senderFrameLocal = senderFrame;
                            CheckNextSend();
                        }
                    }
                }
            }
        }
        public bool HasSend()
        {
            return current != null;
        }
        public void Dispose()
        {
            lock (SyncRoot)
            {
                revderBuffer = null;
                senderQueue.Clear();
                revderQueue.Clear();
                senderDict.Clear();
                current = null;
            }
        }
        public override string ToString()
        {
            return $"{RemotePoint}";
        }
    }
}