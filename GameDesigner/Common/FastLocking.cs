using System.Threading;

namespace Net.Common
{
    /// <summary>
    /// 快速锁 (共享锁), 比lock性能要好很多
    /// </summary>
    public class FastLocking
    {
        private int isLocked;

        public void Enter()
        {
            var spin = new SpinWait();
            while (Interlocked.Exchange(ref isLocked, 1) != 0)
                spin.SpinOnce();
        }

        public bool TryEnter()
        {
            return Interlocked.Exchange(ref isLocked, 1) != 0;
        }

        public void Exit()
        {
            Interlocked.Exchange(ref isLocked, 0);
        }

        public void Release() => Exit();
    }
}