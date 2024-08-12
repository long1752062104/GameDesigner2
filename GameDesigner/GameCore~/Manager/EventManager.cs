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

    public class EventManager : MonoBehaviour
    {
        private readonly Dictionary<object, List<Action<object[]>>> events = new Dictionary<object, List<Action<object[]>>>();
        private readonly Dictionary<object, GetEventBase> getEvents = new Dictionary<object, GetEventBase>(); //这个事件是你不想在Global定义全局字段时用到，并且无耦合代码

        /// <summary>
        /// 添加事件, 以方法名为事件名称
        /// </summary>
        /// <param name="eventDelegate"></param>
        public void AddEvent(Action<object[]> eventDelegate)
        {
            var eventName = eventDelegate.Method.Name;
            AddEvent(eventName, eventDelegate);
        }

        /// <summary>
        /// 添加事件
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="eventDelegate"></param>
        public void AddEvent(string eventName, Action<object[]> eventDelegate)
        {
            if (!events.TryGetValue(eventName, out var delegates))
                events.Add(eventName, delegates = new List<Action<object[]>>());
            delegates.Add(eventDelegate);
        }

        /// <summary>
        /// 添加事件
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="eventDelegate"></param>
        public void AddEvent(Enum eventType, Action<object[]> eventDelegate)
        {
            if (!events.TryGetValue(eventType, out var delegates))
                events.Add(eventType, delegates = new List<Action<object[]>>());
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
        /// 移除事件, 以方法名为事件名查找并移除
        /// </summary>
        /// <param name="eventDelegate"></param>
        public void Remove(Action<object[]> eventDelegate)
        {
            var eventName = eventDelegate.Method.Name;
            Remove(eventName, eventDelegate);
        }

        /// <summary>
        /// 移除事件
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="eventDelegate"></param>
        public void Remove(string eventName, Action<object[]> eventDelegate)
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
        public void Remove(Enum eventType, Action<object[]> eventDelegate)
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