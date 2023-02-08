using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Net.Helper
{
    public class StringHelper
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
    }
}