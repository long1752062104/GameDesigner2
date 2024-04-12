#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
namespace Net.Component
{
    using Net.Event;
    using UnityEngine;

    public abstract class NDebugInUnity : MonoBehaviour
    {
        public LogMode logMode = LogMode.Default;
        public WriteLogMode writeLogMode = WriteLogMode.None;

        protected virtual void Awake()
        {
#if !UNITY_EDITOR && UNITY_SERVER
            NDebug.BindLogAll(System.Console.WriteLine);
#else
            BindLog();
#endif
#if !UNITY_WEBGL
            NDebug.WriteFileMode = writeLogMode;
            if (writeLogMode != WriteLogMode.None)
                Application.logMessageReceivedThreaded += LogMessageReceivedHandler;
#endif
        }

#if !UNITY_WEBGL
        protected virtual void LogMessageReceivedHandler(string condition, string stackTrace, UnityEngine.LogType type)
        {
            if (writeLogMode == WriteLogMode.None)
                return;
            switch (type)
            {
                case UnityEngine.LogType.Error:
                    if (writeLogMode == WriteLogMode.Error | writeLogMode == WriteLogMode.WarnAndError | writeLogMode == WriteLogMode.All)
                        NDebug.WriteLog(condition + " : " + stackTrace);
                    break;
                case UnityEngine.LogType.Warning:
                    if (writeLogMode == WriteLogMode.Warn | writeLogMode == WriteLogMode.WarnAndError | writeLogMode == WriteLogMode.All)
                        NDebug.WriteLog(condition + " : " + stackTrace);
                    break;
                case UnityEngine.LogType.Log:
                    if (writeLogMode == WriteLogMode.Log | writeLogMode == WriteLogMode.All)
                        NDebug.WriteLog(condition + " : " + stackTrace);
                    break;
                default:
                    if (writeLogMode == WriteLogMode.All)
                        NDebug.WriteLog(condition + " : " + stackTrace);
                    break;
            }
        }
#endif

        private void BindLog()
        {
            switch (logMode)
            {
                case LogMode.Default:
                    NDebug.BindLogAll(Debug.Log, Debug.LogWarning, Debug.LogError);
                    break;
                case LogMode.LogAll:
                    NDebug.BindLogAll(Debug.Log);
                    break;
                case LogMode.LogAndWarning:
                    NDebug.BindLogAll(Debug.Log, Debug.Log, Debug.LogError);
                    break;
                case LogMode.WarnAndError:
                    NDebug.BindLogAll(Debug.Log, Debug.LogError, Debug.LogError);
                    break;
                case LogMode.OnlyError:
                    NDebug.BindLogAll(null, null, Debug.LogError);
                    break;
                case LogMode.OnlyWarnAndError:
                    NDebug.BindLogAll(null, Debug.LogError, Debug.LogError);
                    break;
            }
        }

        protected virtual void OnDestroy()
        {
            switch (logMode)
            {
                case LogMode.Default:
                    NDebug.RemoveLogAll(Debug.Log, Debug.LogWarning, Debug.LogError);
                    break;
                case LogMode.LogAll:
                    NDebug.RemoveLogAll(Debug.Log);
                    break;
                case LogMode.LogAndWarning:
                    NDebug.RemoveLogAll(Debug.Log, Debug.Log, Debug.LogError);
                    break;
                case LogMode.WarnAndError:
                    NDebug.RemoveLogAll(Debug.Log, Debug.LogError, Debug.LogError);
                    break;
                case LogMode.OnlyError:
                    NDebug.RemoveLogAll(null, null, Debug.LogError);
                    break;
                case LogMode.OnlyWarnAndError:
                    NDebug.RemoveLogAll(null, Debug.LogError, Debug.LogError);
                    break;
            }
#if !UNITY_WEBGL
            if (writeLogMode != WriteLogMode.None)
                Application.logMessageReceivedThreaded -= LogMessageReceivedHandler;
#endif
        }
    }
}
#endif