using Net.Component;
using System.Reflection;
using UnityEngine;

namespace GameCore
{
    [DefaultExecutionOrder(-50)]
    public class Global : SingleCase<Global>
    {
        [SerializeField] protected Camera uiCamera;
        [SerializeField] protected ResourcesManager resources;
        [SerializeField] protected UIManager ui;
        [SerializeField] protected AssetBundleCheckUpdate checkUpdate;
        [SerializeField] protected TableManager table;
        [SerializeField] protected SceneManager scene;
        [SerializeField] protected new AudioManager audio;
        [SerializeField] protected TimerManager timer;
        [SerializeField] protected ConfigManager config;
        [SerializeField] protected NetworkManager network;
        [SerializeField] protected Logger logger;
        [SerializeField] protected ObjectPool pool;
        [SerializeField] protected EventManager @event;
        [SerializeField] protected PlayerPrefsManager playerPrefs;

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
        public static EventManager Event;
        public static PlayerPrefsManager PlayerPrefs;

        public AssetBundleMode Mode = AssetBundleMode.EditorMode;
        public Platform platform;
        public string version = "1.0.0";
        public string entryRes;
        public bool compressionJson;
        [Tooltip("可寻址资源， 资源加载仅使用资源名，当你使用这个选项时，加载资源时不需要资源路径和后缀；不开启时，使用完整路径加载资源")]
        public bool addressables;
        public bool dontDestroyOnLoad = true;

        public static Camera UICamera { get => Instance.uiCamera; set => Instance.uiCamera = value; }
        public string AssetBundlePath
        {
            get
            {
                if (Mode == AssetBundleMode.LocalMode)
                    return $"{Application.streamingAssetsPath}/AssetBundles/{platform}/{version}/";
                if (Mode == AssetBundleMode.ServerMode)
                    return $"{Application.persistentDataPath}/AssetBundles/{platform}/{version}/";
                return string.Empty;
            }
        }
        public static Assembly HotfixAssembly { get; set; }

        protected override void Awake()
        {
            base.Awake();
            Initialize();
        }

        protected virtual void Initialize()
        {
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
            Event = @event;
            PlayerPrefs = playerPrefs;
            if (dontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// 当初始化完成，初始化包括检查热更新，文件下载等等
        /// </summary>
        public virtual void OnInit()
        {
            if (string.IsNullOrEmpty(entryRes))
                return;
            Resources.Instantiate<GameObject>(entryRes);
        }
    }
}