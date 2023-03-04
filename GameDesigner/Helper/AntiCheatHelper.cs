using System;

namespace Net.Helper
{
    public class AntiCheatHelper
    {
        /// <summary>
        /// 当检测到作弊, 参数1: 哪个属性被修改
        /// </summary>
        public static Action<string> OnDetected { get; set; }
    }
}