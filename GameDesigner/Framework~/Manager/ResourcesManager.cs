using System.Collections.Generic;
using System;
using UnityEngine;
using Object = UnityEngine.Object;
using System.IO;
using Cysharp.Threading.Tasks;

namespace Framework
{
    [Serializable]
    public class AssetBundleInfo
    {
        public string name;
        public string path;
        internal AssetBundle assetBundle;
        public AssetBundle AssetBundle => assetBundle;
    }

    public enum AssetBundleMode
    {
        /// <summary>
        /// 本机路径, 也就是编辑器路径
        /// </summary>
        LocalPath,
        /// <summary>
        /// 流路径, 不需要网络下载的模式
        /// </summary>
        StreamingAssetsPath,
        /// <summary>
        /// HFS服务器下载资源更新
        /// </summary>
        HFSPath,
    }

    public class ResourcesManager : MonoBehaviour
    {
#if UNITY_2020_1_OR_NEWER
        [NonReorderable]
#endif
        public List<AssetBundleInfo> assetBundleInfos = new List<AssetBundleInfo>();

        public virtual async UniTask InitAssetBundleInfos()
        {
            string abPath;
            if (Global.I.Mode == AssetBundleMode.StreamingAssetsPath)
                abPath = Application.streamingAssetsPath + "/";
            else if (Global.I.Mode == AssetBundleMode.HFSPath)
                abPath = Application.persistentDataPath + "/";
            else
                return;
            foreach (var info in assetBundleInfos)
            {
                if (File.Exists(abPath + info.path))
                {
                    if (info.assetBundle != null)
                        info.assetBundle.Unload(true);
                    info.assetBundle = await AssetBundle.LoadFromFileAsync(abPath + info.path);
                }
            }
        }

        /// <summary>
        /// 加载资源，遍历所有资源进行查找尝试加载资源， 如果成功则直接返回
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public virtual T LoadAsset<T>(string assetPath) where T : Object
        {
            T assetObj;
            foreach (var assetBundleInfo in assetBundleInfos)
            {
                if (assetBundleInfo.assetBundle == null)
                    continue;
                assetObj = assetBundleInfo.assetBundle.LoadAsset<T>(assetPath);
                if (assetObj != null)
                    return assetObj;
            }
#if UNITY_EDITOR
            assetObj = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (assetObj != null)
                return assetObj;
#endif
            var path = assetPath;
            var index = path.LastIndexOf('.');
            if (index >= 0)
                path = path.Remove(index, path.Length - index);
            index = path.IndexOf("Resources/");
            if (index >= 0)
                path = path.Remove(0, index + 10);
            assetObj = Resources.Load<T>(path);
            if (assetObj != null)
                return assetObj;
            throw new Exception("找不到资源:" + assetPath);
        }

        public T Instantiate<T>(string assetPath, Transform parent = null) where T : Object
        {
            var assetObj = LoadAsset<GameObject>(assetPath);
            if (assetObj == null)
            {
                Global.Logger.LogError($"资源加载失败:{assetPath}");
                return null;
            }
            var obj = Instantiate(assetObj, parent);
            if (typeof(T).IsSubclassOf(typeof(Component)))
                return obj.GetComponent(typeof(T)) as T;
            return obj as T;
        }
    }
}