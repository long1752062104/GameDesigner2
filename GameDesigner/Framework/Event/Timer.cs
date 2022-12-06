using System;
using System.Collections.Generic;

namespace GF.Event
{
    public class Timer
    {
        private readonly Dictionary<string, int> timerDict = new Dictionary<string, int>();

        /// <summary>
        /// 检查时间是否到达, 如果时间到达,则重新计算下一次的时间并返回true
        /// </summary>
        /// <param name="name"></param>
        /// <param name="millisecondsTimeout"></param>
        /// <param name="firstValue">如果此时间名是第一次调用, 需要返回的是true或者false</param>
        /// <returns></returns>
        public bool IsTimeOut(string name, int millisecondsTimeout, bool firstValue = false)
        {
            if (!timerDict.TryGetValue(name, out var tick))
            {
                timerDict.Add(name, Environment.TickCount + millisecondsTimeout);
                return firstValue;
            }
            if(Environment.TickCount >= tick)
            {
                timerDict[name] = Environment.TickCount + millisecondsTimeout;
                return true;
            }
            return false;
        }

        public void RemoveTime(string name)
        {
            timerDict.Remove(name);
        }
    }
}
