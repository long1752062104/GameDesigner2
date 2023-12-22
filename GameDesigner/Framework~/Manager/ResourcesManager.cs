using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Cysharp.Threading.Tasks;

namespace Framework
{
    [Serializable]
    public class AssetBundleInfo
    {
        public string name;
        public string md5;
        public int fileSize;

        public AssetBundleInfo() { }

        public AssetBundleInfo(string name, string md5, int fileSize)
        {
            this.name = name;
            this.md5 = md5;
            this.fileSize = fileSize;
        }
    }

    public enum AssetBundleMode
    {
        /// <summary>
        /// 本机路径, 也就是编辑器路径
        /// </summary>
        EditorMode,
        /// <summary>
        /// 流路径, 不需要网络下载的模式
        /// </summary>
        LocalMode,
        /// <summary>
        /// HFS服务器下载资源更新
        /// </summary>
        ServerMode,
    }

    [Serializable]
    public class AssetInfo
    {
        public string name;
        public string path;
        public string assetBundleName;
    }

    public class ResourcesManager : MonoBehaviour
    {
#if UNITY_2020_1_OR_NEWER
        [NonReorderable]
#endif
        public Dictionary<string, AssetBundle> assetBundles = new Dictionary<string, AssetBundle>();
        public Dictionary<string, AssetInfo> assetInfos = new Dictionary<string, AssetInfo>();
        private AssetBundle manifestBundle;
        private AssetBundleManifest assetBundleManifest;
        public bool encrypt;
        public int password = 154789548;

        public virtual void Init()
        {
            if (Global.I.Mode == AssetBundleMode.EditorMode)
                return;
            var assetBundlePath = Global.I.AssetBundlePath;
            var assetInfoList = assetBundlePath + "AssetInfoList.json";
            if (!File.Exists(assetInfoList))
            {
                Debug.LogError("请构建AB包后再运行!");
                return;
            }
            var json = LoadAssetFileReadAllText(assetInfoList);
            var assetInfos = Newtonsoft_X.Json.JsonConvert.DeserializeObject<List<AssetInfo>>(json);
            foreach (var assetInfo in assetInfos)
                this.assetInfos.Add(assetInfo.path, assetInfo);
            var manifestBundlePath = $"{assetBundlePath}{Global.I.version}";
            manifestBundle = LoadAssetBundle(manifestBundlePath);
            assetBundleManifest = manifestBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        }

        public virtual string LoadAssetFileReadAllText(string assetPath)
        {
            var bytes = LoadAssetFile(assetPath);
            return Encoding.UTF8.GetString(bytes);
        }

        public virtual AssetBundle LoadAssetBundle(string assetBundlePath)
        {
            var bytes = LoadAssetFile(assetBundlePath);
            var assetBundle = AssetBundle.LoadFromMemory(bytes);
            return assetBundle;
        }

        public virtual byte[] LoadAssetFile(string assetPath)
        {
            var bytes = File.ReadAllBytes(assetPath);
            if (encrypt)
                Net.Helper.EncryptHelper.ToDecrypt(password, bytes);
            return bytes;
        }

        public virtual void OnDestroy()
        {
            manifestBundle?.Unload(true);
            if (assetBundles == null)
                return;
            foreach (var AssetBundleInfo in assetBundles)
                AssetBundleInfo.Value?.Unload(true);
        }

        /// <summary>
        /// 加载资源，遍历所有资源进行查找尝试加载资源， 如果成功则直接返回
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public virtual T LoadAsset<T>(string assetPath) where T : Object
        {
#if UNITY_EDITOR
            if (Global.I.Mode == AssetBundleMode.EditorMode)
                return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath);
#endif
            if (!assetInfos.TryGetValue(assetPath, out var assetInfoBase))
                return default;
            if (!assetBundles.TryGetValue(assetInfoBase.assetBundleName, out var assetBundle))
            {
                var fullPath = Global.I.AssetBundlePath + assetInfoBase.assetBundleName;
                if (File.Exists(fullPath))
                    assetBundles.Add(assetInfoBase.assetBundleName, assetBundle = LoadAssetBundle(fullPath));
                else return default;
                DirectDependencies(assetInfoBase.assetBundleName);
            }
            return assetBundle.LoadAsset<T>(assetPath);
        }

        public virtual async UniTask<T> LoadAssetAsync<T>(string assetPath) where T : Object
        {
#if UNITY_EDITOR
            if (Global.I.Mode == AssetBundleMode.EditorMode)
                return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath);
#endif
            if (!assetInfos.TryGetValue(assetPath, out var assetInfoBase))
                return default;
            if (!assetBundles.TryGetValue(assetInfoBase.assetBundleName, out var assetBundle))
            {
                var fullPath = Global.I.AssetBundlePath + assetInfoBase.assetBundleName;
                if (File.Exists(fullPath))
                    assetBundles.Add(assetInfoBase.assetBundleName, assetBundle = LoadAssetBundle(fullPath));
                else return default;
                DirectDependencies(assetInfoBase.assetBundleName);
            }
            var assetObject = await assetBundle.LoadAssetAsync<T>(assetPath) as T;
            return assetObject;
        }

        public virtual void LoadAssetScene(string assetPath)
        {
#if UNITY_EDITOR
            if (Global.I.Mode == AssetBundleMode.EditorMode)
                return;
#endif
            if (!assetInfos.TryGetValue(assetPath, out var assetInfoBase))
                return;
            if (!assetBundles.TryGetValue(assetInfoBase.assetBundleName, out var assetBundle))
            {
                var fullPath = Global.I.AssetBundlePath + assetInfoBase.assetBundleName;
                if (File.Exists(fullPath))
                    assetBundles.Add(assetInfoBase.assetBundleName, assetBundle = LoadAssetBundle(fullPath));
                else return;
                DirectDependencies(assetInfoBase.assetBundleName);
            }
        }

        protected virtual void DirectDependencies(string assetBundleName)
        {
            var dependencies = assetBundleManifest.GetDirectDependencies(assetBundleName);
            foreach (var dependencie in dependencies)
            {
                if (assetBundles.ContainsKey(dependencie))
                    continue;
                var fullPath = Global.I.AssetBundlePath + dependencie;
                var assetBundle = LoadAssetBundle(fullPath);
                assetBundles.Add(dependencie, assetBundle);
                DirectDependencies(dependencie);
            }
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

        /// <summary>
        /// 转移对象信息, 保留对象数据, 可能旧的ResourcesManager被销毁, 转移数据给新的ResourcesManager组件
        /// </summary>
        /// <param name="resources"></param>
        public void Change(ResourcesManager resources)
        {
            assetBundles = resources.assetBundles;
            assetInfos = resources.assetInfos;
            manifestBundle = resources.manifestBundle;
            assetBundleManifest = resources.assetBundleManifest;
            resources.assetBundles = null;
            resources.assetInfos = null;
            resources.manifestBundle = null;
            resources.assetBundleManifest = null;
        }
    }
}