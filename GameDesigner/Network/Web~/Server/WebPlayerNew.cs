using Net.Helper;
using Net.Share;
using Net.System;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using WebSocketSharp;

namespace Net.Server
{
    internal enum Fin : byte
    {
        /// <summary>
        /// Equivalent to numeric value 0. Indicates more frames of a message follow.
        /// </summary>
        More = 0x0,
        /// <summary>
        /// Equivalent to numeric value 1. Indicates the final frame of a message.
        /// </summary>
        Final = 0x1
    }
    internal enum Rsv : byte
    {
        /// <summary>
        /// Equivalent to numeric value 0. Indicates zero.
        /// </summary>
        Off = 0x0,
        /// <summary>
        /// Equivalent to numeric value 1. Indicates non-zero.
        /// </summary>
        On = 0x1
    }
    public enum Opcode : byte
    {
        /// <summary>
        /// Equivalent to numeric value 0. Indicates continuation frame.
        /// </summary>
        Cont = 0x0,
        /// <summary>
        /// Equivalent to numeric value 1. Indicates text frame.
        /// </summary>
        Text = 0x1,
        /// <summary>
        /// Equivalent to numeric value 2. Indicates binary frame.
        /// </summary>
        Binary = 0x2,
        /// <summary>
        /// Equivalent to numeric value 8. Indicates connection close frame.
        /// </summary>
        Close = 0x8,
        /// <summary>
        /// Equivalent to numeric value 9. Indicates ping frame.
        /// </summary>
        Ping = 0x9,
        /// <summary>
        /// Equivalent to numeric value 10. Indicates pong frame.
        /// </summary>
        Pong = 0xa
    }
    internal enum Mask : byte
    {
        /// <summary>
        /// Equivalent to numeric value 0. Indicates not masked.
        /// </summary>
        Off = 0x0,
        /// <summary>
        /// Equivalent to numeric value 1. Indicates masked.
        /// </summary>
        On = 0x1
    }

    public class WebSocketFrame
    {
        internal Fin _fin;
        internal Rsv _rsv1;
        internal Rsv _rsv2;
        internal Rsv _rsv3;
        internal Opcode _opcode;
        internal Mask _mask;
        internal byte _payloadLength;
        internal byte[] _maskingKey;
        internal byte[] _extPayloadLength;
        internal byte[] _payloadData;

        public bool IsMasked => _mask == Mask.On;
        /// <summary>
        /// 最后一帧，完成帧
        /// </summary>
        public bool IsFinal => _fin == Fin.Final;
        /// <summary>
        /// 还有其他帧
        /// </summary>
        public bool IsFragment => _fin == Fin.More || _opcode == Opcode.Cont;
        public bool IsData => _opcode == Opcode.Text || _opcode == Opcode.Binary;
        internal int ExtendedPayloadLengthWidth => _payloadLength < 126 ? 0 : _payloadLength == 126 ? 2 : 8;
        internal ulong ExactPayloadLength => _payloadLength < 126 ? _payloadLength : _payloadLength == 126 ? _extPayloadLength.ToUInt16(ByteOrder.Big) : _extPayloadLength.ToUInt64(ByteOrder.Big);
        public bool IsContinuation => _opcode == Opcode.Cont;
        public bool IsCompressed => _rsv1 == Rsv.On;
        public bool IsPing => _opcode == Opcode.Ping;
        public bool IsPong => _opcode == Opcode.Pong;
        public bool IsClose => _opcode == Opcode.Close;

        internal WebSocketFrame() { }

        internal WebSocketFrame(Fin fin, Opcode opcode, byte[] payloadData, bool compressed, bool mask)
        {
            _fin = fin;
            _opcode = opcode;

            _rsv1 = compressed ? Rsv.On : Rsv.Off;
            _rsv2 = Rsv.Off;
            _rsv3 = Rsv.Off;

            var len = payloadData.Length;

            if (len < 126)
            {
                _payloadLength = (byte)len;
                _extPayloadLength = WebSocketSession.EmptyBytes;
            }
            else if (len < 0x010000)
            {
                _payloadLength = 126;
                _extPayloadLength = ((ushort)len).ToByteArray(ByteOrder.Big);
            }
            else
            {
                _payloadLength = 127;
                _extPayloadLength = len.ToByteArray(ByteOrder.Big);
            }

            if (mask)
            {
                _mask = Mask.On;
                _maskingKey = CreateMaskingKey();

                for (int i = 0; i < len; i++)
                    payloadData[i] = (byte)(payloadData[i] ^ _maskingKey[i % 4]);
            }
            else
            {
                _mask = Mask.Off;
                _maskingKey = WebSocketSession.EmptyBytes;
            }

            _payloadData = payloadData;
        }

