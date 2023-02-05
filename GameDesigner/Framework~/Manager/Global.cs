using Net.Component;
using UnityEngine;

namespace Framework
{
    [DefaultExecutionOrder(-50)]
    public partial class Global : SingleCase<Global>
    {
        [SerializeField] private Camera mainCamera, uiCamera;
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

        public static ResourcesManager Resources;
        public static UIManager UI;
        public static AssetBundleCheckUpdate CheckUpdate;
        public static TableManager Table;
        public static SceneManager Scene;
        public static AudioManager Audio;
        public static TimerManager Timer;
        public static ConfigManager Config;
        public static NetworkManager Network;
        public static Logger Logger;
        public static ObjectPool Pool;

        public static Camera MainCamera { get => Instance.mainCamera; set => Instance.mainCamera = value; }
        public static Camera UICamera { get => Instance.uiCamera; set => Instance.uiCamera = value; }

        protected override void Awake()
        {
            base.Awake();
            Resources = resources;
            UI = ui;
            CheckUpdate = checkUpdate;
            Table = table;
            Scene = scene;
            Audio = audio;
            Timer = timer;
            Config = config;
            Network = network;
            Logger = logger;
            Pool = pool;
            DontDestroyOnLoad(gameObject);
        }
    }
}