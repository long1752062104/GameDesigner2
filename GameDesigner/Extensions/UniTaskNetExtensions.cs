using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Net.System;

namespace Cysharp.Threading.Tasks
{
    public class UniTaskNetExtensions
    {
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct EventAwaitable
        {
            private readonly long time;
            private readonly Func<object, bool> update;
            private readonly object state;

            [StructLayout(LayoutKind.Sequential)]
            public readonly struct Awaiter : ICriticalNotifyCompletion, INotifyCompletion
            {
                private readonly EventAwaitable awaitable;
                public bool IsCompleted => false;

                public Awaiter(in EventAwaitable awaitable)
                {
                    this.awaitable = awaitable;
                }

                public void GetResult()
                {
                }

                public void OnCompleted(Action continuation)
                {
                    UnsafeOnCompleted(continuation);
                }

                public void UnsafeOnCompleted(Action continuation)
                {
                    ThreadManager.Event.AddEvent(0f, Run, continuation);
                }

                private bool Run(object state)
                {
                    if (ThreadManager.Event.CurrentTime < awaitable.time)
                        if (!awaitable.update(awaitable.state))
                            return true;
                    ((Action)state)();
                    return false;
                }
            }

            public EventAwaitable(int time, Func<object, bool> update, object state)
            {
                this.time = ThreadManager.Event.CurrentTime + time;
                this.update = update;
                this.state = state;
            }

            public Awaiter GetAwaiter()
            {
                return new Awaiter(this);
            }
        }

        public static EventAwaitable Wait(int time, Func<object, bool> update, object state)
        {
            return new EventAwaitable(time, update, state);
        }
    }
}
