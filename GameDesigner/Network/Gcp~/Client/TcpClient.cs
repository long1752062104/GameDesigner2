namespace Net.Client
{
    using global::System;
    using global::System.IO;
    using global::System.Net;
    using global::System.Net.Sockets;
    using Net.Share;
    using Net.System;
    using Net.Event;
    using Net.Helper;
    using Cysharp.Threading.Tasks;

    /// <summary>
    /// TCP客户端类型 
    /// 第三版本 2020.9.14
    /// </summary>
    [Serializable]
    public class TcpClient : ClientBase
    {
        public override int HeartInterval { get; set; } = 1000 * 60 * 10;//10分钟跳一次
        public override byte HeartLimit { get; set; } = 2;//确认两次

        /// <summary>
        /// 构造可靠传输客户端
        /// </summary>
        public TcpClient()
        {
        }

        /// <summary>
        /// 构造不可靠传输客户端
        /// </summary>
        /// <param name="useUnityThread">使用unity多线程?</param>
        public TcpClient(bool useUnityThread) : this()
        {
            UseUnityThread = useUnityThread;
        }

        ~TcpClient()
        {
#if !UNITY_EDITOR
            Close();
#endif
        }

        /// <inheritdoc/>
        protected async override UniTask<bool> ConnectResult(string host, int port, int localPort, Action<bool> result)
        {
            await UniTask.SwitchToThreadPool();
            try
            {
                Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    SendBufferSize = SendBufferSize,
                    ReceiveBufferSize = ReceiveBufferSize,
                    NoDelay = true
                };
                this.localPort = localPort;
                if (localPort != -1)
                    Client.Bind(new IPEndPoint(IPAddress.Any, localPort));
                Client.Connect(host, port);
                var segment = BufferPool.Take(SendBufferSize);
                segment.Write(PreUserId);
                Client.Send(segment.ToArray(true));
                await UniTaskNetExtensions.Wait(8000, (state) =>
                {
                    NetworkTick();
                    return UID != 0;
                }, null);
                if (UID == 0 && openClient)
                    throw new Exception("连接握手失败!");
                if (UID == 0 && !openClient)
                    throw new Exception("客户端调用Close!");
                StackStream = new MemoryStream(Config.Config.BaseCapacity);
                StartupThread();
                await UniTask.Yield(); //切换到线程池中, 不要由事件线程去往下执行, 如果有耗时就会卡死事件线程
                result(true);
                return true;
            }
            catch (Exception ex)
            {
                NDebug.LogError("连接错误:" + ex);
                Connected = false;
                Client?.Close();
                Client = null;
                result(false);
                return false;
            }
        }

        public override void OnNetworkTick()
        {
            if (!Client.Connected)
                throw new SocketException((int)SocketError.Disconnecting);
        }

        protected override void PackData(ISegment stream)
        {
            stream.Flush(false);
            SetDataHead(stream);
            PackageAdapter.Pack(stream);
            var len = stream.Count - frame;
            var lenBytes = BitConverter.GetBytes(len);
            var crc = CRCHelper.CRC8(lenBytes, 0, lenBytes.Length);
            stream.Position = 0;
            stream.Write(lenBytes, 0, 4);
            stream.WriteByte(crc);
            stream.Position += len;
        }

        protected override void SendByteData(ISegment buffer)
        {
            sendCount += buffer.Count;
            sendAmount++;
            if (Client.Poll(0, SelectMode.SelectWrite))
            {
                int count = Client.Send(buffer.Buffer, buffer.Offset, buffer.Count, SocketFlags.None);
                if (count <= 0)
                    OnSendErrorHandle?.Invoke(buffer);
                else if (count != buffer.Count)
                    NDebug.LogError($"发送了{buffer.Count - count}个字节失败!");
            }
            else
            {
                NDebug.LogError("发送窗口已满,等待对方接收中!");
            }
        }

        protected override void ResolveBuffer(ref ISegment buffer, bool isTcp)
        {
            heart = 0;
            if (stack > 0)
            {
                stack++;
                StackStream.Seek(stackIndex, SeekOrigin.Begin);
                int size = buffer.Count - buffer.Position;
                stackIndex += size;
                StackStream.Write(buffer.Buffer, buffer.Position, size);
                if (stackIndex < stackCount)
                {
                    InvokeRevdRTProgress(stackIndex, stackCount);
                    return;
                }
                var count = (int)StackStream.Position;//.Length; //错误问题,不能用length, 这是文件总长度, 之前可能已经有很大一波数据
                BufferPool.Push(buffer);//要回收掉, 否则会提示内存泄露
                buffer = BufferPool.Take(count);//ref 才不会导致提示内存泄露
                StackStream.Seek(0, SeekOrigin.Begin);
                StackStream.Read(buffer.Buffer, 0, count);
                buffer.Count = count;
            }
            while (buffer.Position < buffer.Count)
            {
                if (buffer.Position + frame > buffer.Count)//流数据偶尔小于frame头部字节
                {
                    var position = buffer.Position;
                    var count = buffer.Count - position;
                    stackIndex = count;
                    stackCount = 0;
                    StackStream.Seek(0, SeekOrigin.Begin);
                    StackStream.Write(buffer.Buffer, position, count);
                    stack++;
                    break;
                }
                var lenBytes = buffer.Read(4);
                var crcCode = buffer.ReadByte();//CRC检验索引
                var retVal = CRCHelper.CRC8(lenBytes, 0, 4);
                if (crcCode != retVal)
                {
                    stack = 0;
                    NDebug.LogError($"[{UID}]CRC校验失败!");
                    return;
                }
                var size = BitConverter.ToInt32(lenBytes, 0);
                if (size < 0 | size > PackageSize)//如果出现解析的数据包大小有问题，则不处理
                {
                    stack = 0;
                    NDebug.LogError($"[{UID}]数据被拦截修改或数据量太大: size:{size}，如果想传输大数据，请设置PackageSize属性");
                    return;
                }
                if (buffer.Position + size <= buffer.Count)
                {
                    stack = 0;
                    var count = buffer.Count;//此长度可能会有连续的数据(粘包)
                    buffer.Count = buffer.Position + size;//需要指定一个完整的数据长度给内部解析
                    base.ResolveBuffer(ref buffer, true);
                    buffer.Count = count;//解析完成后再赋值原来的总长
                }
                else
                {
                    var position = buffer.Position - frame;
                    var count = buffer.Count - position;
                    stackIndex = count;
                    stackCount = size;
                    StackStream.Seek(0, SeekOrigin.Begin);
                    StackStream.Write(buffer.Buffer, position, count);
                    stack++;
                    break;
                }
            }
        }

        /// <inheritdoc/>
        public override void Close(bool await = true, int millisecondsTimeout = 100)
        {
            base.Close(await, millisecondsTimeout);
        }
    }
}