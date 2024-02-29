using Net.Component;
using Net.Event;

namespace GameCore
{
    public class Logger : NDebugInUnity
    {
        public bool EnableLog = true;
        
        public void Log(object message)
        {
            if (!EnableLog)
                return;
            NDebug.Log(message);
        }

        public void LogWarning(object message)
        {
            if (!EnableLog)
                return;
            NDebug.LogWarning(message);
        }

        public void LogError(object message)
        {
            if (!EnableLog)
                return;
            NDebug.LogError(message);
        }
    }
}