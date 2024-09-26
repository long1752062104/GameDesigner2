#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using Net.System;
using SoftFloat;

namespace LockStep
{
    public class LSRandom
    {
        private static RandomSafe random = new RandomSafe(1752062104);

        /// <summary>
        /// 初始化随机种子
        /// </summary>
        /// <param name="Seed"></param>
        public static void InitSeed(int Seed)
        {
            random = new RandomSafe(Seed);
        }

        /// <summary>
        /// 随机范围
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public static int Range(int minValue, int maxValue)
        {
            return random.Range(minValue, maxValue);
        }

        /// <summary>
        /// 随机范围
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public static sfloat Range(sfloat minValue, sfloat maxValue)
        {
            if (minValue > maxValue)
            {
                sfloat maxValue1 = maxValue;
                sfloat minValue1 = minValue;
                minValue = maxValue1;
                maxValue = minValue1;
            }
            int value1 = (int)(minValue * 10000);
            int value2 = (int)(maxValue * 10000);
            sfloat value3 = random.Next(value1, value2) * (sfloat)0.0001f;
            if (value3 < minValue | value3 > maxValue)
                return Range(minValue, maxValue);
            return value3;
        }
    }
}
#endif