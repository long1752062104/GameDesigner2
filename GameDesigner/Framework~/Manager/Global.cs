using Net.Component;
using UnityEngine;

namespace Framework
{
    [DefaultExecutionOrder(-50)]
    public partial class Global : SingleCase<Global>
    {
        public Camera MainCamera, UICamera;

        [SerializeField] private ResourcesManager resources;
        [SerializeField] private UIManager ui;
        [SerializeField] private AssetBundleCheckUpdate checkUpdate;
        [SerializeField] private TableManager table;
        [SerializeField] private SceneManager scene;
        [SerializeField] private new AudioManager audio;
        [SerializeField] private TimerManager timer;
        [SerializeField] private ConfigManager config;
        [SerializeField] private NetworkManager network;
        [SerializeField] private Logger logger;
        [SerializeField] private ObjectPool pool;

        public static ResourcesManager Resources => Instance.resources;
        public static UIManager UI => Instance.ui;
        public static AssetBundleCheckUpdate CheckUpdate => Instance.checkUpdate;
        public static TableManager Table => Instance.table;
        public static SceneManager Scene => Instance.scene;
        public static AudioManager Audio => Instance.audio;
        public static TimerManager Timer => Instance.timer;
        public static ConfigManager Config => Instance.config;
        public static NetworkManager Network => Instance.network;
        public static Logger Logger => Instance.logger;
        public static ObjectPool Pool => Instance.pool;

        void Awake()
        {
            if (instance != null)
            {
                DestroyImmediate(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}