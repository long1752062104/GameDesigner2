using System.Threading;

namespace Net.Common
{
    /// <summary>
    /// 快速锁 (共享锁), 比lock性能要好很多
    /// </summary>
    public class FastLocking
    {
        private int isLocked;

        /// <summary>
        /// 进入锁
        /// </summary>
        public void Enter()
        {
            var spin = new SpinWait();
            while (Interlocked.Exchange(ref isLocked, 1) != 0)
                spin.SpinOnce();
        }

        /// <summary>
        /// 尝试获取锁
        /// </summary>
        /// <returns></returns>
        public bool TryEnter()
        {
            return Interlocked.Exchange(ref isLocked, 1) != 0;
        }

        /// <summary>
        /// 退出锁
        /// </summary>
        public void Exit()
        {
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