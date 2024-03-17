using System.Threading;

namespace Net.Common
{
    /// <summary>
    /// 快速锁 (共享锁), 比lock性能要好很多
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
        public bool TryEnter()
        {
            if (Thread.CurrentThread == currentLock)
                return true;
            return isLocked == 0;
        }

        /// <summary>
        /// 退出锁
        /// </summary>
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
        public void Lock() => Enter();

        /// <summary>
        /// 释放锁
        /// </summary>
        public void Release() => Exit();
    }
}