        private static byte[] CreateMaskingKey()
        {
            var key = new byte[WebSocketSession._defaultMaskingKeyLength];
            WebSocketSession.RandomNumber.GetBytes(key);
            return key;
        }

        internal static WebSocketFrame CreatePongFrame(byte[] payloadData, bool mask)
        {
            return new WebSocketFrame(Fin.Final, Opcode.Pong, payloadData, false, mask);
        }

        internal static WebSocketFrame CreateCloseFrame(byte[] payloadData, bool mask)
        {
            return new WebSocketFrame(Fin.Final, Opcode.Close, payloadData, false, mask);
        }

        public byte[] ToArray()
        {
            using (var buff = new MemoryStream())
            {
                var header = (int)_fin;
                header = (header << 1) + (int)_rsv1;
                header = (header << 1) + (int)_rsv2;
                header = (header << 1) + (int)_rsv3;
                header = (header << 4) + (int)_opcode;
                header = (header << 1) + (int)_mask;
                header = (header << 7) + _payloadLength;

                ushort uint16Header = (ushort)header;
                var rawHeader = uint16Header.ToByteArray(ByteOrder.Big);

                buff.Write(rawHeader, 0, WebSocketSession._defaultHeaderLength);

                if (_payloadLength >= 126)
                    buff.Write(_extPayloadLength, 0, _extPayloadLength.Length);

                if (_mask == Mask.On)
                    buff.Write(_maskingKey, 0, WebSocketSession._defaultMaskingKeyLength);

                if (_payloadLength > 0)
                {
                    var bytes = _payloadData.ToArray();

                    if (_payloadLength > 126)
                        buff.WriteBytes(bytes, 1024);
                    else
                        buff.Write(bytes, 0, bytes.Length);
                }

                buff.Close();

                return buff.ToArray();
            }
        }
    }

    public delegate void OnMessageHandler(object state, Opcode opcode, ref ISegment segment);

    public class WebSocketSession
    {
        internal static readonly int _defaultHeaderLength = 2;
        internal static readonly int _defaultMaskingKeyLength = 4;
        internal static readonly byte[] EmptyBytes = Array.Empty<byte>();
        internal static RandomNumberGenerator RandomNumber;
        internal Socket socket;
        private ISegment fragment;
        private Opcode opcode;
        private bool isCompressed;
        internal Stream stream;
        private readonly ISegment bufferStream;
        internal bool isHandshake;
        public int FragmentLength { get; private set; }
        public bool Connected => socket.Connected;
        public OnMessageHandler onMessageHandler;
        private readonly WebSocketFrame frame;
        internal int performance;

        public WebSocketSession(Socket socket)
        {
            this.socket = socket;
            bufferStream = BufferPool.Take(socket.ReceiveBufferSize);
            FragmentLength = 1016;
            RandomNumber = new RNGCryptoServiceProvider();
            frame = new WebSocketFrame();
        }

        public void Receive(object state)
        {
            Receive(state, onMessageHandler);
        }

