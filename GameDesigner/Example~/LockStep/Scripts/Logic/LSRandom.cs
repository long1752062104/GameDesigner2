#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using Net.System;

namespace LockStep.Client
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
        public static float Range(float minValue, float maxValue)
        {
            return random.Range(minValue, maxValue);
        }
    }
}
#endif