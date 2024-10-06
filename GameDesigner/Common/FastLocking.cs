using System.Threading;
using System.Runtime.CompilerServices;

namespace Net.Common
{
    /// <summary>
    /// 快速锁 (共享锁), 比lock性能要好很多 --可单线程套娃锁
    /// </summary>
    public class FastLocking
    {
        private int isLocked;
        private Thread currentLock;
        private int lockCount;
        public Thread CurrentThread => currentLock;

        /// <summary>
        /// 进入锁
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enter()
        {
            var spin = new SpinWait();
            while (Interlocked.Exchange(ref isLocked, 1) != 0)
            {
                if (Thread.CurrentThread == currentLock) //解决在一个线程内多次套娃锁导致的问题
                {
                    lockCount++;
                    return;
                }
                spin.SpinOnce();
            }
            currentLock = Thread.CurrentThread;
            lockCount = 1;
        }

        /// <summary>
        /// 尝试获取锁
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnter()
        {
            if (Thread.CurrentThread == currentLock)
                return true;
            return isLocked == 0;
        }

        /// <summary>
        /// 退出锁
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Exit()
        {
            if (--lockCount > 0) //当线程有套娃锁的时候才有效果
                return;
            currentLock = null; //这里必须要设置，如果不设置则会导致多线程同时访问下锁无效问题
            Interlocked.Exchange(ref isLocked, 0);
        }

        /// <summary>
        /// 加锁
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Lock() => Enter();

        /// <summary>
        /// 释放锁
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Release() => Exit();
    }

    /// <summary>
    /// 快速锁 (共享锁), 比lock性能要好很多 --不可单线程套娃锁，
    /// 当Task线程池执行后加锁，在锁期间await Task.Delay进入下一个线程池，然后新的请求获得的是上个Task的线程池线程对象，也不能立即进入锁内的代码块，如果进入则导致锁无效!!!
    /// </summary>
    public class FastLockingTask
    {
        private int isLocked;

        /// <summary>
        /// 进入锁
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enter()
        {
            var spin = new SpinWait();
            while (Interlocked.Exchange(ref isLocked, 1) != 0)
            {
                spin.SpinOnce();
            }
        }

        /// <summary>
        /// 尝试获取锁
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnter()
        {
            return isLocked == 0;
        }

        /// <summary>
        /// 退出锁
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Exit()
        {
            Interlocked.Exchange(ref isLocked, 0);
        }

        /// <summary>
        /// 加锁
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Lock() => Enter();

        /// <summary>
        /// 释放锁
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Release() => Exit();
    }
}