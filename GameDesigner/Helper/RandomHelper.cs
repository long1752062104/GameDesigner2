namespace Net.Share
{
    using global::System;
    using Net.System;

    /// <summary>
    /// 随机帮助类 (多线程安全)
    /// </summary>
    public static class RandomHelper
    {
        private static RandomSafe random = new RandomSafe(Guid.NewGuid().GetHashCode());

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