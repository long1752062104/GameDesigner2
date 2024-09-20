namespace Net.Common
{
    /// <summary>
    /// 通用类单例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Singleton<T> where T : class, new()
    {
        protected static T instance;
        public static T Instance
        {
            get
            {
                if (instance == null)
                    instance = new T();
                return instance;
            }
        }
    }

#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
    /// <summary>
    /// Unity组件类单例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SingletonMono<T> : UnityEngine.MonoBehaviour where T : SingletonMono<T>
    {
        /// <summary>
        /// 由于第一次判断实例是否为空的时候, 如果直接使用Instance会进行查找类型, 单例被被赋值, 所以有必要的时候这个静态字段要用到
        /// </summary>
        protected static T instance;
        /// <summary>
        /// 单例实例
        /// </summary>
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
#if UNITY_2020_1_OR_NEWER
                    instance = FindObjectOfType<T>(true);
#else
                    var ts = UnityEngine.Resources.FindObjectsOfTypeAll<T>();
                    foreach (var t in ts)
                    {
                        if (t.gameObject.scene.isLoaded)
                        {
                            instance = t;
                            break;
                        }
                    }
#endif
                }
                return instance;
            }
        }
        /// <summary>
        /// 单例实例
        /// </summary>
        public static T I => Instance;
        public static T Singleton => Instance;

        protected virtual void Awake()
        {
            if (instance != null & instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this as T;
        }
    }
#endif
}