        public unsafe void Receive(object state, OnMessageHandler onMessage)
        {
            if (socket.Poll(performance, SelectMode.SelectRead))
            {
                ISegment segment;
                if (bufferStream.Position > 0)
                {
                    var buffer = bufferStream.Buffer;
                    bufferStream.Position += stream.Read(buffer, bufferStream.Position, buffer.Length - bufferStream.Position);
                    segment = BufferPool.NewSegment(buffer, 0, bufferStream.Position);
                    bufferStream.SetPositionLength(0);
                }
                else
                {
                    segment = BufferPool.Take();
                    segment.Count = stream.Read(segment.Buffer, 0, segment.Length);
                }
                int startPos, canReadCount;
                while (segment.Position < segment.Count)
                {
                    startPos = segment.Position; //初始位置
                    canReadCount = segment.Count - startPos;
                    if (canReadCount < _defaultHeaderLength)
                    {
                        bufferStream.Write(segment.Buffer, startPos, canReadCount);
                        break;
                    }
                    var header = segment.ReadPtr(_defaultHeaderLength);
                    frame._fin = (header[0] & 0x80) == 0x80 ? Fin.Final : Fin.More;
                    frame._rsv1 = (header[0] & 0x40) == 0x40 ? Rsv.On : Rsv.Off;
                    frame._rsv2 = (header[0] & 0x20) == 0x20 ? Rsv.On : Rsv.Off;
                    frame._rsv3 = (header[0] & 0x10) == 0x10 ? Rsv.On : Rsv.Off;
                    frame._opcode = (Opcode)(byte)(header[0] & 0x0f);
                    frame._mask = (header[1] & 0x80) == 0x80 ? Mask.On : Mask.Off;
                    frame._payloadLength = (byte)(header[1] & 0x7f);
                    var extendedPayloadLengthWidth = frame.ExtendedPayloadLengthWidth;
                    if (extendedPayloadLengthWidth > 0)
                    {
                        canReadCount = segment.Count - segment.Position;
                        if (canReadCount < extendedPayloadLengthWidth)
                        {
                            bufferStream.Write(segment.Buffer, startPos, segment.Count - startPos);
                            break;
                        }
                        frame._extPayloadLength = new byte[extendedPayloadLengthWidth];
                        segment.Read(frame._extPayloadLength, extendedPayloadLengthWidth);
                    }
                    if (frame.IsMasked)
                    {
                        canReadCount = segment.Count - segment.Position;
                        if (canReadCount < _defaultMaskingKeyLength)
                        {
                            bufferStream.Write(segment.Buffer, startPos, segment.Count - startPos);
                            break;
                        }
                        frame._maskingKey ??= new byte[_defaultMaskingKeyLength];
                        segment.Read(frame._maskingKey, _defaultMaskingKeyLength);
                    }
                    var exactPayloadLen = (int)frame.ExactPayloadLength;
                    var exactPayloadPos = segment.Position;
                    canReadCount = segment.Count - segment.Position;
                    if (canReadCount < exactPayloadLen)
                    {
                        bufferStream.Write(segment.Buffer, startPos, segment.Count - startPos);
                        break;
                    }
                    segment.Position += exactPayloadLen;
                    if (frame.IsMasked)
                    {
                        for (int i = exactPayloadPos; i < exactPayloadPos + exactPayloadLen; i++)
                            segment[i] = (byte)(segment[i] ^ frame._maskingKey[(i - exactPayloadPos) % 4]);
                    }
                    if (frame.IsFragment)
                    {
                        if (fragment == null)
                        {
                            if (frame.IsContinuation)
                                continue;
                            fragment = BufferPool.Take();
                            opcode = frame._opcode;
                            isCompressed = frame.IsCompressed;
                        }
                        var dataCount = fragment.Position + exactPayloadLen;
                        if (dataCount >= fragment.Length)
                        {
                            BufferPool.Push(fragment);
                            fragment = BufferPool.Take(dataCount);
                        }
                        fragment.Write(segment.Buffer, exactPayloadPos, exactPayloadLen);
                        if (!frame.IsFinal)
                            continue;
                        fragment.Flush();
                        if (isCompressed)
                        {
                        }
                        onMessage?.Invoke(state, opcode, ref fragment);
                        BufferPool.Push(fragment);
                        fragment = null;
                        continue;
                    }
                    else if (frame.IsData)
                    {
                        var pos = segment.Position;
                        var count = segment.Count;
                        segment.Position = exactPayloadPos;
                        segment.Count = exactPayloadPos + exactPayloadLen;
                        onMessage?.Invoke(state, frame._opcode, ref segment);
                        segment.Position = pos;
                        segment.Count = count;
                    }
                    else if (frame.IsPing)
                    {
                        var pong = WebSocketFrame.CreatePongFrame(EmptyBytes, false);
                        var bytes = pong.ToArray();
                        stream.Write(bytes, 0, bytes.Length);
                    }
                    else if (frame.IsClose)
                    {
                        var pong = WebSocketFrame.CreateCloseFrame(EmptyBytes, false);
                        var bytes = pong.ToArray();
                        stream.Write(bytes, 0, bytes.Length);
                    }
                }
                segment.Dispose();
            }
        }

