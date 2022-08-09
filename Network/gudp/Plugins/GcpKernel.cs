using Net.Share;
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

    class DataFrame
    {
        internal byte[] buffer;
        internal int tick;
        public DataFrame()
        {
        }
        public DataFrame(byte[] buffer)
        {
            this.buffer = buffer;
        }
    }

    class DataPackage 
    {
        internal byte[] revderBuffer;
        internal int revderFrameEnd;
        internal byte[] revderHash;
        internal int revderHashCount;
        internal bool finish;
    }

    /// <summary>
    /// Gcp可靠协议核心类
    /// </summary>
    public class GcpKernel : IDisposable
    {
        private uint package;
        private uint packageLocal;
        private readonly Queue<byte[]> senderQueue = new Queue<byte[]>();
        private readonly Queue<byte[]> revderQueue = new Queue<byte[]>();
        public Action<byte[]> OnSender;
        public Action<RTProgress> OnSendProgress;
        public Action<RTProgress> OnRevdProgress;
        private readonly MyDictionary<uint, MyDictionary<int, DataFrame>> senderDict = new MyDictionary<uint, MyDictionary<int, DataFrame>>();
        private readonly MyDictionary<uint, DataPackage> revderPackage = new MyDictionary<uint, DataPackage>();
        public EndPoint RemotePoint { get; set; }
        
        private readonly object SyncRoot = new object();
        private int tick;
        private int flowTick;
        private int currFlow;
        private int progressTick;
        public ushort MTU { get; set; } = 1300;
        public int RTO = 1000;
        public int MTPS { get; set; } = 1024 * 1024;
        public int ProgressDataLen { get; set; } = ushort.MaxValue;//要求数据大于多少才会调用发送，接收进度值

        public void Update()
        {
            lock (SyncRoot)
            {
                CheckNextSend();
                tick = Environment.TickCount;
                if (tick >= flowTick)
                {
                    flowTick = tick + 1000;
                    currFlow = 0;
                }
                foreach (var dic in senderDict.Values)
                {
                    foreach (var sender in dic.Values)
                    {
                        if (tick >= sender.tick & currFlow < MTPS)
                        {
                            sender.tick = tick + RTO;
                            currFlow += sender.buffer.Length;
                            OnSender(sender.buffer);
                        }
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
            if (senderQueue.Count > 0)
            {
                var current = senderQueue.Dequeue();
                var count = current.Length;
                var frameEnd = (int)Math.Ceiling(count / (float)MTU);
                using (var segment = BufferPool.Take())
                {
                    var dic = new MyDictionary<int, DataFrame>();
                    senderDict.Add(package, dic);
                    for (int serialNo = 0; serialNo < frameEnd; serialNo++)
                    {
                        segment.SetPositionLength(0);
                        segment.WriteByte(Cmd.Frame);
                        segment.Write(package);
                        segment.Write(serialNo);
                        segment.Write(count);
                        var offset = serialNo * MTU;
                        segment.Write(current, offset, offset + MTU >= count ? count - offset : MTU);
                        dic.Add(serialNo, new DataFrame(segment.ToArray()));
                    }
                    package++;
                }
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
                    var package = segment.ReadUInt32();
                    var serialNo = segment.ReadInt32();
                    var dataLen = segment.ReadInt32();
                    if (package < packageLocal)
                        goto J;
                    if (!revderPackage.TryGetValue(package, out var dp))
                        revderPackage.Add(package, dp = new DataPackage());
                    if (dp.revderBuffer == null)
                    {
                        dp.revderBuffer = new byte[dataLen];
                        dp.revderFrameEnd = (int)Math.Ceiling(dataLen / (float)MTU);
                        dp.revderHash = new byte[dp.revderFrameEnd];
                    }
                    if (dp.revderHash[serialNo] == 0)
                    {
                        dp.revderHashCount++;
                        dp.revderHash[serialNo] = 1;
                        Buffer.BlockCopy(segment, segment.Position, dp.revderBuffer, (int)(serialNo * MTU), segment.Count - segment.Position);
                        if (dp.revderHashCount >= dp.revderFrameEnd)
                            dp.finish = true;
                    }
                    while (revderPackage.TryGetValue(packageLocal, out var dp1))
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
                        revderPackage.Remove(packageLocal);
                        packageLocal++;
                        dp1.revderBuffer = null;
                        dp1.revderHash = null;
                        dp1.revderHashCount = 0;
                    }
                J: segment.SetPositionLength(0);
                    segment.WriteByte(Cmd.Ack);
                    segment.Write(package);
                    segment.Write(serialNo);
                    segment.Write(dataLen);
                    var bytes = segment.ToArray(true);
                    OnSender(bytes);
                }
                else if (flags == Cmd.Ack)
                {
                    var package = segment.ReadUInt32();
                    var serialNo = segment.ReadInt32();
                    var dataLen = segment.ReadInt32();
                    if (senderDict.TryGetValue(package, out var dic)) 
                    {
                        dic.Remove(serialNo);
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
                }
            }
        }
        public bool HasSend()
        {
            return false;
        }
        public void Dispose()
        {
            lock (SyncRoot)
            {
                package = 0;
                packageLocal = 0;
                senderQueue.Clear();
                revderQueue.Clear();
                senderDict.Clear();
                revderPackage.Clear();
                RemotePoint = null;
            }
        }
        public override string ToString()
        {
            return $"{RemotePoint}";
        }
    }
}