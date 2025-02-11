﻿using System;
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
        /// <returns>总毫秒数</returns>
        public static long GetNextTime(int day, int hour, int minute, int second)
        {
            var now = DateTime.Now;
            var dayTime = now.AddDays(day);
            dayTime = new DateTime(dayTime.Year, dayTime.Month, dayTime.Day, hour, minute, second);
            var time = (dayTime - now).TotalMilliseconds;
            var seconds = (long)Math.Ceiling(time);
            return seconds;
        }

        /// <summary>
        /// 获取当前时间到dateTime时间的总毫秒数
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns>总毫秒数</returns>
        public static long GetNextTime(DateTime dateTime)
        {
            var now = DateTime.Now;
            var time = (dateTime - now).TotalMilliseconds;
            var seconds = (long)Math.Ceiling(time); //用int，30天的倒计时会出问题
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

        /// <summary>
        /// 打印时分秒，当不满足时时只显示分和秒，当不满时和分时，只显示秒
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string FormatDateTime(DateTime dateTime)
        {
            if (dateTime.Hour > 0)
            {
                return dateTime.ToString("H时m分s秒");
            }
            else if (dateTime.Minute > 0)
            {
                return dateTime.ToString("m分s秒");
            }
            else
            {
                return dateTime.ToString("s秒");
            }
        }

        /// <summary>
        /// 打印时分秒，当不满足时时只显示分和秒，当不满时和分时，只显示秒
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string FormatTimeSpan(TimeSpan timeSpan)
        {
            if (timeSpan.Hours > 0)
            {
                return $"{timeSpan.Hours}时{timeSpan.Minutes}分{timeSpan.Seconds}秒";
            }
            else if (timeSpan.Minutes > 0)
            {
                return $"{timeSpan.Minutes}分{timeSpan.Seconds}秒";
            }
            else
            {
                return $"{timeSpan.Seconds}秒";
            }
        }
    }
}
