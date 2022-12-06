using System.Collections.Generic;
using System;
using UnityEngine;
using Object = UnityEngine.Object;
using System.IO;

namespace Framework
{
    public enum AssetBundleType
    {
        None,
        Prefabs,
        UIPrefabs,
        Audio,
        Texture,
        UI,
        Font,
        Scene,
        All
    }

    [Serializable]
    public class AssetBundleInfo
    {
        public string name;
        public AssetBundleType type;
        public string path;
        internal AssetBundle assetBundle;

        public AssetBundle AssetBundle => assetBundle;
    }

    public enum AssetBundleMode
    {
        LocalPath,
        HFSPath,
    }

    public class ResourcesManager : MonoBehaviour
    {
        public AssetBundleMode Mode = AssetBundleMode.LocalPath;
#if UNITY_2020_1_OR_NEWER
        [NonReorderable]
#endif
        public List<AssetBundleInfo> assetBundleInfos = new List<AssetBundleInfo>();
        private readonly Dictionary<AssetBundleType, AssetBundleInfo> assetBundleDict = new Dictionary<AssetBundleType, AssetBundleInfo>();

        public void InitAssetBundleInfos()
        {
            string abPath;
            if (Mode == AssetBundleMode.LocalPath)
                abPath = Application.streamingAssetsPath + "/";
            else
                abPath = Application.persistentDataPath + "/";
            foreach (var info in assetBundleInfos)
            {
                assetBundleDict[info.type] = info;
                if (File.Exists(abPath + info.path))
                {
                    if (info.assetBundle != null)
                        info.assetBundle.Unload(true);
                    info.assetBundle = AssetBundle.LoadFromFile(abPath + info.path);
                }
            }
        }

        public T LoadAsset<T>(AssetBundleType type, string assetPath) where T : Object
        {
            if (assetBundleDict.TryGetValue(type, out var assetBundleInfo))
            {
                if (assetBundleInfo.assetBundle != null)
                {
                    var assetObj = assetBundleInfo.assetBundle.LoadAsset<T>(assetPath);
                    return assetObj;
                }
            }
            if (assetPath.Contains("Resources/"))
            {
                var path = assetPath.Split(new string[] { "Resources/" }, 0);
                var resPath = path[1].Split('.');
                var resObj = Resources.Load<T>(resPath[0]);
                return resObj;
            }
            throw new Exception("�Ҳ�����Դ:" + assetPath);
        }

        public GameObject Instantiate(string assetPath)
        {
            return Instantiate<GameObject>(assetPath, null);
        }

        public T Instantiate<T>(string assetPath) where T : Object
        {
            return Instantiate<T>(assetPath, null);
        }

        public T Instantiate<T>(string assetPath, Transform parent) where T : Object
        {
            var assetObj = LoadAsset<GameObject>(AssetBundleType.All, assetPath);
            if (assetObj == null)
            {
                Global.Logger.LogError($"��Դ����ʧ��:{assetPath}");
                return null;
            }
            var obj = Instantiate(assetObj, parent);
            if (typeof(T).IsSubclassOf(typeof(Component)))
                return obj.GetComponent<T>();
            return obj as T;
        }
    }
}