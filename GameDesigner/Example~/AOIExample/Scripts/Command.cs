#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
namespace AOIExample
{
    public class Command : Net.Component.Command
    {
        public const byte EnterArea = 151;
        public const byte ExitArea = 152;
        public const byte RobotUpdate = 153;
    }
}
#endif