        internal bool PerformHandshake()
        {
            if (socket.Poll(performance, SelectMode.SelectRead))
            {
                var segment = BufferPool.Take();
                segment.Count = stream.Read(segment.Buffer, 0, segment.Length);
                var requestBuilder = Encoding.UTF8.GetString(segment.Buffer, 0, segment.Count);
                var headers = new NameValueCollection();
                var headersText = requestBuilder.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
                for (int i = 1; i < headersText.Length; i++)
                {
                    var idx = headersText[i].IndexOf(':'); //必须用这个，否则遇到"Host: 192.168.1.6:9543"会出问题
                    var name = headersText[i].Substring(0, idx).Trim();
                    var value = headersText[i].Substring(idx + 1).Trim();
                    headers.Add(name, value);
                }
                var upgrade = headers.Get("Upgrade");
                if (upgrade != "websocket")
                    return false;
                var key = headers.Get("Sec-WebSocket-Key");
                var responseKey = Convert.ToBase64String(SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(key + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")));
                var response = new StringBuilder();
                response.AppendLine("HTTP/1.1 101 Switching Protocols");
                response.AppendLine("Server: websocket-gdnet/1.0");
                response.AppendLine("Upgrade: websocket");
                response.AppendLine("Connection: Upgrade");
                response.AppendLine($"Sec-WebSocket-Accept: {responseKey}");
                response.AppendLine(); //必须有最后这个换行，否则失败
                var responseBytes = Encoding.UTF8.GetBytes(response.ToString());
                stream.Write(responseBytes);
                isHandshake = true;
                return true;
            }
            return false;
        }

        internal void Close()
        {
            socket.Close();
        }

        internal void Send(string text)
        {
            var buffer = text.ToBytes();
            Send(Opcode.Text, new MemoryStream(buffer, 0, buffer.Length), false);
        }

        internal void Send(byte[] buffer, int offset, int count)
        {
            Send(Opcode.Binary, new MemoryStream(buffer, offset, count), false);
        }

        private bool Send(Opcode opcode, Stream dataStream, bool compressed)
        {
            var len = dataStream.Length;
            if (len == 0)
                return Send(Fin.Final, opcode, EmptyBytes, false);
            var quo = len / FragmentLength;
            var rem = (int)(len % FragmentLength);
            byte[] buff;
            if (quo == 0)
            {
                buff = new byte[rem];
                return dataStream.Read(buff, 0, rem) == rem && Send(Fin.Final, opcode, buff, compressed);
            }
            if (quo == 1 && rem == 0)
            {
                buff = new byte[FragmentLength];
                return dataStream.Read(buff, 0, FragmentLength) == FragmentLength && Send(Fin.Final, opcode, buff, compressed);
            }
            buff = new byte[FragmentLength];
            var sent = dataStream.Read(buff, 0, FragmentLength) == FragmentLength && Send(Fin.More, opcode, buff, compressed);
            if (!sent)
                return false;
            var n = rem == 0 ? quo - 2 : quo - 1;
            for (long i = 0; i < n; i++)
            {
                sent = dataStream.Read(buff, 0, FragmentLength) == FragmentLength && Send(Fin.More, Opcode.Cont, buff, false);
                if (!sent)
                    return false;
            }
            if (rem == 0)
                rem = FragmentLength;
            else
                buff = new byte[rem];
            return dataStream.Read(buff, 0, rem) == rem && Send(Fin.Final, Opcode.Cont, buff, false);
        }

        internal bool Send(Fin fin, Opcode opcode, byte[] data, bool compressed)
        {
            var frame = new WebSocketFrame(fin, opcode, data, compressed, false);
            var rawFrame = frame.ToArray();
            stream.WriteAsync(rawFrame, 0, rawFrame.Length);
            return true;
        }
    }

    public class WebPlayerNew : NetPlayer
    {
        public WebSocketSession Session;
    }
}
