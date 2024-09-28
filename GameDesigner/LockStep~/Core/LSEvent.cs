#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using Net.Event;
using SoftFloat;
using System;

namespace LockStep
{
    public class LSEvent
    {
        public static TimerEvent Event;

        public static void Init()
        {
            Event = new TimerEvent();
        }

        public static void Update()
        {
            Event.UpdateEvent();
        }

        /// <summary>
        /// 添加计时器事件, time时间后调用ptr
        /// </summary>
        /// <param name="time">以秒为单位</param>
        /// <param name="ptr"></param>
        /// <param name="isAsync">如果是耗时任务, 需要设置true</param>
        /// <returns>可用于结束事件的id</returns>
        public static int AddEvent(sfloat time, Action ptr, bool isAsync = false)
        {
            return Event.AddEvent(string.Empty, (long)(time * 1000f), ptr, isAsync);
        }

        /// <summary>
        /// 添加计时器事件, time时间后调用ptr
        /// </summary>
        /// <param name="time">以秒为单位</param>
        /// <param name="ptr"></param>
        /// <param name="obj"></param>
        /// <param name="isAsync">如果是耗时任务, 需要设置true</param>
        /// <returns>可用于结束事件的id</returns>
        public static int AddEvent(sfloat time, Action<object> ptr, object obj, bool isAsync = false)
        {
            return Event.AddEvent(string.Empty, (long)(time * 1000f), 0, ptr, obj, isAsync);
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
        public static int AddEvent(sfloat time, int invokeNum, Action<object> ptr, object obj, bool isAsync = false)
        {
            return Event.AddEvent(string.Empty, (long)(time * 1000f), invokeNum, ptr, obj, isAsync);
        }

        /// <summary>
        /// 添加计时器事件, 当time时间到调用ptr, 当ptr返回true则time时间后再次调用ptr, 直到ptr返回false为止
        /// </summary>
        /// <param name="time">以秒为单位</param>
        /// <param name="ptr"></param>
        /// <param name="isAsync">如果是耗时任务, 需要设置true</param>
        /// <returns>可用于结束事件的id</returns>
        public static int AddEvent(sfloat time, Func<bool> ptr, bool isAsync = false)
        {
            return Event.AddEvent(string.Empty, (long)(time * 1000f), ptr, isAsync);
        }

        /// <summary>
        /// 添加计时器事件, 当time时间到调用ptr, 当ptr返回true则time时间后再次调用ptr, 直到ptr返回false为止
        /// </summary>
        /// <param name="name">以秒为单位</param>
        /// <param name="time"></param>
        /// <param name="ptr"></param>
        /// <param name="isAsync">如果是耗时任务, 需要设置true</param>
        /// <returns>可用于结束事件的id</returns>
        public static int AddEvent(string name, sfloat time, Func<bool> ptr, bool isAsync = false)
        {
            return Event.AddEvent(name, (long)(time * 1000f), ptr, isAsync);
        }

        /// <summary>
        /// 添加计时事件, 当time时间到调用ptr, 当ptr返回true则time时间后再次调用ptr, 直到ptr返回false为止
        /// </summary>
        /// <param name="time">以秒为单位</param>
        /// <param name="ptr"></param>
        /// <param name="obj"></param>
        /// <param name="isAsync">如果是耗时任务, 需要设置true</param>
        /// <returns>可用于结束事件的id</returns>
        public static int AddEvent(sfloat time, Func<object, bool> ptr, object obj, bool isAsync = false)
        {
            return Event.AddEvent(string.Empty, (long)(time * 1000f), ptr, obj, isAsync);
        }
    }
}
#endif