using System;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Net.Event;
using Net.Share;
using Net.System;
using Net.Helper;

namespace Cysharp.Threading.Tasks
{
    public class UniTaskNetExtensions
    {
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct EventAwaitable
        {
            private readonly TimerEvent timerEvent;
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
                    awaitable.timerEvent.AddEvent(0f, Run, continuation);
                }

                private bool Run(object state)
                {
                    if (awaitable.timerEvent.CurrentTime < awaitable.time)
                        if (!awaitable.update(awaitable.state))
                            return true;
                    ((Action)state)();
                    return false;
                }
            }

            public EventAwaitable(TimerEvent timerEvent, int time, Func<object, bool> update, object state)
            {
                this.timerEvent = timerEvent;
                this.time = timerEvent.CurrentTime + time;
                this.update = update;
                this.state = state;
            }

            public Awaiter GetAwaiter()
            {
                return new Awaiter(this);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public readonly struct CallbackAwaitable
        {
            private readonly TimerEvent timerEvent;
            private readonly long time;
            private readonly RPCModelTask state;

            [StructLayout(LayoutKind.Sequential)]
            public readonly struct Awaiter : ICriticalNotifyCompletion, INotifyCompletion
            {
                private readonly CallbackAwaitable awaitable;
                public bool IsCompleted => false;

                public Awaiter(in CallbackAwaitable awaitable)
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
                    awaitable.state.callback = continuation;
                    awaitable.timerEvent.AddEvent(0f, Run);
                }

                private bool Run()
                {
                    if (awaitable.timerEvent.CurrentTime < awaitable.time)
                        if (!awaitable.state.IsCompleted)
                            return true;
                    var original = Interlocked.Exchange(ref awaitable.state.callback, null);
                    original?.Invoke();
                    return false;
                }
            }

            public CallbackAwaitable(TimerEvent timerEvent, int time, RPCModelTask state)
            {
                this.timerEvent = timerEvent;
                this.time = timerEvent.CurrentTime + time;
                this.state = state;
            }

            public Awaiter GetAwaiter()
            {
                return new Awaiter(this);
            }
        }

        public struct SwitchToMainThreadAwaitable
        {
            readonly JobQueueHelper queue;

            public SwitchToMainThreadAwaitable(JobQueueHelper queue)
            {
                this.queue = queue;
            }

            public Awaiter GetAwaiter() => new Awaiter(queue);

            public struct Awaiter : ICriticalNotifyCompletion
            {
                readonly JobQueueHelper queue;

                public Awaiter(JobQueueHelper queue)
                {
                    this.queue = queue;
                }

                public bool IsCompleted
                {
#if !SERVICE
                    get => PlayerLoopHelper.MainThreadId == Thread.CurrentThread.ManagedThreadId;
#else
                    get => false;
#endif
                }

                public void GetResult() { }

                public void OnCompleted(Action continuation)
                {
                    UnsafeOnCompleted(continuation);
                }

                public void UnsafeOnCompleted(Action continuation)
                {
                    queue.Call(continuation);
                }
            }
        }

        public static EventAwaitable Wait(int time, Func<object, bool> update, object state) => Wait(ThreadManager.Event, time, update, state);

        public static EventAwaitable Wait(TimerEvent timerEvent, int time, Func<object, bool> update, object state)
        {
            return new EventAwaitable(timerEvent, time, update, state);
        }

        public static CallbackAwaitable WaitCallback(TimerEvent timerEvent, int time, RPCModelTask modelTask)
        {
            return new CallbackAwaitable(timerEvent, time, modelTask);
        }

        public static SwitchToMainThreadAwaitable SwitchToMainThread(JobQueueHelper queue)
        {
            return new SwitchToMainThreadAwaitable(queue);
        }
    }
}
