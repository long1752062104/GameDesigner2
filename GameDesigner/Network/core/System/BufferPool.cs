using System;
using System.Drawing;
using System.Reflection;

namespace Net.System
{
    /// <summary>
    /// 内存片段类型
    /// </summary>
    public enum SegmentType
    {
        /// <summary>
        /// 使用内存片(类内存段)
        /// </summary>
        Segment = 1,
        /// <summary>
        /// 使用内存片(结构内存段)
        /// </summary>
        ArraySegment,
    }

    /// <summary>
    /// 数据缓冲内存池
    /// </summary>
    public static class BufferPool
    {
        /// <summary>
        /// 数据缓冲池大小. 默认65536字节
        /// </summary>
        public static int Size { get; set; } = 65536;
        /// <summary>
        /// 当没有合理回收内存，导致内存泄漏被回收后提示
        /// </summary>
        public static bool Log { get; set; }
        /// <summary>
        /// 使用的缓存块类型
        /// </summary>
        public static SegmentType SegmentType = SegmentType.Segment;

        private static readonly GStack<ISegment>[] STACKS = new GStack<ISegment>[37];
        private static readonly int[] TABLE = new int[] {
            256,512,1024,2048,4096,8192,16384,32768,65536,98304,131072,196608,262144,393216,524288,786432,1048576,
            1572864,2097152,3145728,4194304,6291456,8388608,12582912,16777216,25165824,33554432,50331648,67108864,
            100663296,134217728,201326592,268435456,402653184,536870912,805306368,1073741824
        };

        private static readonly object SyncRoot = new object();

        static BufferPool()
        {
            for (int i = 0; i < TABLE.Length; i++)
                STACKS[i] = new GStack<ISegment>();
            ThreadManager.Invoke("BufferPool", 5f, ReferenceCountCheck, true);
        }

        private static bool ReferenceCountCheck()
        {
            try
            {
                lock (SyncRoot)
                {
                    for (int i = 0; i < STACKS.Length; i++)
                    {
                        foreach (var stack in STACKS[i])
                        {
                            if (stack == null)
                                continue;
                            if (stack.ReferenceCount == 0)
                                stack.Close();
                            stack.ReferenceCount = 0;
                        }
                    }
                }
            }
            catch { }
            return true;
        }

        /// <summary>
        /// 从内存池取数据片
        /// </summary>
        /// <returns></returns>
        public static ISegment Take()
        {
            return Take(Size);
        }

        /// <summary>
        /// 从内存池取数据片
        /// </summary>
        /// <param name="size">内存大小</param>
        /// <returns></returns>
        public static ISegment Take(int size)
        {
#if !SerializeTest
            lock (SyncRoot)
#endif
            {
                int tableInx = 0;
                var table = TABLE;
                var count = table.Length;
                for (int i = 0; i < count; i++)
                {
                    if (size <= table[i])
                    {
                        size = table[i];
                        tableInx = i;
                        goto J;
                    }
                }
            J: var stack = STACKS[tableInx];
                ISegment segment;
            J1: if (stack.Count > 0)
                {
                    segment = stack.Pop();
                    if (!segment.IsRecovery | !segment.IsDespose)
                        goto J1;
                    goto J2;
                }
                var buffer = new byte[size];
                segment = NewSegment(buffer, 0, size, true);
            J2: segment.IsDespose = false;
                segment.ReferenceCount++;
                segment.Init();
                return segment;
            }
        }

        /// <summary>
        /// 压入数据片, 等待复用
        /// </summary>
        /// <param name="segment"></param>
        public static void Push(ISegment segment)
        {
#if !SerializeTest
            lock (SyncRoot)
#endif
            {
                if (!segment.IsRecovery)
                    return;
                if (segment.IsDespose)
                    return;
                segment.IsDespose = true;
                var table = TABLE;
                for (int i = 0; i < table.Length; i++)
                {
                    if (segment.Length == table[i])
                    {
                        STACKS[i].Push(segment);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// 创建内存块
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <param name="isRecovery"></param>
        /// <returns>根据版本设置创建接口对象</returns>
        public static ISegment NewSegment(byte[] buffer, int index, int count, bool isRecovery = false)
        {
            ISegment segment;
            switch (SegmentType)
            {
                case SegmentType.ArraySegment:
                    segment = new ArraySegment(buffer, index, count, isRecovery);
                    break;
                default:
                    segment = new Segment(buffer, index, count, isRecovery);
                    break;
            }
            return segment;
        }
    }

    public static class ObjectPool<T> where T : new()
    {
        private static readonly GStack<T> STACK = new GStack<T>();

        public static void Init(int poolSize)
        {
            for (int i = 0; i < poolSize; i++)
            {
                STACK.Push(new T());
            }
        }

        public static T Take()
        {
            lock (STACK)
            {
                if (STACK.TryPop(out T obj))
                    return obj;
                return new T();
            }
        }

        public static T Take(Action<T> onNew)
        {
            lock (STACK)
            {
                if (STACK.TryPop(out T obj))
                    return obj;
                obj = new T();
                onNew?.Invoke(obj);
                return obj;
            }
        }

        public static void Push(T obj)
        {
            lock (STACK)
            {
                STACK.Push(obj);
            }
        }

        public static void Clear()
        {
            lock (STACK)
            {
                STACK.Clear();
            }
        }
    }

    /// <summary>
    /// 数据缓冲数组池
    /// </summary>
    public static class ArrayPool<T>
    {
        private static readonly GStack<T>[] STACKS;
        private static readonly int[] TABLE = new int[] {
            2, 4,8,16,32,64,128,256,512,1024,2048,4096,8192,16384,32768,65536,98304,131072,196608,262144,393216,
            524288,786432,1048576,1572864,2097152,3145728,4194304,6291456,8388608,12582912,16777216,25165824,
            33554432,50331648,67108864,100663296,134217728,201326592,268435456,402653184,536870912,805306368,1073741824
        };
        private static readonly object SyncRoot = new object();

        static ArrayPool()
        {
            STACKS = new GStack<T>[TABLE.Length];
            for (int i = 0; i < TABLE.Length; i++)
                STACKS[i] = new GStack<T>();
        }

        /// <summary>
        /// 从内存池取数据片
        /// </summary>
        /// <param name="size">内存大小</param>
        /// <returns></returns>
        public static T Take(int size)
        {
#if !SerializeTest
            lock (SyncRoot)
#endif
            {
                int tableInx = 0;
                var table = TABLE;
                var count = table.Length;
                for (int i = 0; i < count; i++)
                {
                    if (size <= table[i])
                    {
                        size = table[i];
                        tableInx = i;
                        goto J;
                    }
                }
            J: var stack = STACKS[tableInx];
                T array;
                if (stack.Count > 0)
                    array = stack.Pop();
                else
                    array = (T)Activator.CreateInstance(typeof(T), size);
                return array;
            }
        }

        /// <summary>
        /// 压入数据片, 等待复用
        /// </summary>
        /// <param name="array"></param>
        public static void Push(T array)
        {
#if !SerializeTest
            lock (SyncRoot)
#endif
            {
                var table = TABLE;
                for (int i = 0; i < table.Length; i++)
                {
                    if ((array as Array).Length == table[i])
                    {
                        STACKS[i].Push(array);
                        return;
                    }
                }
            }
        }
    }
}
