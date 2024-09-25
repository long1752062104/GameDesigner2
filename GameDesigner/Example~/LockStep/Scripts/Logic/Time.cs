using SoftFloat;

namespace LockStep
{
    public static class LSTime
    {
        public static sfloat time;
        public static sfloat deltaTime = 0.033f;//1 / 30(一秒30次) = 每秒0.033值
    }
}