using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Net.Helper
{
    public static class StringHelper
    {
        /// <summary>
        /// 检查sql的字符串类型的值合法性
        /// </summary>
        /// <param name="value"></param>
        /// <param name="length"></param>
        public static void CheckSqlString(ref string value, int length)
        {
            value = value.Replace("\\", "\\\\"); //如果包含\必须转义, 否则出现 \'时就会出错
            value = value.Replace("'", "\\\'"); //出现'时必须转转义成\'
            if (value.Length >= length)
                value = value.Substring(0, length);
        }

        /// <summary>
        /// 查找一个字符在text出现了几次
        /// </summary>
        /// <param name="text"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int FindHitCount(string text, char value)
        {
            int count = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == value)
                    count++;
            }
            return count;
        }

        /// <summary>
        /// 一个字符在text出现的第hitcount次后被移除
        /// </summary>
        /// <param name="text"></param>
        /// <param name="value"></param>
        /// <param name="hitCount"></param>
        public static void RemoveHit(ref string text, char value, int hitCount)
        {
            int count = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == value)
                {
                    if (count == hitCount)
                    {
                        text = text.Remove(i);
                        break;
                    }
                    count++;
                }
            }
        }

        public static int IndexOf(List<char> chars, string text)
        {
            for (int i = 0; i < chars.Count; i++)
            {
                if (chars[i] == text[0])
                {
                    int index = i + 1;
                    int index1 = 1;
                    while (index < chars.Count & index1 < text.Length)
                    {
                        if (chars[index] != text[index1])
                            goto J;
                    }
                    return i;
                }
            J:;
            }
            return -1;
        }

        public static void Remove(List<char> chars, int index, int count)
        {
            chars.RemoveRange(index, count);
        }

        public static string ToString(List<char> chars)
        {
            var stringBuilder = new StringBuilder();
            for (int i = 0; i < chars.Count; i++)
                stringBuilder.Append(chars[i]);
            return stringBuilder.ToString();
        }

        public static List<char> Substring(List<char> chars, int index, int count)
        {
            return chars.GetRange(index, count);
        }

        public static string ReplaceClear(this string self, params string[] pars)
        {
            foreach (var item in pars)
                self = self.Replace(item, "");
            return self;
        }

        public static bool StartsWith(this string self, string text)
        {
            if (self.Length < text.Length)
                return false;
            for (int i = 0; i < text.Length; i++)
                if (self[i] != text[i])
                    return false;
            return true;
        }

        public static byte[] ToBytes(this string self)
        {
            return Encoding.UTF8.GetBytes(self);
        }

        public static string ToText(this byte[] self)
        {
            return Encoding.UTF8.GetString(self);
        }

        /// <summary>
        /// 输出金币字符串格式
        /// </summary>
        /// <param name="self"></param>
        /// <param name="format">格式默认是英文e, 如果输出中文则是c</param>
        /// <returns></returns>
        public static string ToStringFormat(this int self, string format = "e")
        {
            return ToStringFormat((long)self, format);
        }

        /// <summary>
        /// 输出金币字符串格式
        /// </summary>
        /// <param name="self"></param>
        /// <param name="format">格式默认是英文e, 如果输出中文则是c</param>
        /// <returns></returns>
        public static string ToStringFormat(this uint self, string format = "e")
        {
            return ToStringFormat((long)self, format);
        }

        /// <summary>
        /// 输出金币字符串格式
        /// </summary>
        /// <param name="self"></param>
        /// <param name="format">格式默认是英文e, 如果输出中文则是c</param>
        /// <returns></returns>
        public static string ToStringFormat(this long self, string format = "e")
        {
            var mark = self < 0 ? "-" : string.Empty;
            double value = Math.Abs(self);
            if (format == "e")
            {
                var units = new string[] { "B", "K", "M", "G", "T", "P" };
                double mod = 1000;
                int i = 0;
                while (value >= mod)
                {
                    value /= mod;
                    i++;
                }
                return $"{mark}{Math.Round(value, 2)}{units[i]}";
            }
            else
            {
                var thresholds = new double[] { 1000, 10000, 100000000, 1000000000000, 10000000000000000, (double)100000000000000000000M };
                var units = new string[] { "", "千", "万", "亿", "兆", "京", "稊" };
                for (int i = 0; i < thresholds.Length; i++)
                    if (value < thresholds[i])
                        return $"{mark}{(i == 0 ? value.ToString("f0") : (value / thresholds[i - 1]).ToString("0.##"))}{units[i]}";
                return $"{mark}{(value / thresholds[thresholds.Length - 1]).ToString("0.##")}{units[units.Length - 1]}";
            }
        }
    }
}