#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using UnityEngine;

namespace Net.Component
{
    public class NetworkManager : NetworkManagerBase
    {
        private static NetworkManager instance;
        public static NetworkManager Instance
        {
            get
            {
                if (instance == null)
                    instance = FindObjectOfType<NetworkManager>(true);
                return instance;
            }
        }
        public static NetworkManager I => Instance;
        public bool dontDestroyOnLoad = true;

        public override void Awake()
        {
            instance = this;
            if (dontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);
            Application.runInBackground = true;
        }
    }
}
#endif