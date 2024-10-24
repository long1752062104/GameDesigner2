using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    public abstract class GetEventBase
    {
        public abstract object Invoke();
    }

    public class GetEvent<T> : GetEventBase
    {
        public Func<T> getFunc;

        public GetEvent(Func<T> getFunc)
        {
            this.getFunc = getFunc;
        }

        public override object Invoke() => getFunc();
    }

    public delegate void EventDelegate(params object[] args);

    public class EventManager : MonoBehaviour
    {
        private readonly Dictionary<object, List<EventDelegate>> events = new Dictionary<object, List<EventDelegate>>();
        private readonly Dictionary<object, GetEventBase> getEvents = new Dictionary<object, GetEventBase>(); //这个事件是你不想在Global定义全局字段时用到，并且无耦合代码

        /// <summary>
        /// 添加事件
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="eventDelegate"></param>
        public void AddEvent(string eventName, EventDelegate eventDelegate)
        {
            if (!events.TryGetValue(eventName, out var delegates))
                events.Add(eventName, delegates = new List<EventDelegate>());
            delegates.Add(eventDelegate);
        }

        /// <summary>
        /// 添加事件
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="eventDelegate"></param>
        public void AddEvent(Enum eventType, EventDelegate eventDelegate)
        {
            if (!events.TryGetValue(eventType, out var delegates))
                events.Add(eventType, delegates = new List<EventDelegate>());
            delegates.Add(eventDelegate);
        }

        /// <summary>
        /// 添加获取事件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="eventType"></param>
        /// <param name="func"></param>
        public void AddGetEvent<T>(Enum eventType, Func<T> func)
        {
            getEvents[eventType] = new GetEvent<T>(func); //获取函数只能有一个
        }

        /// <summary>
        /// 注册的事件在self物体被销毁或关闭时自动移除
        /// </summary>
        /// <param name="self"></param>
        /// <param name="eventType"></param>
        /// <param name="eventDelegate"></param>
        /// <param name="isCall">注册事件完成后调用一次?</param>
        /// <param name="args">调用一次时的参数</param>
        public void AutoEvent(MonoBehaviour self, Enum eventType, EventDelegate eventDelegate, bool isCall = false, params object[] args)
            => AutoEvent(self, UnEventMode.OnDestroy, eventType, eventDelegate, isCall, args);

        /// <summary>
        /// 注册的事件在self物体被销毁或关闭时自动移除
        /// </summary>
        /// <param name="self"></param>
        /// <param name="eventType"></param>
        /// <param name="eventDelegate"></param>
        /// <param name="isCall">注册事件完成后调用一次?</param>
        /// <param name="args">调用一次时的参数</param>
        public void AutoEvent(MonoBehaviour self, UnEventMode eventMode, Enum eventType, EventDelegate eventDelegate, bool isCall, params object[] args)
        {
            AddEvent(eventType, eventDelegate);
            if (!self.TryGetComponent<UnEventTrigger>(out var trigger))
                trigger = self.gameObject.AddComponent<UnEventTrigger>();
            trigger.RegisterUnEvents(eventMode, 1, eventType, eventDelegate);
            if (isCall)
                eventDelegate(args);
        }

        /// <summary>
        /// 注册的事件在self物体被销毁或关闭时自动移除
        /// </summary>
        /// <param name="self"></param>
        /// <param name="eventType"></param>
        /// <param name="func"></param>
        public void AutoGetEvent<T>(MonoBehaviour self, Enum eventType, Func<T> func) => AutoGetEvent(self, UnEventMode.OnDestroy, eventType, func);

        /// <summary>
        /// 注册的事件在self物体被销毁或关闭时自动移除
        /// </summary>
        /// <param name="self"></param>
        /// <param name="eventType"></param>
        /// <param name="func"></param>
        public void AutoGetEvent<T>(MonoBehaviour self, UnEventMode eventMode, Enum eventType, Func<T> func)
        {
            AddGetEvent(eventType, func);
            if (!self.TryGetComponent<UnEventTrigger>(out var trigger))
                trigger = self.gameObject.AddComponent<UnEventTrigger>();
            trigger.RegisterUnEvents(eventMode, 2, eventType, null);
        }

        /// <summary>
        /// 派发事件
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="pars"></param>
        public void Dispatch(string eventName, params object[] pars)
        {
            if (events.TryGetValue(eventName, out var delegates))
            {
                foreach (var item in delegates)
                    item.Invoke(pars);
            }
        }

        /// <summary>
        /// 派发事件
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="pars"></param>
        public void Dispatch(Enum eventType, params object[] pars)
        {
            if (events.TryGetValue(eventType, out var delegates))
            {
                foreach (var item in delegates)
                    item.Invoke(pars);
            }
        }

        /// <summary>
        /// 获取事件值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="eventType"></param>
        /// <returns></returns>
        public T Get<T>(Enum eventType)
        {
            if (getEvents.TryGetValue(eventType, out var getEvent))
                return (T)getEvent.Invoke();
            return default;
        }

        /// <summary>
        /// 移除事件
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="eventDelegate"></param>
        public void Remove(string eventName, EventDelegate eventDelegate)
        {
            if (events.TryGetValue(eventName, out var delegates))
            {
                foreach (var item in delegates)
                {
                    if (item.Equals(eventDelegate))
                    {
                        delegates.Remove(item);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 移除事件
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="eventDelegate"></param>
        public void Remove(Enum eventType, EventDelegate eventDelegate)
        {
            if (events.TryGetValue(eventType, out var delegates))
            {
                foreach (var item in delegates)
                {
                    if (item.Equals(eventDelegate))
                    {
                        delegates.Remove(item);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 移除获取事件
        /// </summary>
        /// <param name="eventType"></param>
        public void RemoveGet(Enum eventType)
        {
            getEvents.Remove(eventType);
        }
    }
}