using Net.Event;
using Net.Serialize;
using System.Runtime.CompilerServices;

namespace Net.System
{
    public class FixedSegment : Segment
    {
        /// <summary>
        /// 构造内存分片
        /// </summary>
        /// <param name="buffer"></param>
        public FixedSegment(byte[] buffer) : base(buffer, 0, buffer.Length) { }

        /// <summary>
        /// 构造内存分片
        /// </summary>
        /// <param name="buffer"></param>
        public FixedSegment(byte[] buffer, bool isRecovery) : base(buffer, 0, buffer.Length, isRecovery) { }

        /// <summary>
        /// 构造内存分片
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        public FixedSegment(byte[] buffer, int index, int count, bool isRecovery = true) : base(buffer, index, count, isRecovery) { }

        public static implicit operator FixedSegment(byte[] buffer)
        {
            return new FixedSegment(buffer);
        }

        public static implicit operator byte[](FixedSegment segment)
        {
            return segment.buffer;
        }

        ~FixedSegment()
        {
            if (IsRecovery && BufferPool.Log)
                NDebug.LogError("片段内存泄漏!请检查代码正确Push内存池!");
            Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override unsafe void Write(ushort value)
        {
            Write(&value, 2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override unsafe void Write(uint value)
        {
            Write(&value, 4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override unsafe void Write(ulong value)
        {
            Write(&value, 8);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override unsafe void Write(string value)
        {
            fixed (byte* ptr1 = &buffer[position])
            {
                int offset = 0;
                NetConvertHelper.Write(ptr1, ref offset, value);
                position += offset;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override ushort ReadUInt16()
        {
            return ReadUInt16Fixed();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override uint ReadUInt32()
        {
            return ReadUInt32Fixed();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override ulong ReadUInt64()
        {
            return ReadUInt64Fixed();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override unsafe string ReadString()
        {
            fixed (byte* ptr1 = &buffer[position])
            {
                int offset = 0;
                var value = NetConvertHelper.Read(ptr1, ref offset);
                position += offset;
                return value;
            }
        }
    }
}