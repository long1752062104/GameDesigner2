﻿using System;
using System.Runtime.CompilerServices;

namespace Net.Share
{
    internal class Inc
    {
        /// <summary>
        /// 参数To或As调用一次+1
        /// </summary>
        internal byte parsIndex;
        /// <summary>
        /// 当数据已经填充, 获取Buffer可直接返回真正数据
        /// </summary>
        internal bool isFill;
    }

    /// <summary>
    /// 远程过程调用模型,此类负责网络通讯中数据解析临时缓存的对象
    /// 经过测试结构体和类差距不大，如果用in修饰符，结构会比较快，但是我们不知道开发者在哪里会使用Asxxx读取参数，如果都全部用in修饰符，则Asxxx会失效，由于只读属性原因，导致parsIndex无法++
    /// </summary>
    public class RPCModel
    {
        /// <summary>
        /// 网络指令
        /// </summary>
        public byte cmd;
        /// <summary>
        /// 内核? true:数据经过框架内部序列化 false:数据由开发者自己处理
        /// </summary>
        public bool kernel;
        /// <summary>
        /// 这是内存池数据，这个字段要配合index，count两字段使用，如果想得到实际数据，请使用Buffer属性
        /// </summary>
        public byte[] buffer;
        /// <summary>
        /// 真正数据段索引和长度
        /// </summary>
        public int index, count;
        /// <summary>
        /// 数据缓存器(正确的数据段)
        /// </summary>
        public byte[] Buffer
        {
            get
            {
                if (inc.isFill)
                    return buffer;
                if (count == 0)
                    return Array.Empty<byte>(); //byte[]不能为空,否则出错
                var array = new byte[count];
                Unsafe.CopyBlock(ref array[0], ref buffer[index], (uint)count);
                return array;
            }
        }
        /// <summary>
        /// 协议值, 合并之前版本的func字段和methodHash字段
        /// </summary>
        public uint protocol;
        /// <summary>
        /// 远程参数
        /// </summary>
        public object[] pars;
        /// <summary>
        /// 数据是否经过内部序列化?
        /// </summary>
        public bool serialize;
        /// <summary>
        /// 请求和响应的Token, 当几千几万个客户端同时发起相同的请求时, 可以根据Token区分响应, 得到真正的响应值
        /// </summary>
        public uint token;
        private readonly Inc inc;

        public RPCModel(byte cmd = default, bool kernel = default, byte[] buffer = default, int index = default, int count = default, uint protocol = default, object[] pars = default, bool serialize = default, uint token = default)
        {
            this.inc = new Inc();
            this.cmd = cmd;
            this.kernel = kernel;
            this.buffer = buffer;
            this.index = index;
            this.count = count;
            this.protocol = protocol;
            this.pars = pars;
            this.serialize = serialize;
            this.token = token;
        }

        public RPCModel Reset(bool kernel = default, byte cmd = default, byte[] buffer = default, int index = default, int count = default, uint protocol = default, object[] pars = default, bool serialize = default, uint token = default)
        {
            this.inc.isFill = false;
            this.inc.parsIndex = 0;
            this.kernel = kernel;
            this.cmd = cmd;
            this.buffer = buffer;
            this.index = index;
            this.count = count;
            this.protocol = protocol;
            this.pars = pars;
            this.serialize = serialize;
            this.token = token;
            return this;
        }

        /// <summary>
        /// 每次调用参数都会指向下一个参数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T To<T>()
        {
            var t = (T)pars[inc.parsIndex];
            inc.parsIndex++;
            return t;
        }

        /// <summary>
        /// 每次调用参数都会指向下一个参数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T As<T>() where T : class
        {
            var t = pars[inc.parsIndex] as T;
            inc.parsIndex++;
            return t;
        }

        public byte AsByte { get => To<byte>(); }
        public sbyte AsSbyte { get => To<sbyte>(); }
        public bool AsBoolen { get => To<bool>(); }
        public short AsShort { get => To<short>(); }
        public ushort AsUshort { get => To<ushort>(); }
        public char AsChar { get => To<char>(); }
        public int AsInt { get => To<int>(); }
        public uint AsUint { get => To<uint>(); }
        public float AsFloat { get => To<float>(); }
        public long AsLong { get => To<long>(); }
        public ulong AsUlong { get => To<ulong>(); }
        public double AsDouble { get => To<double>(); }
        public string AsString { get => As<string>(); }

        public object Obj
        {
            get
            {
                var obj = pars[inc.parsIndex];
                inc.parsIndex++;
                return obj;
            }
        }

        /// <summary>
        /// 类信息字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var fields = typeof(NetCmd).GetFields(global::System.Reflection.BindingFlags.Static | global::System.Reflection.BindingFlags.Public);
            var cmdStr = string.Empty;
            for (int i = 0; i < fields.Length; i++)
            {
                if (cmd.Equals(fields[i].GetValue(null)))
                {
                    cmdStr = fields[i].Name;
                    break;
                }
            }
            return $"指令:{cmdStr} 内核:{kernel} 协议:{protocol} 数据:{(buffer != null ? buffer.Length : 0)}";
        }

        public void Flush()
        {
            buffer = Buffer;
            index = 0;
            count = buffer.Length;
            inc.isFill = true;
        }

        /// <summary>
        /// 复制远程调用数据模型
        /// </summary>
        /// <returns></returns>
        public RPCModel Copy()
        {
            return new RPCModel(cmd: cmd, kernel: kernel, buffer: Buffer, serialize: false);
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 23 + kernel.GetHashCode();
            hash = hash * 23 + cmd.GetHashCode();
            hash = hash * 23 + index.GetHashCode();
            hash = hash * 23 + count.GetHashCode();
            hash = hash * 23 + protocol.GetHashCode();
            hash = hash * 23 + serialize.GetHashCode();
            return hash;
        }

        public override bool Equals(object obj)
        {
            if (obj is RPCModel model)
            {
                if (model.cmd != cmd)
                    return false;
                if (model.kernel != kernel)
                    return false;
                if (model.index != index)
                    return false;
                if (model.count != count)
                    return false;
                if (model.protocol != protocol)
                    return false;
                if (model.serialize != serialize)
                    return false;
                return true;
            }
            return false;
        }

        public static bool operator ==(RPCModel a, RPCModel b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(RPCModel a, RPCModel b)
        {
            return !a.Equals(b);
        }
    }
}
