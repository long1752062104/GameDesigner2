using Net.Common;
using Net.Share;
using Net.System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
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
        /// 冗余帧
        /// </summary>
        internal const byte DAck = 6;
    }

    class DataFrame
    {
        private int hashCode;
        internal ISegment buffer;
        internal int tick;
        public DataFrame()
        {
        }
        public DataFrame(int hashCode, ISegment buffer)
        {
            this.hashCode = hashCode;
            this.buffer = buffer;
        }
        public override int GetHashCode()
        {
            return hashCode;
        }
    }

    class DataPackage
    {
        internal ISegment revderBuffer;
        internal int revderFrameEnd;
        internal byte[] revderHash;
        internal int revderHashCount;
        internal bool finish;
    }

    /// <summary>
    /// Gcp可靠协议核心类
    /// </summary>
    public class GcpKernel : IGcp, IDisposable
    {
        private uint pushPackage;
        private uint senderPackage;
        private uint revderPackage;
        private readonly Queue<ISegment> revderQueue = new Queue<ISegment>();
        public Action<EndPoint, ISegment> OnSender { get; set; }
        public Action<RTProgress> OnSendProgress { get; set; }
        public Action<RTProgress> OnRevdProgress { get; set; }
        private readonly MyDictionary<uint, MyDictionary<int, DataFrame>> senderDict = new MyDictionary<uint, MyDictionary<int, DataFrame>>();
        private readonly MyDictionary<uint, DataPackage> revderDict = new MyDictionary<uint, DataPackage>();
        public EndPoint RemotePoint { get; set; }
        private int tick;
        private int flowTick;
        private int currFlow;
        private int progressTick;
        public ushort MTU { get; set; } = 1300;
        public int RTO { get; set; } = 1000;
        private int ackNumber = 1;
        private int currAck;
        public int MTPS { get; set; } = 1024 * 1024;
        public int ProgressDataLen { get; set; } = ushort.MaxValue;//要求数据大于多少才会调用发送，接收进度值
        public FlowControlMode FlowControl { get; set; } = FlowControlMode.Normal;
        private readonly FastLocking Locking = new FastLocking();

        public void Update()
        {
            try
            {
                Locking.Lock();
                tick = Environment.TickCount;
                if (tick >= flowTick)
                {
                    flowTick = tick + 1000;
                    currFlow = 0;
                    currAck = 0;
                    ackNumber = FlowControl == FlowControlMode.Normal ? 1 : 100;
                }
                if (senderDict.Count <= 0)
                    return;
                var length = senderPackage + senderDict.Count;
                for (uint i = senderPackage; i < length; i++)
                {
                    if (!FastRetransmit(i))
                        if (i == senderPackage)
                            senderPackage++;
                    if (currFlow >= MTPS | currAck >= ackNumber)
                        break;
                }
            }
            finally
            {
                Locking.Release();
            }
        }
        public int Receive(out ISegment segment)
        {
            try
            {
                Locking.Lock();
                if (revderQueue.Count <= 0)
                {
                    segment = null;
                    return 0;
                }
                segment = revderQueue.Dequeue();
                return segment.Count;
            }
            finally
            {
                Locking.Release();
            }
        }
        public void Send(ISegment buffer)
        {
            try
            {
                Locking.Lock();
                var count = buffer.Count;
                var frameEnd = (int)Math.Ceiling(count / (float)MTU);
                var dic = new MyDictionary<int, DataFrame>(frameEnd);
                senderDict.Add(pushPackage, dic);
                for (int serialNo = 0; serialNo < frameEnd; serialNo++)
                {
                    var offset = serialNo * MTU;
                    var dataCount = offset + MTU >= count ? count - offset : MTU;
                    var segment = BufferPool.Take(dataCount + 13);
                    segment.SetPositionLength(0);
                    segment.WriteByte(Cmd.Frame);
                    segment.Write(pushPackage);
                    segment.Write(serialNo);
                    segment.Write(count);
                    segment.Write(buffer.Buffer, offset, dataCount);
                    segment.Flush();
                    var dataFrame = new DataFrame(serialNo, segment);
                    dic.Add(serialNo, dataFrame);
                }
                pushPackage++;
            }
            finally
            {
                Locking.Release();
            }
        }
        public void Input(ISegment stream)
        {
            try
            {
                Locking.Lock();
                var flags = stream.ReadByte();
                if (flags == Cmd.Frame)
                {
                    var package = stream.ReadUInt32();
                    var serialNo = stream.ReadInt32();
                    var dataLen = stream.ReadInt32();
                    if (package < revderPackage)
                        goto J;
                    if (!revderDict.TryGetValue(package, out var dp))
                        revderDict.Add(package, dp = new DataPackage());
                    if (dp.revderBuffer == null)
                    {
                        dp.revderBuffer = BufferPool.Take(dataLen);
                        dp.revderBuffer.Count = dataLen;
                        dp.revderFrameEnd = (int)Math.Ceiling(dataLen / (float)MTU);
                        dp.revderHash = new byte[dp.revderFrameEnd];
                    }
                    if (dp.revderHash[serialNo] == 0)
                    {
                        dp.revderHashCount++;
                        dp.revderHash[serialNo] = 1;
                        Unsafe.CopyBlockUnaligned(ref dp.revderBuffer.Buffer[serialNo * MTU], ref stream.Buffer[stream.Position], (uint)(stream.Count - stream.Position));
                        if (dp.revderHashCount >= dp.revderFrameEnd)
                            dp.finish = true;
                    }
                    while (revderDict.TryGetValue(revderPackage, out var dp1))
                    {
                        if (tick >= progressTick)
                        {
                            progressTick = tick + 1000;
                            if (dp.revderFrameEnd * MTU >= ProgressDataLen)
                            {
                                var progress = (dp.revderHashCount / (float)dp.revderFrameEnd) * 100f;
                                OnRevdProgress?.Invoke(new RTProgress(progress, (RTState)(progress >= 100f ? 3 : 1)));
                            }
                        }
                        if (!dp1.finish)
                            break;
                        revderQueue.Enqueue(dp1.revderBuffer);
                        revderDict.Remove(revderPackage);
                        revderPackage++;
                        dp1.revderBuffer = null;
                        dp1.revderHash = null;
                        dp1.revderHashCount = 0;
                    }
                J: var segment = BufferPool.Take(13); //要重新取出，不能用stream，会导致外部出现问题
                    segment.WriteByte(Cmd.Ack);
                    segment.Write(package);
                    segment.Write(serialNo);
                    segment.Write(dataLen);
                    segment.Flush();
                    OnSender(RemotePoint, segment);
                }
                else if (flags == Cmd.Ack)
                {
                    var package = stream.ReadUInt32();
                    var serialNo = stream.ReadInt32();
                    var dataLen = stream.ReadInt32();
                    if (senderDict.TryGetValue(package, out var dic))
                    {
                        if (dic.TryRemove(serialNo, out var dataFrame))
                        {
                            dataFrame.buffer.Dispose();
                            ackNumber++;
                        }
                        if (dic.Count <= 0)
                            senderDict.Remove(package);
                        if (tick >= progressTick)
                        {
                            progressTick = tick + 1000;
                            if (dataLen >= ProgressDataLen)
                            {
                                var progress = ((dic.Count * MTU) / (float)dataLen) * 100f;
                                OnSendProgress?.Invoke(new RTProgress(progress, (RTState)(progress >= 100f ? 3 : 1)));
                            }
                        }
                    }
                    FastRetransmit(senderPackage);
                }
                else if (flags == Cmd.DAck)
                {
                    var package = stream.ReadUInt32();
                    FastRetransmit(package);
                }
            }
            finally
            {
                Locking.Release();
            }
        }
        private bool FastRetransmit(uint package)
        {
            if (senderDict.TryGetValue(package, out var dict))
            {
                foreach (var sender in dict.Values)
                {
                    if (tick >= sender.tick & currFlow < MTPS & currAck < ackNumber)
                    {
                        sender.tick = tick + RTO;
                        var count = sender.buffer.Length;
                        currFlow += count;
                        currAck++;
                        OnSender(RemotePoint, sender.buffer);
                    }
                }
                return true;
            }
            return false;
        }
        public bool HasSend()
        {
            return false;
        }
        public void Dispose()
        {
            try
            {
                Locking.Lock();
                pushPackage = 0;
                senderPackage = 0;
                revderPackage = 0;
                revderQueue.Clear();
                senderDict.Clear();
                revderDict.Clear();
                RemotePoint = null;
            }
            finally
            {
                Locking.Release();
            }
        }
        public override string ToString()
        {
            return $"{RemotePoint}";
        }
    }
}