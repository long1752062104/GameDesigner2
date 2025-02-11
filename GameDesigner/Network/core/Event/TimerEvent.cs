﻿using Net.System;
using System;
using System.Threading;
#if !UNITY_WEBGL
using System.Threading.Tasks;
#endif

namespace Net.Event
{
    /// <summary>
    /// 时间计时器类
    /// </summary>
    public class TimerEvent
    {
        public abstract class EventAction : IDisposable
        {
            public bool IsAsync;
            public bool IsCompleted;

            public abstract void Invoke();
            public abstract void WorkExecute(object args);
            public abstract void Dispose();
        }

        public class EventAction1 : EventAction
        {
            public Event Event;
            public Action action;

            public override void Invoke()
            {
#if !UNITY_WEBGL
                if (IsAsync)
                {
                    if (!IsCompleted)
                        return;
                    IsCompleted = false;
                    ThreadPool.UnsafeQueueUserWorkItem(WorkExecute, null);
                }
                else
#endif
                    action();
                if (Event.invokeNum == -1)
                    Event.time += Event.timeMax;
                else if (--Event.invokeNum <= 0)
                    Event.isRemove = true;
            }

            public override void WorkExecute(object args)
            {
                try { action(); } catch (Exception ex) { NDebug.LogError(ex); }
                if (--Event.invokeNum <= 0)
                    Event.isRemove = true;
                else
                    Event.time += Event.timeMax;
                IsCompleted = true;
            }

            public override void Dispose()
            {
                action = null;
            }
        }

        public class EventAction2<T> : EventAction
        {
            public Event<T> Event;
            public Action<T> action;

            public override void Invoke()
            {
#if !UNITY_WEBGL
                if (IsAsync)
                {
                    if (!IsCompleted)
                        return;
                    IsCompleted = false;
                    ThreadPool.UnsafeQueueUserWorkItem(WorkExecute, null);
                }
                else
#endif
                    action(Event.obj);
                if (Event.invokeNum == -1)
                    Event.time += Event.timeMax;
                else if (--Event.invokeNum <= 0)
                    Event.isRemove = true;
            }

            public override void WorkExecute(object args)
            {
                try { action(Event.obj); } catch (Exception ex) { NDebug.LogError(ex); }
                if (--Event.invokeNum <= 0)
                    Event.isRemove = true;
                else
                    Event.time += Event.timeMax;
                IsCompleted = true;
            }

            public override void Dispose()
            {
                action = null;
            }
        }

        public class EventAction3 : EventAction
        {
            public Event Event;
            public Func<bool> action;

            public override void Invoke()
            {
#if !UNITY_WEBGL
                if (IsAsync)
                {
                    if (!IsCompleted)
                        return;
                    IsCompleted = false;
                    ThreadPool.UnsafeQueueUserWorkItem(WorkExecute, null);
                }
                else
#endif
                if (action())
                    Event.time += Event.timeMax;
                else
                    Event.isRemove = true;
            }

            public override void WorkExecute(object args)
            {
                try
                {
                    if (action())
                        Event.time += Event.timeMax;
                    else
                        Event.isRemove = true;
                }
                catch (Exception ex)
                {
                    NDebug.LogError(ex);
                }
                finally
                {
                    IsCompleted = true;
                }
            }

            public override void Dispose()
            {
                action = null;
            }
        }

        public class EventAction4<T> : EventAction
        {
            public Event<T> Event;
            public Func<T, bool> action;

            public override void Invoke()
            {
#if !UNITY_WEBGL
                if (IsAsync)
                {
                    if (!IsCompleted)
                        return;
                    IsCompleted = false;
                    ThreadPool.UnsafeQueueUserWorkItem(WorkExecute, null);
                }
                else
#endif
                if (action(Event.obj))
                    Event.time += Event.timeMax;
                else
                    Event.isRemove = true;
            }

