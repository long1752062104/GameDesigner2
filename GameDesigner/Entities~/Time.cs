
namespace Net.Entities
{
    public class Time
    {
        public static uint _time;
        public static uint _deltaTime = 1000 / 60;

        public static float time => _time * 0.001f;
        public static float deltaTime = 1f / 60f;

        public static void Update()
        {
            _time += _deltaTime;
        }
    }
}
