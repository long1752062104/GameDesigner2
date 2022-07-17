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
        /// <summary>
        /// 一段完整数据发送完成
        /// </summary>
        internal const byte FrameFinish = 6;
        /// <summary>
        /// 一段完整数据帧确认(结束发送方的重传计数)
        /// </summary>
        internal const byte Finish = 8;
        /// <summary>
        /// 一段完整数据帧确认(结束接收方的重传计数)
        /// </summary>
        internal const byte FinishAck = 10;
    }

    class RTODataFrame
    {
        internal uint tick;
        internal byte[] buffer;
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
        private readonly MemoryStream stream = new MemoryStream();
        private readonly Queue<byte[]> senderQueue = new Queue<byte[]>();
        private readonly Queue<byte[]> revderQueue = new Queue<byte[]>();
        public ushort MTU { get; set; } = 1300;
        public event Action<byte[]> OnSender;
        private readonly MyDictionary<uint, RTODataFrame> senderDict = new MyDictionary<uint, RTODataFrame>();
        private readonly MyDictionary<uint, RTODataFrame> revderDict = new MyDictionary<uint, RTODataFrame>();
        private byte[] current;
        public EndPoint RemotePoint { get; set; }
        public int WinSize { get; set; } = ushort.MaxValue;
        public uint RTO = 250;
        private readonly static object SyncRoot = new object();
        private uint tick;
        public void Update()
        {
            lock (SyncRoot) 
            {
                CheckNextSend();
                tick += 17;
                foreach (var sender in senderDict.Values)
                {
                    if (tick >= sender.tick)
                    {
                        sender.tick = tick + RTO;
                        OnSender(sender.buffer);
                    }
                }
                foreach (var revder in revderDict.Values)
                {
                    if (tick >= revder.tick)
                    {
                        revder.tick = tick + RTO;
                        OnSender(revder.buffer);
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
                current = senderQueue.Peek();
                var segment = BufferPool.Take();
                segment.WriteByte(MTU >= current.Length ? Cmd.FrameFinish : Cmd.Frame);
                segment.Write(senderFrame);
                segment.Write(current, 0, MTU >= current.Length ? current.Length : MTU);
                var bytes = segment.ToArray(true);
                senderFrame++;
                senderDict.TryAdd(senderFrame, new RTODataFrame() { buffer = bytes });
            }
        }
        public void Input(byte[] buffer)
        {
            lock (SyncRoot)
            {
                var segment = new Segment(buffer, false);
                var flags = segment.ReadByte();
                if (flags == Cmd.Ack)
                {
                    if (current == null)
                        return;
                    var frame = segment.ReadUInt32();
                    if (frame < senderFrame)
                        return;
                    var frame1 = frame - senderFrameLocal;
                    if (frame1 < 0)
                        return;
                    int index = (int)(frame1 * MTU);
                    if (index >= current.Length)
                        return;
                    segment = BufferPool.Take();
                    segment.WriteByte(index + MTU >= current.Length ? Cmd.FrameFinish : Cmd.Frame);
                    segment.Write(frame);
                    segment.Write(current, index, index + MTU >= current.Length ? current.Length - index : MTU);
                    var bytes = segment.ToArray(true);
                    if (senderDict.Remove(frame))
                    {
                        senderFrame++;
                        frame = senderFrame;
                    }
                    senderDict.TryAdd(frame, new RTODataFrame() { buffer = bytes });
                }
                else if (flags == Cmd.Frame | flags == Cmd.FrameFinish)
                {
                    var frame = segment.ReadUInt32();
                    var cmd = Cmd.Ack;
                    if (revderFrame == frame)
                    {
                        revderFrame++;
                        var frame1 = frame - revderFrameLocal;
                        frame = revderFrame;
                        stream.Seek(frame1 * MTU, SeekOrigin.Begin);
                        stream.Write(segment, segment.Position, segment.Count - segment.Position);
                        if (flags == Cmd.FrameFinish)
                        {
                            cmd = Cmd.Finish;
                            revderFrameLocal = revderFrame;
                            revderQueue.Enqueue(stream.ToArray());
                            stream.SetLength(0);
                        }
                    }
                    segment = BufferPool.Take();
                    segment.WriteByte(cmd);
                    segment.Write(frame);
                    var bytes = segment.ToArray(true);
                    revderDict.Remove(frame);
                    revderDict.TryAdd(frame, new RTODataFrame() { buffer = bytes });
                }
                else if (flags == Cmd.Finish)
                {
                    var frame = segment.ReadUInt32();
                    if (senderDict.Remove(frame))
                    {
                        senderFrame = frame;
                        senderQueue.Dequeue();
                        current = null;
                        senderFrameLocal = senderFrame;
                        CheckNextSend();
                    }
                    segment = BufferPool.Take();
                    segment.WriteByte(Cmd.FinishAck);
                    segment.Write(frame);
                    var bytes = segment.ToArray(true);
                    OnSender(bytes);
                }
                else if (flags == Cmd.FinishAck)
                {
                    var frame = segment.ReadUInt32();
                    revderDict.Remove(frame);
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
                stream.Close();
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