            public override void WorkExecute(object args)
            {
                try
                {
                    if (action(Event.obj))
                        Event.time += Event.timeMax;
                    else
                        Event.isRemove = true;
                }
                catch (Exception ex)
                {
                    NDebug.LogError(ex);
                }
                finally
                {
                    IsCompleted = true;
                }
            }

            public override void Dispose()
            {
                action = null;
            }
        }

        public class Event
        {
            public string name;
            public long time;
            public EventAction action;
            public int invokeNum;
            public long timeMax;
            public int eventId;
            internal bool isRemove;
            public void SetIntervalTime(uint value)
            {
                timeMax = value;
            }
            public override string ToString()
            {
                return $"{name}";
            }
        }
        public class Event<T> : Event
        {
            public T obj;
        }
        public FastListSafe<Event> events = new FastListSafe<Event>();
        private int eId = 1000;
        private long time;
        private uint frame;
        private uint startTick;
        private uint nextTick;

        public long CurrentTime => time;

        public TimerEvent()
        {
            frame = 0u;//一秒60次
            startTick = (uint)Environment.TickCount;
            nextTick = startTick + 1000u;
        }

        /// <summary>
        /// 添加计时器事件, time时间后调用ptr
        /// </summary>
        /// <param name="time">以秒为单位</param>
        /// <param name="ptr"></param>
        /// <param name="isAsync">如果是耗时任务, 需要设置true</param>
        /// <returns>可用于结束事件的id</returns>
        public int AddEvent(float time, Action ptr, bool isAsync = false)
        {
            return AddEvent(string.Empty, (long)(time * 1000f), ptr, isAsync);
        }

        /// <summary>
        /// 添加计时器事件, time时间后调用ptr
        /// </summary>
        /// <param name="time">以秒为单位</param>
        /// <param name="ptr"></param>
        /// <param name="obj"></param>
        /// <param name="isAsync">如果是耗时任务, 需要设置true</param>
        /// <returns>可用于结束事件的id</returns>
        public int AddEvent<T>(float time, Action<T> ptr, T obj, bool isAsync = false)
        {
            return AddEvent(string.Empty, (long)(time * 1000f), 0, ptr, obj, isAsync);
        }

        /// <summary>
        /// 添加计时器事件, 当time时间到调用ptr, 总共调用invokeNum次数
        /// </summary>
        /// <param name="time">以秒为单位</param>
        /// <param name="invokeNum">调用次数, -1则是无限循环</param>
        /// <param name="ptr"></param>
        /// <param name="obj"></param>
        /// <param name="isAsync">如果是耗时任务, 需要设置true</param>
        /// <returns>可用于结束事件的id</returns>
        public int AddEvent<T>(float time, int invokeNum, Action<T> ptr, T obj, bool isAsync = false)
        {
            return AddEvent(string.Empty, (long)(time * 1000f), invokeNum, ptr, obj, isAsync);
        }

        /// <summary>
        /// 添加计时器事件, 当time时间到调用ptr, 当ptr返回true则time时间后再次调用ptr, 直到ptr返回false为止
        /// </summary>
        /// <param name="time">以秒为单位</param>
        /// <param name="ptr"></param>
        /// <param name="isAsync">如果是耗时任务, 需要设置true</param>
        /// <returns>可用于结束事件的id</returns>
        public int AddEvent(float time, Func<bool> ptr, bool isAsync = false)
        {
            return AddEvent(string.Empty, (long)(time * 1000f), ptr, isAsync);
        }

        /// <summary>
        /// 添加计时器事件, 当time时间到调用ptr, 当ptr返回true则time时间后再次调用ptr, 直到ptr返回false为止
        /// </summary>
        /// <param name="name">以秒为单位</param>
        /// <param name="time"></param>
        /// <param name="ptr"></param>
        /// <param name="isAsync">如果是耗时任务, 需要设置true</param>
        /// <returns>可用于结束事件的id</returns>
        public int AddEvent(string name, float time, Func<bool> ptr, bool isAsync = false)
        {
            return AddEvent(name, (long)(time * 1000f), ptr, isAsync);
        }

