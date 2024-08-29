using System;
using Net.Common;

namespace Net.Distributed
{
    /// <summary>
    /// 唯一ID生成器
    /// </summary>
    public class UniqueIdGenerator
    {
        private long machineId = 0; // 假设机器标识为 123
        private int maxBits = 10; // 最大值为 1024，因此需要 10 位表示
        private int sequenceBits = 54; // 64 - 10
        private long sequence = 0;
        private bool useMachineId = true;
        private readonly FastLocking locking = new FastLocking();

        public UniqueIdGenerator() { }

        public UniqueIdGenerator(int machineId, long uniqueIdMax) : this(true, machineId, 10, uniqueIdMax) { }

        public UniqueIdGenerator(bool useMachineId, int machineId, long uniqueIdMax) : this(useMachineId, machineId, 10, uniqueIdMax) { }

        public UniqueIdGenerator(bool useMachineId, int machineId, int machineIdBits, long uniqueIdMax)
        {
            this.useMachineId = useMachineId;
            this.machineId = machineId;
            if (useMachineId)
                sequence = uniqueIdMax >> machineIdBits;
            else
                sequence = uniqueIdMax;
            SetMachineIdBits(machineIdBits);
        }

        /// <summary>
        /// 设置机器ID占用比特位
        /// </summary>
        /// <param name="machineIdBits"></param>
        public void SetMachineIdBits(int machineIdBits)
        {
            if (machineIdBits >= 64)
                return;
            maxBits = machineIdBits;
            sequenceBits = 64 - machineIdBits;
        }

        /// <summary>
        /// 设置机器ID
        /// </summary>
        /// <param name="machineId"></param>
        public void SetMachineId(int machineId)
        {
            this.machineId = machineId;
        }

        /// <summary>
        /// 设置当前序号
        /// </summary>
        /// <param name="sequence"></param>
        public void SetSequenceId(long sequence)
        {
            this.sequence = sequence;
        }

        /// <summary>
        /// 获取新的唯一ID
        /// </summary>
        /// <returns></returns>
        public long NewUniqueId()
        {
            locking.Enter();
            long uniqueId = 0;
            if (useMachineId)
            {
                uniqueId |= (++sequence & ((1L << sequenceBits) - 1)) << (64 - sequenceBits);
                uniqueId |= (machineId & ((1L << maxBits) - 1)) << 1;
            }
            else uniqueId = ++sequence;
            locking.Exit();
            return uniqueId;
        }

        /// <summary>
        /// 获取当前唯一ID
        /// </summary>
        /// <returns></returns>
        public long CurrentId()
        {
            locking.Enter();
            long uniqueId = 0;
            if (useMachineId)
            {
                uniqueId |= (sequence & ((1L << sequenceBits) - 1)) << (64 - sequenceBits);
                uniqueId |= (machineId & ((1L << maxBits) - 1)) << 1;
            }
            else uniqueId = sequence;
            locking.Exit();
            return uniqueId;
        }

        /// <summary>
        /// 获取二进制比特位字符串
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        public string GetBinaryBits(long uniqueId)
        {
            return Convert.ToString(uniqueId, 2).PadLeft(64, '0');
        }
    }
}