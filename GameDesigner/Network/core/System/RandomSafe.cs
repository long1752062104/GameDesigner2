using System;
using System.Runtime.InteropServices;
#if LOCK_STEP
using FLOAT = SoftFloat.sfloat;
#else
using FLOAT = System.Single;
#endif


namespace Net.System
{
    /// <summary>
    /// 随机类 (多线程安全)
    /// </summary>
    [Serializable]
    [ComVisible(true)]
    public class RandomSafe
    {
        private int inext;
        private int inextp;
        private readonly int[] SeedArray = new int[56];

        public RandomSafe() : this(Environment.TickCount)
        {
        }

        public RandomSafe(int Seed)
        {
            int num = (Seed == int.MinValue) ? int.MaxValue : Math.Abs(Seed);
            int num2 = 161803398 - num;
            SeedArray[55] = num2;
            int num3 = 1;
            for (int i = 1; i < 55; i++)
            {
                int num4 = 21 * i % 55;
                SeedArray[num4] = num3;
                num3 = num2 - num3;
                if (num3 < 0)
                {
                    num3 += int.MaxValue;
                }

                num2 = SeedArray[num4];
            }

            for (int j = 1; j < 5; j++)
            {
                for (int k = 1; k < 56; k++)
                {
                    SeedArray[k] -= SeedArray[1 + (k + 30) % 55];
                    if (SeedArray[k] < 0)
                    {
                        SeedArray[k] += int.MaxValue;
                    }
                }
            }

            inext = 0;
            inextp = 21;
        }


        protected virtual FLOAT Sample()
        {
            return InternalSample() * (FLOAT)4.6566128752457969E-10;
        }

        private int InternalSample()
        {
            lock (this)
            {
                int num = inext;
                int num2 = inextp;
                if (++num >= 56)
                {
                    num = 1;
                }

                if (++num2 >= 56)
                {
                    num2 = 1;
                }

                int num3 = SeedArray[num] - SeedArray[num2];
                if (num3 == int.MaxValue)
                {
                    num3--;
                }

                if (num3 < 0)
                {
                    num3 += int.MaxValue;
                }

                SeedArray[num] = num3;
                inext = num;
                inextp = num2;
                return num3;
            }
        }

        public virtual int Next()
        {
            return InternalSample();
        }

        private FLOAT GetSampleForLargeRange()
        {
            int num = InternalSample();
            if (InternalSample() % 2 == 0)
            {
                num = -num;
            }

            FLOAT num2 = num;
            num2 += (FLOAT)2147483646.0f;
            return num2 / (FLOAT)4294967293.0f;
        }

        public virtual int Next(int minValue, int maxValue)
        {
            if (minValue > maxValue)
            {
                throw new ArgumentOutOfRangeException("minValue", "");
            }

            long num = maxValue - (long)minValue;
            if (num <= int.MaxValue)
            {
                return (int)(Sample() * num) + minValue;
            }

            return (int)((long)(GetSampleForLargeRange() * num) + minValue);
        }

        public virtual int Next(int maxValue)
        {
            if (maxValue < 0)
            {
                throw new ArgumentOutOfRangeException("maxValue", "");
            }

            return (int)(Sample() * maxValue);
        }

        public virtual FLOAT NextDouble()
        {
            return Sample();
        }

        public virtual void NextBytes(byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (byte)(InternalSample() % 256);
            }
        }

        /// <summary>
        /// 随机范围
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public int Range(int minValue, int maxValue)
        {
            if (minValue > maxValue)
            {
                return Next(maxValue, minValue);
            }
            return Next(minValue, maxValue);
        }

        /// <summary>
        /// 随机范围
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public float Range(float minValue, float maxValue)
        {
            if (minValue > maxValue)
            {
                float maxValue1 = maxValue;
                float minValue1 = minValue;
                minValue = maxValue1;
                maxValue = minValue1;
            }
            int value1 = (int)(minValue * 10000);
            int value2 = (int)(maxValue * 10000);
            float value3 = Next(value1, value2) * (float)0.0001f;
            if (value3 < minValue | value3 > maxValue)
                return Range(minValue, maxValue);
            return value3;
        }
    }
}