        /// <summary>
        /// 添加计时事件, 当time时间到调用ptr, 当ptr返回true则time时间后再次调用ptr, 直到ptr返回false为止
        /// </summary>
        /// <param name="time">以秒为单位</param>
        /// <param name="ptr"></param>
        /// <param name="obj"></param>
        /// <param name="isAsync">如果是耗时任务, 需要设置true</param>
        /// <returns>可用于结束事件的id</returns>
        public int AddEvent<T>(float time, Func<T, bool> ptr, T obj, bool isAsync = false)
        {
            return AddEvent(string.Empty, (long)(time * 1000f), ptr, obj, isAsync);
        }

        public int AddEvent(string name, long time, Action ptr, bool isAsync = false)
        {
            var eventID = Interlocked.Increment(ref eId);
            var eventObj = new Event()
            {
                name = name,
                time = this.time + time,
                eventId = eventID,
                timeMax = time,
            };
            eventObj.action = new EventAction1()
            {
                Event = eventObj,
                IsAsync = isAsync,
                action = ptr,
                IsCompleted = true,
            };
            events.Add(eventObj);
            return eventID;
        }

        public int AddEvent<T>(string name, long time, int invokeNum, Action<T> ptr, T obj, bool isAsync = false)
        {
            var eventID = Interlocked.Increment(ref eId);
            var eventObj = new Event<T>()
            {
                name = name,
                time = this.time + time,
                eventId = eventID,
                timeMax = time,
                obj = obj,
                invokeNum = invokeNum,
            };
            eventObj.action = new EventAction2<T>()
            {
                Event = eventObj,
                IsAsync = isAsync,
                action = ptr,
                IsCompleted = true,
            };
            events.Add(eventObj);
            return eventID;
        }

        public int AddEvent<T>(string name, long time, Func<T, bool> ptr, T obj, bool isAsync = false)
        {
            var eventID = Interlocked.Increment(ref eId);
            var eventObj = new Event<T>()
            {
                name = name,
                time = this.time + time,
                eventId = eventID,
                timeMax = time,
                obj = obj,
            };
            eventObj.action = new EventAction4<T>()
            {
                Event = eventObj,
                IsAsync = isAsync,
                action = ptr,
                IsCompleted = true,
            };
            events.Add(eventObj);
            return eventID;
        }

        /// <summary>
        /// 添加计时器事件, 当time时间到调用ptr, 当ptr返回true则time时间后再次调用ptr, 直到ptr返回false为止
        /// </summary>
        /// <param name="name"></param>
        /// <param name="time">毫秒为单位</param>
        /// <param name="ptr"></param>
        /// <param name="isAsync">如果是耗时任务, 需要设置true</param>
        /// <returns>可用于结束事件的id</returns>
        public int AddEvent(string name, long time, Func<bool> ptr, bool isAsync = false)
        {
            var eventID = Interlocked.Increment(ref eId);
            var eventObj = new Event()
            {
                name = name,
                time = this.time + time,
                eventId = eventID,
                timeMax = time,
            };
            eventObj.action = new EventAction3()
            {
                Event = eventObj,
                IsAsync = isAsync,
                action = ptr,
                IsCompleted = true,
            };
            events.Add(eventObj);
            return eventID;
        }

        public void UpdateEventFixed(uint interval = 17, bool sleep = false)//60帧
        {
            UpdateEventFixed((uint)Environment.TickCount, interval, sleep);
        }

        public void UpdateEventFixed(uint tick, uint interval, bool sleep = false)//60帧
        {
            var frameRate = 1000u / interval;
            if (tick >= nextTick)
            {
                if (frame < frameRate)
                {
                    var step = (frameRate - frame) * interval;
                    UpdateEvent(step);
                }
                frame = 0u;
                startTick = tick;
                nextTick = tick + 1000u;
                if (startTick >= nextTick)
                    nextTick = uint.MaxValue;
            }
            else if (frame < frameRate & (tick - startTick) >= frame * interval)
            {
                UpdateEvent(interval);
                frame++;
            }
            else
            {
                UpdateEvent(0); //需要执行那些没有延迟的事件
                if (sleep) Thread.Sleep(1);
                var total = nextTick - tick;
                if (total >= 2000u) //当uint.MaxValue和1差距很大时, 会出现计数不命中的问题, 导致bug
                    nextTick = tick;
            }
        }

