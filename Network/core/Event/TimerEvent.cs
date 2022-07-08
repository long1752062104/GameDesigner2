using Net.System;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Net.Event
{
    /// <summary>
    /// 时间计时器类
    /// </summary>
    public class TimerEvent
    {
        public class Event
        {
            public string name;
            public ulong time;
            public Action<object> ptr1;
            public Func<object, bool> ptr2;
            public object obj;
            public int invokeNum;
            internal ulong timeMax;
            internal int eventId;
            internal bool async;
            internal bool complete = true;
            internal bool isRemove;

            public override string ToString()
            {
                return $"{name}";
            }
        }

        public FastListSafe<Event> events = new FastListSafe<Event>();
        private int eventId = 1000;
        private ulong time;

        /// <summary>
        /// 添加计时器事件, time时间后调用ptr
        /// </summary>
        /// <param name="time"></param>
        /// <param name="ptr"></param>
        /// <param name="isAsync">如果是耗时任务, 需要设置true</param>
        /// <returns>可用于结束事件的id</returns>
        public int AddEvent(float time, Action ptr, bool isAsync = false)
        {
            var enentID = Interlocked.Increment(ref eventId);
            events.Add(new Event()
            {
                time = (ulong)(this.time + (time * 1000)),
                ptr1 = (o) => { ptr(); },
                eventId = enentID,
                async = isAsync,
            });
            return enentID;
        }

        /// <summary>
        /// 添加计时器事件, time时间后调用ptr
        /// </summary>
        /// <param name="time"></param>
        /// <param name="ptr"></param>
        /// <param name="obj"></param>
        /// <param name="isAsync">如果是耗时任务, 需要设置true</param>
        /// <returns>可用于结束事件的id</returns>
        public int AddEvent(float time, Action<object> ptr, object obj, bool isAsync = false)
        {
            var enentID = Interlocked.Increment(ref eventId);
            events.Add(new Event()
            {
                time = (ulong)(this.time + (time * 1000)),
                ptr1 = ptr,
                obj = obj,
                eventId = enentID,
                async = isAsync,
            });
            return enentID;
        }

        /// <summary>
        /// 添加计时器事件, 当time时间到调用ptr, 总共调用invokeNum次数
        /// </summary>
        /// <param name="time"></param>
        /// <param name="invokeNum">调用次数, -1则是无限循环</param>
        /// <param name="ptr"></param>
        /// <param name="obj"></param>
        /// <param name="isAsync">如果是耗时任务, 需要设置true</param>
        /// <returns>可用于结束事件的id</returns>
        public int AddEvent(float time, int invokeNum, Action<object> ptr, object obj, bool isAsync = false)
        {
            var enentID = Interlocked.Increment(ref eventId);
            events.Add(new Event()
            {
                time = (ulong)(this.time + (time * 1000)),
                ptr1 = ptr,
                obj = obj,
                invokeNum = invokeNum,
                timeMax = (ulong)(time * 1000),
                eventId = enentID,
                async = isAsync,
            });
            return enentID;
        }

        /// <summary>
        /// 添加计时器事件, 当time时间到调用ptr, 当ptr返回true则time时间后再次调用ptr, 直到ptr返回false为止
        /// </summary>
        /// <param name="time"></param>
        /// <param name="ptr"></param>
        /// <param name="isAsync">如果是耗时任务, 需要设置true</param>
        /// <returns>可用于结束事件的id</returns>
        public int AddEvent(float time, Func<bool> ptr, bool isAsync = false)
        {
            return AddEvent("", time, ptr, isAsync);
        }

        /// <summary>
        /// 添加计时器事件, 当time时间到调用ptr, 当ptr返回true则time时间后再次调用ptr, 直到ptr返回false为止
        /// </summary>
        /// <param name="name"></param>
        /// <param name="time"></param>
        /// <param name="ptr"></param>
        /// <param name="isAsync">如果是耗时任务, 需要设置true</param>
        /// <returns>可用于结束事件的id</returns>
        public int AddEvent(string name, float time, Func<bool> ptr, bool isAsync = false)
        {
            var enentID = Interlocked.Increment(ref eventId);
            events.Add(new Event()
            {
                name = name,
                time = (ulong)(this.time + (time * 1000)),
                ptr2 = (o) => { return ptr(); },
                eventId = enentID,
                timeMax = (ulong)(time * 1000),
                async = isAsync,
            });
            return enentID;
        }

        /// <summary>
        /// 添加计时器事件, 当time时间到调用ptr, 当ptr返回true则time时间后再次调用ptr, 直到ptr返回false为止
        /// </summary>
        /// <param name="name"></param>
        /// <param name="time"></param>
        /// <param name="ptr"></param>
        /// <param name="isAsync">如果是耗时任务, 需要设置true</param>
        /// <returns>可用于结束事件的id</returns>
        public int AddEvent(string name, int time, Func<bool> ptr, bool isAsync = false)
        {
            var enentID = Interlocked.Increment(ref eventId);
            events.Add(new Event()
            {
                name = name,
                time = this.time + (ulong)time,
                ptr2 = (o) => { return ptr(); },
                eventId = enentID,
                timeMax = (ulong)time,
                async = isAsync,
            });
            return enentID;
        }

        /// <summary>
        /// 添加计时事件, 当time时间到调用ptr, 当ptr返回true则time时间后再次调用ptr, 直到ptr返回false为止
        /// </summary>
        /// <param name="time"></param>
        /// <param name="ptr"></param>
        /// <param name="obj"></param>
        /// <param name="isAsync">如果是耗时任务, 需要设置true</param>
        /// <returns>可用于结束事件的id</returns>
        public int AddEvent(float time, Func<object, bool> ptr, object obj, bool isAsync = false)
        {
            var enentID = Interlocked.Increment(ref eventId);
            events.Add(new Event()
            {
                time = (ulong)(this.time + (time * 1000)),
                ptr2 = ptr,
                obj = obj,
                invokeNum = 1,
                timeMax = (ulong)(time * 1000),
                eventId = enentID,
                async = isAsync,
            });
            return enentID;
        }

        private int frame;
        private DateTime nextTime;
        private readonly Stopwatch stopwatch;

        public TimerEvent() 
        {
            frame = 0;//一秒60次
            nextTime = DateTime.Now.AddSeconds(1);
            stopwatch = Stopwatch.StartNew();
        }

        public void UpdateEventFixed(uint interval = 17, bool sleep = false)//60帧
        {
            var frameRate = 1000 / interval;
            var now = DateTime.Now;
            if (now >= nextTime)
            {
                if (frame < frameRate)
                {
                    var step = (frameRate - frame) * interval;
                    UpdateEvent((uint)step);
                }
                nextTime = DateTime.Now.AddSeconds(1);
                frame = 0;
                stopwatch.Restart();
            }
            else if (frame < frameRate & stopwatch.ElapsedMilliseconds >= frame * interval)
            {
                UpdateEvent(interval);
                frame++;
            }
            else if ((nextTime - now).TotalSeconds > 60d)//当系统时间被改很长后问题
            {
                nextTime = DateTime.Now.AddSeconds(1);
            }
            else if(sleep)
            {
                Thread.Sleep(1);
            }
        }

        public void UpdateEvent(uint interval = 17)//60帧
        {
            time += interval;
            var count = events.Count;
            for (int i = 0; i < count; i++)
            {
                var @event = events._items[i];
                if (@event.isRemove)
                {
                    events.RemoveAt(i);
                    count = events.Count;
                    if (i >= 0) i--;
                    continue;
                }
                if (time >= @event.time)
                {
                    if (@event.ptr1 != null)
                    {
                        if (@event.async)
                            WorkExecute1(@event);
                        else
                            @event.ptr1(@event.obj);
                    }
                    else if (@event.ptr2 != null)
                    {
                        if (@event.async)
                        {
                            WorkExecute2(@event);
                            continue;
                        }
                        if (@event.ptr2(@event.obj))
                            goto J;
                    }
                    if (@event.invokeNum == -1)
                        goto J;
                    if (--@event.invokeNum <= 0)
                    {
                        events.RemoveAt(i);
                        count = events.Count;
                        if (i >= 0) i--;
                        continue;//解决J:执行后索引超出异常
                    }
                J: if (i >= 0 & i < count)
                        @event.time = time + @event.timeMax;
                }
            }
        }

        private async void WorkExecute1(Event @event)
        {
            if (!@event.complete)
                return;
            @event.complete = false;
            await Task.Yield();
            @event.ptr1(@event.obj);
            if (--@event.invokeNum <= 0)
                @event.isRemove = true;
            else
                @event.time = time + @event.timeMax;
            @event.complete = true;
        }

        private async void WorkExecute2(Event @event)
        {
            if (!@event.complete)
                return;
            @event.complete = false;
            await Task.Run(()=>
            {
                if (@event.ptr2(@event.obj))
                    @event.time = time + @event.timeMax;
                else
                    @event.isRemove = true;
                @event.complete = true;
            });
        }

        /// <summary>
        /// 移除事件
        /// </summary>
        /// <param name="eventId"></param>
        public void RemoveEvent(int eventId)
        {
            for (int i = 0; i < events.Count; i++)
            {
                if (events[i].eventId == eventId)
                {
                    events[i].isRemove = true;
                    return;
                }
            }
        }
    }
}