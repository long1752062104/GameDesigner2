#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using Net.Event;
using UnityEngine;

namespace Net.Component
{
    public class NetworkLogger : MonoBehaviour
    {
        public LogMode logMode = LogMode.Default;
        public WriteLogMode writeLogMode = WriteLogMode.None;

        private void Awake()
        {
#if !UNITY_EDITOR && UNITY_SERVER
            NDebug.BindLogAll(System.Console.WriteLine);
#else
            BindLog();
#endif
            NDebug.WriteFileMode = writeLogMode;
            Application.logMessageReceivedThreaded += CaptureLogThread;
        }

        private void CaptureLogThread(string condition, string stackTrace, UnityEngine.LogType type)
        {
            var writeFileMode = NDebug.WriteFileMode;
            if (writeFileMode == WriteLogMode.None)
                return;
            switch (type)
            {
                case UnityEngine.LogType.Error:
                    if (writeFileMode == WriteLogMode.Error | writeFileMode == WriteLogMode.WarnAndError | writeFileMode == WriteLogMode.All)
                        NDebug.WriteLog(condition + " : " + stackTrace);
                    break;
                case UnityEngine.LogType.Warning:
                    if (writeFileMode == WriteLogMode.Warn | writeFileMode == WriteLogMode.WarnAndError | writeFileMode == WriteLogMode.All)
                        NDebug.WriteLog(condition + " : " + stackTrace);
                    break;
                case UnityEngine.LogType.Log:
                    if (writeFileMode == WriteLogMode.Log | writeFileMode == WriteLogMode.All)
                        NDebug.WriteLog(condition + " : " + stackTrace);
                    break;
                default:
                    if (writeFileMode == WriteLogMode.All)
                        NDebug.WriteLog(condition + " : " + stackTrace);
                    break;
            }
        }

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

        void OnDestroy()
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
        }
    }
}
#endif