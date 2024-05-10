using System;
using System.Runtime.CompilerServices;

namespace Net.Helper
{
    public class DateTimeHelper
    {
        /// <summary>
        /// 获取当前时间到某天的总毫秒数
        /// </summary>
        /// <param name="day"></param>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static int GetNextTime(int day, int hour, int minute, int second)
        {
            var now = DateTime.Now;
            var dayTime = now.AddDays(day);
            dayTime = new DateTime(dayTime.Year, dayTime.Month, dayTime.Day, hour, minute, second);
            var time = (dayTime - now).TotalMilliseconds;
            var seconds = (int)Math.Ceiling(time);
            return seconds;
        }

        /// <summary>
        /// 获取当前时间到下一个工作日的总毫秒数
        /// </summary>
        /// <param name="day"></param>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static int GetNextWeekday(DayOfWeek day, int hour, int minute, int second)
        {
            var now = DateTime.Now;
            var dayTime = GetNextWeekday(now, day);
            dayTime = new DateTime(dayTime.Year, dayTime.Month, dayTime.Day, hour, minute, second);
            var time = (dayTime - now).TotalMilliseconds;
            var seconds = (int)Math.Ceiling(time);
            return seconds;
        }

        private static DateTime GetNextWeekday(DateTime startDate, DayOfWeek day)
        {
            int daysUntilNext = ((int)day - (int)startDate.DayOfWeek + 7) % 7;
            return startDate.AddDays(daysUntilNext);
        }

        /// <summary>
        /// 获取当前系统启动后经过的毫秒数
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long GetTickCount64()
        {
#if CORE
            var timeout = Environment.TickCount64;
#elif UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL //在安卓平台下也必须是这个，否则会报kernel32错误!
            var timeout = Environment.TickCount;
#else
            var timeout = Share.Win32KernelAPI.GetTickCount64();
#endif
            return timeout;

        }
    }
}