        public void UpdateEvent(uint interval = 17)//60帧
        {
            time += interval;
            var count = events.Count;
            for (int i = 0; i < count; i++)
            {
                try
                {
                    var _event = events._items[i];
                    if (_event == null) //当unity关闭客户端也会导致这里null
                        continue;
                    if (_event.isRemove)
                    {
                        _event.action.Dispose();
                        events.RemoveAt(i);
                        count = events.Count;
                        if (i >= 0) i--;
                        continue;
                    }
                    if (time < _event.time)
                        continue;
                    _event.action.Invoke();
                }
                catch (Exception ex)
                {
                    NDebug.LogError("计时器异常:" + ex);
                }
            }
        }

        /// <summary>
        /// 获取计时事件
        /// </summary>
        /// <param name="eventId"></param>
        public Event GetEvent(int eventId)
        {
            for (int i = 0; i < events.Count; i++)
            {
                var @event = events[i];
                if (@event == null)
                    continue;
                if (@event.eventId == eventId)
                {
                    return @event;
                }
            }
            return null;
        }

        /// <summary>
        /// 移除事件
        /// </summary>
        /// <param name="eventId"></param>
        public void RemoveEvent(int eventId)
        {
            for (int i = 0; i < events.Count; i++)
            {
                var @event = events[i];
                if (@event == null)
                    continue;
                if (@event.eventId == eventId)
                {
                    @event.isRemove = true;
                    return;
                }
            }
        }

        /// <summary>
        /// 重置计时器事件间隔
        /// </summary>
        /// <param name="eventID"></param>
        /// <param name="interval"></param>
        /// <param name="restartTiming">是否废弃以前的计时, 从当前重新开始新的计时</param>
        public void ResetTimeInterval(int eventID, long interval, bool restartTiming = false)
        {
            var evt = GetEvent(eventID);
            if (evt != null)
            {
                if (restartTiming)
                    evt.time = time + interval;
                evt.timeMax = interval;
            }
        }

        /// <summary>
        /// 重置事件
        /// </summary>
        public void ResetEvents()
        {
            for (int i = 0; i < events.Count; i++)
            {
                var @event = events[i];
                if (@event == null)
                    continue;
                @event.action.IsCompleted = true;
            }
        }

        /// <summary>
        /// 清除事件列表
        /// </summary>
        public void Clear()
        {
            events.Clear();
        }
    }

    public class TimerTick
    {
        private uint frame;
        private uint startTick;
        private uint nextTick;

        public TimerTick()
        {
            frame = 0;//一秒60次
            startTick = (uint)Environment.TickCount;
            nextTick = startTick + 1000u;
        }

        /// <summary>
        /// 检测时间是否到达
        /// </summary>
        /// <param name="tick"></param>
        /// <param name="interval">间隔最大是1000</param>
        /// <param name="sleep"></param>
        /// <returns></returns>
        public bool CheckTimeout(uint tick, uint interval, bool sleep = false)//60帧
        {
            var frameRate = 1000u / interval;
            if (tick >= nextTick)
            {
                frame = 0u;
                startTick = tick;
                nextTick = tick + 1000u;
                if (startTick >= nextTick)
                    nextTick = uint.MaxValue;
                return false; //这里返回true会调用两次
            }
            else if (frame <= frameRate & (tick - startTick) >= frame * interval)
            {
                frame++;
                return true;
            }
            else
            {
                if (sleep) Thread.Sleep(1);
                var total = nextTick - tick;
                if (total >= 2000u) //当uint.MaxValue和1差距很大时, 会出现计数不命中的问题, 导致bug
                    nextTick = tick;
            }
            return false;
        }
    }
}