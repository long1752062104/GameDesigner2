namespace Net.Client
{
    using global::System;
    using global::System.Collections.Generic;
    using global::System.Runtime.InteropServices;
    using global::System.Threading;
    using global::System.Threading.Tasks;
#if SERVICE
    using global::System.IO;
    using global::System.Net.Sockets;
    using global::System.Collections.Concurrent;
#endif
    using Cysharp.Threading.Tasks;
    using Udx;
    using Net.System;
    using Net.Event;
    using Net.Share;
    using AOT;
    using Net.Helper;

    /// <summary>
    /// udx客户端类型 -> 只能300人以下连接, 如果想要300个客户端以上, 请进入udx网址:www.goodudx.com 联系作者下载专业版FastUdxApi.dll, 然后更换下框架内的FastUdxApi.dll即可
    /// 第三版本 2020.9.14
    /// </summary>
    public class UdxClient : ClientBase
    {
        protected IntPtr udxObj;
        protected IntPtr ClientPtr;
        protected UDXPRC udxPrc;

        /// <summary>
        /// 构造可靠传输客户端
        /// </summary>
        public UdxClient()
        {
        }

        /// <summary>
        /// 构造可靠传输客户端
        /// </summary>
        /// <param name="useUnityThread">使用unity多线程?</param>
        public UdxClient(bool useUnityThread) : this()
        {
            UseUnityThread = useUnityThread;
        }

        ~UdxClient()
        {
#if !UNITY_EDITOR
            Close();
#endif
        }

        public override UniTask<bool> Connect(string host, int port, int localPort, Action<bool> result)
        {
            if (!UdxLib.INIT)
            {
                UdxLib.INIT = true;
#if SERVICE && WINDOWS
                var path = AppDomain.CurrentDomain.BaseDirectory;
                if (!File.Exists(PathHelper.Combine(path, "udxapi.dll")))
                    throw new FileNotFoundException($"udxapi.dll没有在程序根目录中! 请从GameDesigner文件夹下找到 udxapi.dll复制到{path}目录下.");
#endif
                UdxLib.UInit(1);
                UdxLib.UEnableLog(false);
            }
            return base.Connect(host, port, localPort, result);
        }

        protected async override UniTask<bool> ConnectResult(string host, int port, int localPort, Action<bool> result)
        {
            await UniTask.SwitchToThreadPool();
            try
            {
                ReleaseUdx();
                udxObj = UdxLib.UCreateFUObj();
                UdxLib.UBind(udxObj, null, 0);
                udxPrc = new UDXPRC(ProcessReceive);
                UdxLib.USetFUCB(udxObj, udxPrc);
                GC.KeepAlive(udxPrc);
                if (host == "127.0.0.1" | host == "localhost")
                    host = NetPort.GetIP();
                ClientPtr = UdxLib.UConnect(udxObj, host, port, 0, false, 0);
                if (ClientPtr != IntPtr.Zero)
                {
                    UdxLib.UDump(ClientPtr);
                    var handle = GCHandle.Alloc(this);
                    var user = GCHandle.ToIntPtr(handle);
                    UdxLib.USetUserData(ClientPtr, user.ToInt64());
                }
                await UniTaskNetExtensions.Wait(5000, (state) => Connected, DBNull.Value);
                if (Connected)
                    StartupThread();
                else
                    throw new Exception("连接服务器失败!");
                var tick = DateTimeHelper.GetTickCount64();
                await UniTaskNetExtensions.Wait(8000, (state) =>
                {
                    if (DateTimeHelper.GetTickCount64() >= tick)
                    {
                        tick = DateTimeHelper.GetTickCount64() + 1000;
                        var segment = BufferPool.Take(SendBufferSize);
                        segment.Write(PreUserId);
                        RpcModels.Enqueue(new RPCModel(cmd: NetCmd.Identify, buffer: segment.ToArray(true)));
                    }
                    return UID != 0 | !openClient; //如果在爆满事件关闭客户端就需要判断一下
                }, DBNull.Value);
                if (UID == 0 && openClient)
                    throw new Exception("连接握手失败!");
                if (UID == 0 && !openClient)
                    throw new Exception("客户端调用Close!");
                await UniTask.Yield(); //切换到线程池中, 不要由事件线程去往下执行, 如果有耗时就会卡死事件线程, 在unity会切换到unity线程去执行，解决unity组件访问错误问题
                result(Connected);
                return Connected;
            }
            catch (Exception ex)
            {
                ReleaseUdx();
                NDebug.Log("连接错误: " + ex.ToString());
                await UniTask.Yield(); //在unity会切换到unity线程去执行，解决unity组件访问错误问题
                result(false);
                return false;
            }
        }

        public override void ReceiveHandler()
        {
        }

        //IL2CPP使用Marshal.GetFunctionPointerForDelegate需要设置委托方法为静态方法，并且要添加上特性MonoPInvokeCallback
        [MonoPInvokeCallback(typeof(UDXPRC))]
        protected static void ProcessReceive(UDXEVENT_TYPE type, int erro, IntPtr cli, IntPtr pData, int len)//cb回调
        {
            try
            {
                var user = UdxLib.UGetUserData(cli);
                var handle = GCHandle.FromIntPtr(new IntPtr(user));
                var client = handle.Target as UdxClient;
                client.heart = 0;
                switch (type)
                {
                    case UDXEVENT_TYPE.E_CONNECT:
                        if (erro != 0)
                            return;
                        client.ClientPtr = cli;
                        client.Connected = true;
                        UdxLib.USetGameMode(cli, true);
                        break;
                    case UDXEVENT_TYPE.E_LINKBROKEN:
                        client.Connected = false;
                        client.NetworkState = NetworkState.ConnectLost;
                        client.InvokeInMainThread(client.OnConnectLostHandle);
                        client.RpcModels = new QueueSafe<RPCModel>();
                        client.ReleaseUdx();
                        handle.Free();
                        NDebug.Log("断开连接！");
                        break;
                    case UDXEVENT_TYPE.E_DATAREAD:
                        var buffer = BufferPool.Take(len);
                        buffer.Count = len;
                        Marshal.Copy(pData, buffer.Buffer, 0, len);
                        client.receiveCount += len;
                        client.receiveAmount++;
                        client.ResolveBuffer(ref buffer);
                        BufferPool.Push(buffer);
                        break;
                }
            }
            catch (Exception ex)
            {
                NDebug.LogError("处理异常:" + ex);
            }
        }

        protected unsafe override void SendByteData(ISegment buffer)
        {
            if (ClientPtr == IntPtr.Zero)
                return;
            if (buffer.Count <= Frame)//解决长度==5的问题(没有数据)
                return;
            sendAmount++;
            sendCount += buffer.Count;
            fixed (byte* ptr = buffer.Buffer)
            {
                int count = UdxLib.USend(ClientPtr, ptr, buffer.Count);
                if (count <= 0)
                    OnSendErrorHandle?.Invoke(buffer);
            }
        }

        public override void Close(bool await = true, int millisecondsTimeout = 100)
        {
            base.Close(await, millisecondsTimeout);
            ReleaseUdx();
        }

        protected void ReleaseUdx()
        {
            if (ClientPtr != IntPtr.Zero)
            {
                UdxLib.UClose(ClientPtr);
                UdxLib.UUndump(ClientPtr);
                ClientPtr = IntPtr.Zero;
            }
            if (udxObj != IntPtr.Zero)
            {
                UdxLib.UDestroyFUObj(udxObj);
                udxObj = IntPtr.Zero;
            }
        }
    }
}