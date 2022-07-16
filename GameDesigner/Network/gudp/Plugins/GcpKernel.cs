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
        internal const byte Ack = 2;
        internal const byte Frame = 3;
        internal const byte Finish = 6;
        internal const byte FrameFinish = 7;
    }

    class RTODataFrame
    {
        internal uint tick;
        internal byte[] buffer;
    }

    public class GcpKernel : IDisposable
    {
        private int senderFrame = 0;
        private int revderFrame = 0;
        private readonly MemoryStream stream = new MemoryStream();
        private readonly Queue<byte[]> senderQueue = new Queue<byte[]>();
        private readonly Queue<byte[]> revderQueue = new Queue<byte[]>();
        public ushort MTU { get; set; } = 1300;

        public event Action<byte[]> OnSender;
        private readonly MyDictionary<int, RTODataFrame> senderDict = new MyDictionary<int, RTODataFrame>();
        private byte[] current;
        public EndPoint RemotePoint { get; set; }
        public int WinSize { get; set; } = ushort.MaxValue;
        public uint RTO = 250;
        private readonly object SyncRoot = new object();
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
        public void Input(byte[] buffer)
        {
            lock (SyncRoot)
            {
                var segment = new Segment(buffer, false);
                var flags = segment.ReadByte();
                if (flags == Cmd.Ack)
                {
                    var frame = segment.ReadInt32();
                    var index = frame * MTU;
                    segment = BufferPool.Take();
                    segment.WriteByte(index + MTU >= current.Length ? Cmd.FrameFinish : Cmd.Frame);
                    segment.Write(frame);
                    segment.Write(current, index, index + MTU >= current.Length ? current.Length - index : MTU);
                    var bytes = segment.ToArray(true);
                    senderDict.Remove(frame);
                    senderDict.TryAdd(frame + 1, new RTODataFrame() { buffer = bytes });
                }
                else if (flags == Cmd.Finish)
                {
                    var frame = segment.ReadInt32();
                    if (senderDict.Remove(frame))
                    {
                        senderQueue.Dequeue();
                        current = null;
                        CheckNextSend();
                    }
                    else
                    {
                        Console.WriteLine(frame);
                    }
                }
                else if (flags == Cmd.Frame | flags == Cmd.FrameFinish)
                {
                    var frame = segment.ReadInt32();
                    if (revderFrame == frame)
                    {
                        revderFrame++;
                        stream.Seek(frame * MTU, SeekOrigin.Begin);
                        stream.Write(segment, segment.Position, segment.Count - segment.Position);
                    }
                    else
                    {
                        Console.WriteLine($"{revderFrame}-{frame}");
                    }
                    var cmd = Cmd.Ack;
                    frame = revderFrame;
                    if (flags == Cmd.FrameFinish)
                    {
                        revderFrame = 0;
                        cmd = Cmd.Finish;
                        revderQueue.Enqueue(stream.ToArray());
                        stream.SetLength(0);
                    }
                    segment = BufferPool.Take();
                    segment.WriteByte(cmd);
                    segment.Write(frame);
                    var bytes = segment.ToArray(true);
                    OnSender(bytes);
                }
            }
        }
        private void CheckNextSend()
        {
            if (senderQueue.Count > 0 & current == null)
            {
                senderFrame = 0;
                current = senderQueue.Peek();
                var segment = BufferPool.Take();
                segment.WriteByte(MTU >= current.Length ? Cmd.FrameFinish : Cmd.Frame);
                segment.Write(senderFrame++);
                segment.Write(current, 0, MTU >= current.Length ? current.Length : MTU);
                var bytes = segment.ToArray(true);
                senderDict.TryAdd(senderFrame, new RTODataFrame() { buffer = bytes });
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
