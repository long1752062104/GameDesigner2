﻿using Net.System;
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
                _payloadLength = (byte)126;
                _extPayloadLength = ((ushort)len).ToByteArray(ByteOrder.Big);
            }
            else
            {
                _payloadLength = (byte)127;
                _extPayloadLength = len.ToByteArray(ByteOrder.Big);
            }

            if (mask)
            {
                _mask = Mask.On;
                _maskingKey = createMaskingKey();

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

        private static byte[] createMaskingKey()
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
                header = (header << 7) + (int)_payloadLength;

                var uint16Header = (ushort)header;
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

    public class WebSocketSession
    {
        internal static readonly int _defaultHeaderLength = 2;
        internal static readonly int _defaultMaskingKeyLength = 4;
        internal static readonly byte[] EmptyBytes = new byte[0];
        internal static RandomNumberGenerator RandomNumber;
        internal Socket socket;
        private ISegment fragment;
        private Opcode opcode;
        private bool isCompressed;
        internal Stream stream;
        private MemoryStream ms;
        internal bool isHandshake;
        public int FragmentLength { get; private set; }
        public bool Connected => socket.Connected;

        public WebSocketSession(Socket socket)
        {
            this.socket = socket;
            ms = new MemoryStream(socket.ReceiveBufferSize);
            FragmentLength = 1016;
            RandomNumber = new RNGCryptoServiceProvider();
        }

        internal void Receive(Action<Opcode, ISegment> onMessage)
        {
            if (socket.Poll(0, SelectMode.SelectRead))
            {
                ISegment segment;
                if (ms.Position > 0)
                {
                    var buffer = ms.GetBuffer();
                    ms.Position += stream.Read(buffer, (int)ms.Position, buffer.Length - (int)ms.Position);
                    segment = BufferPool.NewSegment(buffer, 0, (int)ms.Position);
                    ms.Position = 0;
                }
                else
                {
                    segment = BufferPool.Take();
                    segment.Count = stream.Read(segment.Buffer, 0, segment.Length);
                }
                int canReadCount;
                while (segment.Position < segment.Count)
                {
                    canReadCount = segment.Count - segment.Position;
                    if (canReadCount < _defaultHeaderLength)
                    {
                        ms.Write(segment.Buffer, segment.Position, canReadCount);
                        break;
                    }
                    var header = segment.Read(_defaultHeaderLength);
                    var frame = new WebSocketFrame
                    {
                        _fin = (header[0] & 0x80) == 0x80 ? Fin.Final : Fin.More,
                        _rsv1 = (header[0] & 0x40) == 0x40 ? Rsv.On : Rsv.Off,
                        _rsv2 = (header[0] & 0x20) == 0x20 ? Rsv.On : Rsv.Off,
                        _rsv3 = (header[0] & 0x10) == 0x10 ? Rsv.On : Rsv.Off,
                        _opcode = (Opcode)(byte)(header[0] & 0x0f),
                        _mask = (header[1] & 0x80) == 0x80 ? Mask.On : Mask.Off,
                        _payloadLength = (byte)(header[1] & 0x7f)
                    };
                    var extendedPayloadLengthWidth = frame.ExtendedPayloadLengthWidth;
                    if (extendedPayloadLengthWidth > 0)
                    {
                        canReadCount = segment.Count - segment.Position;
                        if (canReadCount < extendedPayloadLengthWidth)
                        {
                            ms.Write(segment.Buffer, segment.Position, canReadCount);
                            break;
                        }
                        frame._extPayloadLength = segment.Read(extendedPayloadLengthWidth);
                    }
                    if (!frame.IsMasked)
                        frame._maskingKey = EmptyBytes;
                    else
                    {
                        canReadCount = segment.Count - segment.Position;
                        if (canReadCount < _defaultMaskingKeyLength)
                        {
                            ms.Write(segment.Buffer, segment.Position, canReadCount);
                            break;
                        }
                        frame._maskingKey = segment.Read(_defaultMaskingKeyLength);
                    }
                    var exactPayloadLen = (int)frame.ExactPayloadLength;
                    var exactPayloadPos = segment.Position;
                    canReadCount = segment.Count - segment.Position;
                    if (canReadCount < exactPayloadLen)
                    {
                        ms.Write(segment.Buffer, segment.Position, canReadCount);
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
                        onMessage?.Invoke(opcode, fragment);
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
                        onMessage?.Invoke(frame._opcode, segment);
                        segment.Position = pos;
                        segment.Count = count;
                    }
                    else if (frame.IsPing)
                    {
                        var pong = WebSocketFrame.CreatePongFrame(frame._payloadData, false);
                        var bytes = pong.ToArray();
                        stream.Write(bytes, 0, bytes.Length);
                    }
                    else if (frame.IsPong)
                    {

                    }
                    else if (frame.IsClose)
                    {
                        var pong = WebSocketFrame.CreateCloseFrame(frame._payloadData, false);
                        var bytes = pong.ToArray();
                        stream.Write(bytes, 0, bytes.Length);
                    }
                    else
                    {

                    }
                }
                segment.Dispose();
            }
        }

        internal bool PerformHandshake()
        {
            if (socket.Poll(0, SelectMode.SelectRead))
            {
                var segment = BufferPool.Take();
                segment.Count = stream.Read(segment.Buffer, 0, segment.Length);
                // 读取 HTTP 请求头
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
                // 检查是否为 WebSocket 握手请求
                var upgrade = headers.Get("Upgrade");
                if (upgrade != "websocket")
                    return false;
                // 解析 Sec-WebSocket-Key
                var key = headers.Get("Sec-WebSocket-Key");
                // 生成响应
                var responseKey = Convert.ToBase64String(SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(key + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")));
                // 构建握手响应
                string response = $"HTTP/1.1 101 Switching Protocols\r\n" +
                                  $"Server: websocket-gdnet/1.0\r\n" +
                                  $"Upgrade: websocket\r\n" +
                                  $"Connection: Upgrade\r\n" +
                                  $"Sec-WebSocket-Accept: {responseKey}\r\n\r\n";
                // 发送握手响应
                var responseBytes = Encoding.UTF8.GetBytes(response);
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

        internal void Send(byte[] buffer, int offset, int count)
        {
            send(Opcode.Binary, new MemoryStream(buffer, offset, count), false);
        }

        private bool send(Opcode opcode, Stream dataStream, bool compressed)
        {
            var len = dataStream.Length;

            if (len == 0)
                return send(Fin.Final, opcode, EmptyBytes, false);

            var quo = len / FragmentLength;
            var rem = (int)(len % FragmentLength);

            byte[] buff = null;

            if (quo == 0)
            {
                buff = new byte[rem];

                return dataStream.Read(buff, 0, rem) == rem
                       && send(Fin.Final, opcode, buff, compressed);
            }

            if (quo == 1 && rem == 0)
            {
                buff = new byte[FragmentLength];

                return dataStream.Read(buff, 0, FragmentLength) == FragmentLength
                       && send(Fin.Final, opcode, buff, compressed);
            }

            /* Send fragments */

            // Begin

            buff = new byte[FragmentLength];

            var sent = dataStream.Read(buff, 0, FragmentLength) == FragmentLength
                       && send(Fin.More, opcode, buff, compressed);

            if (!sent)
                return false;

            // Continue

            var n = rem == 0 ? quo - 2 : quo - 1;

            for (long i = 0; i < n; i++)
            {
                sent = dataStream.Read(buff, 0, FragmentLength) == FragmentLength
                       && send(Fin.More, Opcode.Cont, buff, false);

                if (!sent)
                    return false;
            }

            // End

            if (rem == 0)
                rem = FragmentLength;
            else
                buff = new byte[rem];

            return dataStream.Read(buff, 0, rem) == rem
                   && send(Fin.Final, Opcode.Cont, buff, false);
        }

        private bool send(Fin fin, Opcode opcode, byte[] data, bool compressed)
        {
            var frame = new WebSocketFrame(fin, opcode, data, compressed, false);
            var rawFrame = frame.ToArray();

            stream.Write(rawFrame, 0, rawFrame.Length);

            return true;
        }
    }

    public class WebPlayerNew : NetPlayer
    {
        public WebSocketSession Session;
    }
}
