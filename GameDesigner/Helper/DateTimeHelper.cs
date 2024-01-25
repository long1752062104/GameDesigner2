using System;

namespace Net.Helper
{
    public class DateTimeHelper
    {
        /// <summary>
        /// 获取当前时间到某天的时间戳
        /// </summary>
        /// <param name="day"></param>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <param name="second"></param>
        /// <returns>当前时间到某天的时间戳</returns>
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
        /// 获取当前时间到下一个工作日的时间戳
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
    }
}
